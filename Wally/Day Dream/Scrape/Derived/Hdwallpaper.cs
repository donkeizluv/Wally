using HtmlAgilityPack;
using System.Collections.Generic;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap4 : Scraper
    {
        private const string HomePage = "http://www.hdwallpapers.in";
        private const string LinkNodes = "//div[@class='thumbbg']/div[@class='thumb']/a";
        private const string ThumbNodes = "//div[@class='thumbbg']/div[@class='thumb']/a/img";
        private const string ResNodes = "//div[@class='content']/div[@class='wallpaper-resolutions']/a";
        private const string MaxRndNode = "//div[@class = 'pagination']/a";

        public override string SiteName => "HDwallpapers";

        public override int MaxRnd { get; protected set; } = 767;

        protected override string RndUrlTemplate => "http://www.hdwallpapers.in/latest_wallpapers/page/@";

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
            if (nodes == null) return null;
            var resList = new List<ResolutionCapsule>();
            foreach (var node in nodes)
            {
                if (!node.InnerText.Replace(" ", null).IsResolution()) continue;
                var aninfo = new ResolutionCapsule
                {
                    ResolutionValue = node.InnerText.Replace(" ", null),
                    ResolutionUrl = HomePage + node.Attributes["href"].Value
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
                    ThumbUrl = HomePage + thumbs[i].Attributes["src"].Value,
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