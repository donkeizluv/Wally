using System;
using System.Threading;
using System.Linq;
using Wally.Day_Dream;
using Wally.Day_Dream.Helpers;
using Wally.Day_Dream.Scrape;

namespace Wally_diagnostic
{
    partial class Program
    {
        const int NumberOfThumb2DownloadForEachParser = 1;
        const int NumberOfResList2DownloadForEachParser = 1;

        private static bool InnitParser()
        {
            Scraper.InstanciateAllDerivedTypes();
            return true;
        }

        private static readonly Random Rnd = new Random(Guid.NewGuid().GetHashCode());

        static bool TestDownloadRndPage()
        {
            Console.WriteLine(@"[Dl rnd pages]");
            foreach (var parser in Scraper.InstanciatedScrapers)
            {
                var webClient = new WebDownload();
                var capsule = new DataCapsule();
                string link = parser.GetRandomPageUrl();
                string html = string.Empty;
                try
                {
                    html = webClient.DownloadString(link);

                    //if (parser.SiteName == "Goodfon")
                    //    html = string.Empty;

                    if (string.IsNullOrEmpty(html))
                        throw new Exception(@"       html string is null or zero in length at: " + parser.SiteName);
                }
                catch (Exception ex)
                {
                    PrintColoredTextOnce(@"     Site: " + parser.SiteName);
                    if (ex.Message.Length > 0)
                        PrintColoredTextOnce(@"     " + ex.Message);
                    else
                        PrintColoredTextOnce(@"     Failed to download random page");

                    continue;
                }

                capsule.Scraper = parser;
                capsule.RndPageHtml = html;
                _capsuleList.Add(capsule);
            }
            return true;
        }

        static void TestUpdateMaxRnd()
        {
            Console.WriteLine(@"[Updating MaxRnd]");
            foreach (var capsule in _capsuleList)
            {
                try
                {
                    Console.Write(@"     " + capsule.Scraper.SiteName + ": " + capsule.Scraper.MaxRnd + " -> ");
                    capsule.Scraper.UpdateMaxRnd(capsule.RndPageHtml);
                    Console.WriteLine(capsule.Scraper.MaxRnd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Failed to update MaxRnd at: " + capsule.Scraper.SiteName + ". " + ex.Message);
                }
            }
        }

        static bool TestParseRndPage()
        {
            Console.WriteLine(@"[Parsing rnd pages for info]");
            int i = 0;

            for (; i < _capsuleList.Count; i++)
            {
                try
                {
                    var list = _capsuleList[i].Scraper.ExtractImages(_capsuleList[i].RndPageHtml);

                    //if (i == 2)
                    //{
                    //    _capsuleList[i].Fucked();
                    //    continue;
                    //}

                    if (list.Count < 1)
                    {
                        PrintColoredTextOnce(@"     No info in rnd page at: " + _capsuleList[i].Scraper.SiteName);
                        _capsuleList[i].Fucked();
                        continue;
                    }
                    _capsuleList[i].ImgInfoList = list;
                }
                catch
                {
                    PrintColoredTextOnce(@"     Failed: " + _capsuleList[i].Scraper.SiteName);
                    _capsuleList[i].Fucked();
                }
            }
            return i != 0;
        }

        static int GetNumberOfWorkingParsers()
        {
            var workingParsers = from parser in _capsuleList
                where !parser.IsNotWorking
                select parser;
            return workingParsers.Count();
        }

        static bool TestDownloadThumb()
        {
            Console.WriteLine(@"[Downloading thumbs]");
            int max = NumberOfThumb2DownloadForEachParser*GetNumberOfWorkingParsers();
            int downloaded = 0;
            foreach (var capsule in _capsuleList)
            {
                if (capsule.IsNotWorking)
                {
                    PrintColoredTextOnce(@"     " + capsule.Scraper.SiteName + @" isnt working.... move on");
                    continue;
                }
                for (int i = 0; i < NumberOfThumb2DownloadForEachParser; i++)
                {
                    if (capsule.ImgInfoList.Count == 0)
                    {
                        PrintColoredTextOnce(@"     No info in: " + capsule.Scraper.SiteName);
                        continue;
                    }
                    var getData = new GetDataAsync(capsule.ImgInfoList[i].ThumbUrl);
                    getData.DownloadComplete +=
                        (sender, e) =>
                            getData_OpenReadCompleted_Thumb(sender, e, ref downloaded, max,
                                capsule.ImgInfoList[i].ThumbUrl);
                }
            }
            rsEvent.WaitOne(); //wait for Set()
            Console.WriteLine(@"     Total thumbs downloaded: " + DataCapsule.ThumbsDownloaded);
            return true;
        }

        private static AutoResetEvent rsEvent = new AutoResetEvent(false);

        private static void getData_OpenReadCompleted_Thumb(object sender, System.Net.OpenReadCompletedEventArgs e,
            ref int processed, int max, string thumbUrl)
        {
            try
            {
                var bitmap = e.Result.ConvertToBitmap();
                if (bitmap == null)
                    throw new Exception();
                bitmap.Dispose();
                DataCapsule.ThumbDownloadedSuccessfull();
            }
            catch
            {
                PrintColoredTextOnce(@"     Failed at: " + thumbUrl);
            }
            finally
            {
                processed++;
                if (processed >= max)
                    rsEvent.Set();
            }
        }

        static bool TestDownloadResList()
        {
            Console.WriteLine(@"[Downloading resolution page]");
            int downloaded = 0;
            int max = NumberOfResList2DownloadForEachParser*GetNumberOfWorkingParsers();
            foreach (var capsule in _capsuleList)
            {
                if (capsule.IsNotWorking)
                {
                    PrintColoredTextOnce(@"     " + capsule.Scraper.SiteName + @" isnt working.... move on");
                    continue;
                }
                for (int i = 0; i < NumberOfResList2DownloadForEachParser; i++)
                {
                    var getData = new GetDataAsync(capsule.ImgInfoList[i].PageUrl);
                    getData.DownloadComplete +=
                        (sender, e) =>
                            getData_OpenReadCompleted_ResList(sender, e, ref downloaded, max, capsule.ImgInfoList[i],
                                capsule);
                }   
            }
            rsEvent.WaitOne(); //wait for Set()
            Console.WriteLine(@"     Total res page downloaded: " + DataCapsule.ResPageDownloaded);
            return true;
        }

        private static void getData_OpenReadCompleted_ResList(object sender, System.Net.OpenReadCompletedEventArgs e,
            ref int downloaded, int max, PictureData info, DataCapsule d)
        {
            try
            {
                var s = e.Result.ConvertToString();
                d.ResPageHTMLList.Add(s);
                DataCapsule.ResPageDownloadedSuccessfull();
            }
            catch
            {
                PrintColoredTextOnce(@"     Failed at: " + new Uri(info.PageUrl).Host);
            }
            finally
            {
                downloaded++;
                if (downloaded >= max)
                    rsEvent.Set();
            }
        }

        private static bool TestParseResList()
        {
            Console.WriteLine(@"[Parsing resolution pages for res links]");
            int count = 0;
            foreach (var capsule in _capsuleList)
            {
                if (capsule.IsNotWorking)
                {
                    PrintColoredTextOnce(@"     " + capsule.Scraper.SiteName + @" isnt working.... move on");
                    continue;
                }
                foreach (var html in capsule.ResPageHTMLList)
                {
                    var resList = capsule.Scraper.ExtractResolutions(html);
                    if (resList.Count < 1)
                    {
                        PrintColoredTextOnce(@"     Got none res info at: " + capsule.Scraper.SiteName);
                        continue;
                    }
                    foreach (var l in resList)
                    {
                        if (!DataCapsule.IsRes(l.ResolutionValue))
                            PrintColoredTextOnce(@"     Res is not valid at: " + capsule.Scraper.SiteName + " value: " + l.ResolutionValue);
                    }
                    capsule.ListOfResList.Add(resList);
                    count += resList.Count;
                }
            }
            //or
            //int test = 0;
            //dataList.ForEach(a => a.ListOfResList.ForEach(b => test += b.Count));
            Console.WriteLine(@"     Parsed res: " + count);
            return true;
        }

        private static bool TestDownloadWallpaper()
        {
            rsEvent.Reset();
            Console.WriteLine(@"[Downloading wallpapers(1 random wallpapers per parser) ]");
            int thread = 0;
            int doneThread = 0;
            int maxThreads = 1;
            int downloaded = 0;
            foreach (var capsule in _capsuleList)
            {
               if (capsule.IsNotWorking)
                {
                    PrintColoredTextOnce(@"     " + capsule.Scraper.SiteName + @" isnt working.... move on");
                    continue;
                }
                Console.WriteLine(@"     Now downloading at " + capsule.Scraper.SiteName);
                foreach (var list in capsule.ListOfResList)
                {
                    thread++;
                    for (int i = 0; i < 1; i++) //take 1 time each ResList
                    {
                        string wallpaperLink = list[Rnd.Next(list.Count)].ResolutionUrl;
                        if (capsule.Scraper.IsJpgUrlNeedParsed)
                        {
                            var w = new WebDownload();
                            wallpaperLink =
                                capsule.Scraper.JpgLink(w.DownloadString(new Uri(Extentions.HandleWeirdFormat(wallpaperLink))));
                        }
                        var getData = new GetDataAsync(wallpaperLink);
                        getData.DownloadComplete +=
                            (sender, e) =>
                                getData_DownloadWallpaperCompleted(sender, e, wallpaperLink, ref doneThread, maxThreads,
                                    ref downloaded);
                    }
                    if (thread != maxThreads) continue;
                    rsEvent.WaitOne();
                    thread = 0;
                    break;
                    //rsEvent.WaitOne();
                }
            }
            int numberOfInfoList = 0;
            _capsuleList.ForEach(a => numberOfInfoList += a.ListOfResList.Count);
            Console.WriteLine(@"Downloaded wallpapers: " + downloaded + "/" + numberOfInfoList);
            return true;
        }

        private static void getData_DownloadWallpaperCompleted(object sender, System.Net.OpenReadCompletedEventArgs e,
            string link, ref int doneThread, int maxThread, ref int downloaded)
        {
            try
            {
                var bitmap = e.Result.ConvertToBitmap();
                if (bitmap == null)
                    throw new Exception();
                bitmap.Dispose();
                Interlocked.Increment(ref downloaded);
            }
            catch
            {
                PrintColoredTextOnce(@"     Failed at: " + link);
            }
            finally
            {
                doneThread++;
                if (doneThread >= maxThread)
                {
                    doneThread = 0;
                    rsEvent.Set();
                }
            }
        }

        private static void PrintColoredTextOnce(string text, ConsoleColor color = ConsoleColor.Red)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}