using System.Collections.Generic;
using System.Linq;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap11 : Scraper
    {
        private const string Homepage = "http://wallpaperfx.com";
        private const string MaxRndNode = "//div[@class='pcontent']/div[@class='wpagination']/div[@class='number']/a";
        private const string ImagesNode = "//div[@class='pcontent']/ul[@class='wallpapers']/li/a";
        private const string ResNode = "//div[@class='wallpaperinfo']/ul[@class='wallpaper-resolutions']/li/a";
        private const string JpgNode = "//div[@class='text-center']/a/img[@id='wallpaper']";
        public override string SiteName => "Wallpaperfx";

        //set it through UpdateMaxRnd(int) to avoid repetive calling of IsMaxRandomUpdated, MaxRnd = x
        public override int MaxRnd { get; protected set; } = 702;

        protected override string RndUrlTemplate => "http://wallpaperfx.com/latest_wallpapers/page-@";

        public override bool IsJpgUrlNeedParsed { get; } = true;

        public override void UpdateMaxRnd(string html)
        {
            UpdateMaxRnd(ExtractMaxRnd(html, MaxRndNode));
        }

        public override string JpgLink(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectSingleNode(JpgNode);
            return Homepage + node.Attributes["src"].Value;
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ResNode);
            var list = new List<ResolutionCapsule>();
            foreach (var node in nodes)
            {
                list.Add(new ResolutionCapsule
                {
                    ResolutionUrl = Homepage + node.Attributes["href"].Value,
                    ResolutionValue = node.InnerText
                });
            }
            return list;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ImagesNode);
            var list = nodes.Select(node => new PictureData(this)
            {
                PageUrl = Homepage + node.Attributes["href"].Value,
                ThumbUrl = Homepage + node.Element("img").Attributes["src"].Value,
                WallpaperName = node.Element("img").Attributes["alt"].Value
            }).ToList();
            ThumbPerPage = list.Count;
            return list.Count < 1 ? null : list;
        }
    }
}