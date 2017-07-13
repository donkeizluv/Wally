using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap6 : Scraper
    {
        private const string Homepage = "https://www.pexels.com";
        private const string ThumbNode = "//div[@class='photos']/article/a";

        private const string ResNode =
            "//ul[@class='select-list']/li[@class='select-list__item select-list--active']/input";

        public override string SiteName => "Pexels";

        //set it through UpdateMaxRnd(int) to avoid repetive calling of IsMaxRandomUpdated, MaxRnd = x
        public override int MaxRnd { get; protected set; } = 2560;

        protected override string RndUrlTemplate => "https://www.pexels.com/?format=&page=@";

        public override bool IsJpgUrlNeedParsed { get; } = false;

        public override void UpdateMaxRnd(string html)
        {
        }

        public override string JpgLink(string html)
        {
            throw new NotImplementedException();
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
                    ResolutionUrl = node.Attributes["data-alt-url"].Value,
                    ResolutionValue = node.Attributes["value"].Value
                });
            }
            return list.Count < 1 ? null : list;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ThumbNode);
            var list = nodes.Select(node => new PictureData(this)
            {
                PageUrl = Homepage + node.Attributes["href"].Value,
                ThumbUrl = node.Element("img").Attributes["src"].Value
            }).ToList();
            ThumbPerPage = list.Count;
            return list.Count < 1 ? null : list;
        }
    }
}