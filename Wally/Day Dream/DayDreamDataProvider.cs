using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wally.Day_Dream.Helpers;
using Wally.Day_Dream.Interfaces;
using Wally.Day_Dream.Scrape;

namespace Wally.Day_Dream
{
    //aka random PictureData provider
    internal class DayDreamDataProvider : IPictureDataProvider
    {
        private readonly IDownloader _downloader = new Downloader();

        private readonly Stack<PictureData> _pictureDataStack = new Stack<PictureData>();

        public bool CanSupplyWhenFail => true;

        public async Task<PictureData> GetData()
        {
            if (_pictureDataStack.Count == 0)
                await FetchRandomThumbs().ConfigureAwait(false);
            return _pictureDataStack.Pop();
        }

        public event EventHandler DownloadRandomPagesCompleted;
        private void RaiseDownloadRandomPagesCompleted() => DownloadRandomPagesCompleted?.Invoke(null, EventArgs.Empty);

        //public static PictureData LastFinishedDownloadingThumb
        //{
        //    get; set;
        //}

        public static void Innit()
        {
            Scraper.InstanciateAllDerivedTypes();
        }

        public async Task<List<PictureData>> GetData(int count)
        {
            if (_pictureDataStack.Count < count)
                await FetchRandomThumbs().ConfigureAwait(false);
            var list = new List<PictureData>();
            for (int i = 0; i < count; i++)
            {
                list.Add(_pictureDataStack.Pop());
            }
            return list;
        }

        private async Task FetchRandomThumbs()
        {
            if (!Scraper.IsInitiated)
                Scraper.InstanciateAllDerivedTypes();
            var tasks = Scraper.InstanciatedScrapers.Select(FetchDataAsync).ToList();
            //wait all tasks to complete in order to shuffle the stack
            foreach (var result in await Task.WhenAll(tasks).ConfigureAwait(false))
            {
                if (result == null) continue; //skip this scraper
                foreach (var item in result)
                {
                    _pictureDataStack.Push(item);
                }
            }
            _pictureDataStack.Shuffle();
            RaiseDownloadRandomPagesCompleted();
        }

        private async Task<List<PictureData>> FetchDataAsync(Scraper scraper)
        {
            try
            {
                //if (scraper is Scrap14) throw new Exception();
                string html = (await _downloader.OpenReadAsync(scraper.GetRandomPageUrl()).ConfigureAwait(false)).
                    ConvertToString();
                scraper.UpdateMaxRnd(html); //try updating Max Rnd value
                var list = scraper.ExtractImages(html).ToList();
                if (list.Count < 1)
                    ExManager.Ex(new InvalidOperationException("Parsed list is 0 in length."));
                return list;
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
                return null;
            }
        }
    }
}