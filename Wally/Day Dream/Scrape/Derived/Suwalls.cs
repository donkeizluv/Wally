using System;
using System.Collections.Generic;
using System.Linq;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap8 : Scraper
    {
        private const string Homepage = "https://suwalls.com";
        private const string ThumbNodes = @"//ul[@class='wpl']/li/div[@class='i']/a";
        private const string ResNodes = @"//div/a[@class='dlink']";
        public override string SiteName => "Suwalls";

        //set it through UpdateMaxRnd(int) to avoid repetive calling of IsMaxRandomUpdated, MaxRnd = x
        public override int MaxRnd { get; protected set; } = 1373;

        protected override string RndUrlTemplate => "https://suwalls.com/@.html";

        public override bool IsJpgUrlNeedParsed { get; } = false;

        //No parse able MaxRnd
        public override void UpdateMaxRnd(string html)
        {
            //UpdateMaxRnd(ExtractMaxRnd(html, "node"));
        }

        public override string JpgLink(string html)
        {
            throw new NotImplementedException();
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ResNodes);
            var list = new List<ResolutionCapsule>();
            foreach (var node in nodes)
            {
                list.Add(new ResolutionCapsule
                {
                    ResolutionUrl = node.Attributes["href"].Value,
                    ResolutionValue = node.Attributes["title"].Value.Trim().Split(' ').Last()
                });
            }
            return list.Count < 1 ? null : list;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ThumbNodes);
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