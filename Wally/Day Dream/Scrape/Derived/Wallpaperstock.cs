using System.Collections.Generic;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap13 : Scraper
    {
        private const string HomePage = "http://wallpaperstock.net/";
        //div[contains(concat(' ', @class, ' '), ' wallpaper_thumb ')]
        //this will match something like "xyz wallpaper_thumb xyz" but not "adswallpaper_thumb"
        private const string LinkNodes =
            "//div[contains(concat(' ', @class, ' '), ' wallpaper_thumb ')]/div[@class='links']/a";

        private const string ThumbNodes = "//div[contains(concat(' ', @class, ' '), ' wallpaper_thumb ')]/div/a/img";
        private const string ResNodes = "//div[@class='details_more']/div/div[@class='resolutions']/a";
        private const string Jpgnode = "//tr/td/div/img";
        private const string MaxRndNode = "//div[@class = 'pagination']/a";

        public override string SiteName => "Wallpaperstock";

        public override int MaxRnd { get; protected set; } = 4132;

        protected override string RndUrlTemplate => "http://wallpaperstock.net/wallpapers_p@.html";

        public override bool IsJpgUrlNeedParsed { get; } = true;

        public override string JpgLink(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectSingleNode(Jpgnode);
            return node?.Attributes["src"].Value;
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ResNodes);
            if (nodes == null) return null;
            var resList = new List<ResolutionCapsule>();
            foreach (var node in nodes)
            {
                if (!node.InnerText.IsResolution())
                {
                    break;
                }
                var aninfo = new ResolutionCapsule
                {
                    ResolutionValue = node.InnerText,
                    ResolutionUrl = node.Attributes["href"].Value
                };
                resList.Add(aninfo);
            }
            return resList.Count < 1 ? null : resList;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var info = new List<PictureData>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var links = doc.DocumentNode.SelectNodes(LinkNodes);
            var thumbs = doc.DocumentNode.SelectNodes(ThumbNodes);
            int i = 0;
            if (links == null || thumbs == null) return info;
            foreach (var node in links)
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