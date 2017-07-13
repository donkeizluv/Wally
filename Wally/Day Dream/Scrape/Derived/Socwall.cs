using System;
using System.Collections.Generic;
using System.Linq;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap7 : Scraper
    {
        private const string Homepage = "http://www.socwall.com/";
        private const string MaxRndNode = "//ul[@class='pagination']/li[@class='pageNumber']/a";
        private const string ImagesNode = "//ul[@class='wallpaperList']/li/div/a";
        private const string ResNode = "//div[@class='wallpaperMeta']/p/a[@class='download']";
        public override string SiteName => "Socwall";

        //set it through UpdateMaxRnd(int) to avoid repetive calling of IsMaxRandomUpdated, MaxRnd = x
        public override int MaxRnd { get; protected set; } = 703;

        protected override string RndUrlTemplate => "http://www.socwall.com/wallpapers/page:@/";

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
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ResNode);
            var list = new List<ResolutionCapsule>();
            foreach (var node in nodes)
            {
                list.Add(new ResolutionCapsule
                {
                    ResolutionUrl = Homepage + node.Attributes["href"].Value,
                    ResolutionValue =
                        node.Elements("span").First(span => span.Attributes["class"].Value == "resolution").InnerText
                });
            }
            return list.Count < 1 ? null : list;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ImagesNode);
            var list = nodes.Select(node => new PictureData(this)
            {
                PageUrl = Homepage + node.Attributes["href"].Value,
                ThumbUrl = Homepage + node.Element("img").Attributes["src"].Value
            }).ToList();
            ThumbPerPage = list.Count;
            return list.Count < 1 ? null : list;
        }
    }
}