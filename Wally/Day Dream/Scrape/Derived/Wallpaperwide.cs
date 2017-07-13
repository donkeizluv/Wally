using System.Collections.Generic;
using HtmlAgilityPack;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap14 : Scraper
    {
        private const string HomePage = "http://wallpaperswide.com";
        private const string LinkNodes = "//div[@class='thumb']/a";
        private const string ThumbNodes = "//div[@class='thumb']/a/img";
        private const string ResNodes = "//*[@id='wallpaper-resolutions']/a";
        private const string MaxRndNode = "//div[@class = 'pagination']/a";

        public override string SiteName => "Wallpaperswide";

        //set this through UpdateMaxRnd(int)
        public override int MaxRnd { get; protected set; } = 5621;

        protected override string RndUrlTemplate => "http://wallpaperswide.com/latest_wallpapers/page/@";

        public override bool IsJpgUrlNeedParsed { get; } = false;

        public override string JpgLink(string html)
        {
            return html;
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ResNodes);
            var resList = new List<ResolutionCapsule>();
            foreach (var resNumber in nodes)
            {
                if (!resNumber.Attributes["title"].Value.Contains("4:3") &&
                    !resNumber.Attributes["title"].Value.Contains("16:10") &&
                    !resNumber.Attributes["title"].Value.Contains("16:9"))
                    continue;
                if (resNumber.InnerText.ConvertToPixel() <= 1280*720) continue;
                var aninfo = new ResolutionCapsule
                {
                    ResolutionValue = resNumber.InnerText,
                    ResolutionUrl = HomePage + resNumber.Attributes["href"].Value
                };
                resList.Add(aninfo);
                if (resList.Count >= 16)
                    return resList;
            }
            return resList.Count < 1 ? null : resList;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var info = new List<PictureData>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var urlNodes = doc.DocumentNode.SelectNodes(LinkNodes);
            var thumbs = doc.DocumentNode.SelectNodes(ThumbNodes);
            int i = 0;
            if (urlNodes == null || thumbs == null) return info;
            foreach (var node in urlNodes)
            {
                var anInfo = new PictureData(this)
                {
                    ThumbUrl = thumbs[i].Attributes["src"].Value,
                    PageUrl = HomePage + node.Attributes["href"].Value
                };
                info.Add(anInfo);
                i++;
            }
            ThumbPerPage = info.Count;
            return info.Count < 1 ? null : info;
        }

        public override void UpdateMaxRnd(string html)
        {
            UpdateMaxRnd(ExtractMaxRnd(html, MaxRndNode));
        }
    }
}