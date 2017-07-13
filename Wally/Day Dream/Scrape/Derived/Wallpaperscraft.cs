using System.Collections.Generic;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap12 : Scraper
    {
        private const string HomePage = @"https://wallpaperscraft.com";
        private const string MaxRndNode = @"//div[@class='pages']/a[@class='page_select']";
        private const string LinkNodes = @"//div[@class='wallpaper_pre']/a";
        private const string ThumbNodes = @"//div[@class='wallpaper_pre']/a/img";
        private const string ResNodes = @"//div[@class='wb_resolution']/div[@class='wb_res_cat_raz']";
        private const string JpgNodes = @"//div[@class='wb_preview']/a/img";

        public override string SiteName => "Wallpaperscraft";

        public override int MaxRnd { get; protected set; } = 6647;

        protected override string RndUrlTemplate => @"https://wallpaperscraft.com/all/page@";

        public override bool IsJpgUrlNeedParsed { get; } = true;

        public override void UpdateMaxRnd(string html)
        {
            UpdateMaxRnd(ExtractMaxRnd(html, MaxRndNode));
        }

        public override string JpgLink(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectSingleNode(JpgNodes);
            if (node == null) return null;
            return node.Attributes["src"].Value;
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var resNode = doc.DocumentNode.SelectSingleNode(ResNodes);
            var resList = new List<ResolutionCapsule>();
            if (resNode == null) return null;
            foreach (var element in resNode.Elements("a"))
            {
                var aninfo = new ResolutionCapsule
                {
                    ResolutionValue = element.InnerText,
                    ResolutionUrl = element.Attributes["href"].Value
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
                    PageUrl = node.Attributes["href"].Value
                };
                info.Add(anInfo);
                i++;
            }
            ThumbPerPage = info.Count;
            return info.Count < 1 ? null : info;
        }
    }
}