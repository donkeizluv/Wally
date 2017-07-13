using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Wally.Day_Dream.Helpers;
using Wally.Day_Dream.Scrape;

namespace Wally.Day_Dream
{
    internal struct ResolutionCapsule
    {
        public string ResolutionUrl;
        public string ResolutionValue;
    }

    internal class PictureData
    {
        private readonly IDownloader _downloader = new Downloader();

        public PictureData(Scraper scraper)
        {
            Scraper = scraper;
            _downloader.ProgressChanged += (s, e) => { DownloadProgressChanged?.Invoke(this, e); };
        }

        public bool? IsFavorite { get; set; } = null;
        public string PageUrl { get; set; }
        public string ThumbUrl { get; set; }
        public Scraper Scraper { get; set; }
        public string WallpaperName { get; set; }
        public IEnumerable<ResolutionCapsule> Resolutions { get; set; }

        public Bitmap ThumbBitmap { get; private set; }

        public Bitmap WallpaperBitmap { get; set; }
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        private static IEnumerable<ResolutionCapsule> SortAndTrim(IEnumerable<ResolutionCapsule> resList)
        {
            var sorted = resList.OrderByDescending(o => o.ResolutionValue.ConvertToPixel()).ToList();
            //16 is max res labels count
            for (int i = 0; i < sorted.Count - 16 + i; i++) //im so smart...
                sorted.RemoveAt(i);
            return sorted;
        }

        public async Task<Bitmap> GetThumbBitmap()
        {
            try
            {
                //if (Scraper is Scrap1)
                //    throw new Exception();
                var stream = await _downloader.OpenReadAsync(ThumbUrl).ConfigureAwait(false);
                ThumbBitmap = stream.ConvertToBitmap();
                return ThumbBitmap;
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
                return null;
            }
        }

        public async Task<IEnumerable<ResolutionCapsule>> GetResolutions()
        {
            try
            {
                if (Resolutions == null)
                {
                    string html = (await _downloader.DownloadDataAsync(PageUrl).
                        ConfigureAwait(false)).ConvertToString();
                    Resolutions = SortAndTrim(Scraper.ExtractResolutions(html));
                }
                if (Resolutions.ToList().Count < 1)
                    throw new Exception("Parse res list is 0 in length");
                return Resolutions;
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
                return null;
            }
        }
    }
}