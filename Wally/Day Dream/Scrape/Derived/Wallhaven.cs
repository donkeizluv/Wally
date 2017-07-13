using System;
using System.Collections.Generic;
using System.Linq;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap10 : Scraper
    {
        //public Scrap10()
        //{
        //    Ignore = true;
        //}
        private const string ResUrl = "//section[@id='showcase']/div/img";
        private const string ResValueNode = "//h3[@class='showcase-resolution']";
        private const string ThumbNode = "//section[@class='thumb-listing-page']/ul/li/figure";
        private const string MaxRndNode = "//section/header[@class='thumb-listing-page-header']/h2";
        public override string SiteName => "Wallhaven";

        //set it through UpdateMaxRnd(int) to avoid repetive calling of IsMaxRandomUpdated, MaxRnd = x
        public override int MaxRnd { get; protected set; } = 12203;

        protected override string RndUrlTemplate
            => "https://alpha.wallhaven.cc/search?categories=111&purity=110&sorting=date_added&order=desc&page=@";

        public override bool IsJpgUrlNeedParsed { get; } = false;

        public override void UpdateMaxRnd(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectSingleNode(MaxRndNode);
            int max;
            int.TryParse(nodes.InnerText.Split(' ').Last(), out max);
            if (max > MaxRnd)
                UpdateMaxRnd(max);
        }

        public override string JpgLink(string html)
        {
            throw new NotImplementedException();
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var resValueNode = doc.DocumentNode.SelectSingleNode(ResValueNode);
            var resUrlNode = doc.DocumentNode.SelectSingleNode(ResUrl);

            var list = new List<ResolutionCapsule>();
            list.Add(new ResolutionCapsule
            {
                ResolutionUrl = resUrlNode.Attributes["src"].Value,
                ResolutionValue = resValueNode.InnerText.Replace(" ", null)
            });
            return list.Count < 1 ? null : list;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(ThumbNode);
            var list = nodes.Select(node => new PictureData(this)
            {
                PageUrl = node.Element("a").Attributes["href"].Value,
                ThumbUrl = node.Element("img").Attributes["data-src"].Value
            }).ToList();
            ThumbPerPage = list.Count;
            return list.Count < 1 ? null : list;
        }
    }
}