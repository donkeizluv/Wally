using System;
using System.Collections.Generic;
using System.Linq;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap1 : Scraper
    {
        private const string HomePage = @"http://wall.alphacoders.com/";
        private const string MaxRndNode = @"//ul[@class='pagination']/li/a";
        private const string ThumbnLinkNodes = @"//div[@class='boxgrid']/a[1]";
        private const string ResNode = @"//span[@class='btn btn-success download-button']";
        private const string JpgNode = @"//img[@id='main_wallpaper']";
        public override string SiteName => "Alphacoders";

        //updated 7/8/15
        public override int MaxRnd { get; protected set; } = 15446;

        protected override string RndUrlTemplate => "http://wall.alphacoders.com/newest_wallpapers.php?page=@";

        public override bool IsJpgUrlNeedParsed { get; } = false;

        public override void UpdateMaxRnd(string html)
        {
            UpdateMaxRnd(ExtractMaxRnd(html, MaxRndNode));
        }

        public override string JpgLink(string html)
        {
            throw new NotImplementedException();
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            //return null;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodeValue = doc.DocumentNode.SelectSingleNode(ResNode);
            //var nodeJpg = doc.DocumentNode.SelectSingleNode(JpgNode);
            var list = new List<ResolutionCapsule>(1); //TODO: why???
            list.Add(new ResolutionCapsule
            {
                ResolutionValue = nodeValue.InnerText.Trim().Split(' ').Last(),
                ResolutionUrl = nodeValue.Attributes["data-href"].Value
                //ResolutionUrl = nodeJpg.Attributes["src"].Value
            });
            return list.Count < 1 ? null : list;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ThumbnLinkNodes);
            if (nodes == null) return null;
            var info = new List<PictureData>();
            foreach (var child in nodes)
            {
                var anInfo = new PictureData(this)
                {
                    PageUrl = HomePage + child.Attributes["href"].Value,
                    ThumbUrl = child.Element("img").Attributes["src"].Value
                };
                info.Add(anInfo);
            }
            ThumbPerPage = info.Count;
            return info.Count < 1 ? null : info;
        }
    }
}