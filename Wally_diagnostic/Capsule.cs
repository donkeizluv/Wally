using System.Collections.Generic;
using Wally.Day_Dream;
using Wally.Day_Dream.Scrape;

namespace Wally_diagnostic
{
    partial class Program
    {
        private class DataCapsule
        {
            public DataCapsule()
            {
                ImgInfoList = new List<PictureData>();
                ResPageHTMLList = new List<string>();
                ListOfResList = new List<List<ResolutionCapsule>>();
                IsNotWorking = false;
                ThumbsDownloaded = 0;
                ResPageDownloaded = 0;
            }

            public Scraper Scraper;
            public string RndPageHtml; //test runs once so no need for list
            public List<PictureData> ImgInfoList;
            public List<string> ResPageHTMLList;
            public List<List<ResolutionCapsule>> ListOfResList;
            public bool IsNotWorking { get; private set; }
            public static int ThumbsDownloaded { get; private set; } = 0;
            public static void ThumbDownloadedSuccessfull()
            {
                ThumbsDownloaded++;
            }
            public static int ResPageDownloaded { get; private set; } = 0;
            public static void ResPageDownloadedSuccessfull()
            {
                ResPageDownloaded++;
            }
            public static bool IsRes(string s)
            {
                int vRes;
                int hRes;
                var splited = s.Trim().Split('x');
                return int.TryParse(splited[0], out hRes) && int.TryParse(splited[1], out vRes);
            }

            public void Fucked()
            {
                IsNotWorking = true;
            }
        }
        static List<DataCapsule> _capsuleList = new List<DataCapsule>();
    }
}
