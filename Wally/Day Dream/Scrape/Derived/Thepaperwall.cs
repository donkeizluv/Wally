using System;
using System.Collections.Generic;
using System.Linq;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap9 : Scraper
    {
        public Scrap9() //BUG: this one is fucked up, some pages work some dont
        {
            Ignore = true;
            //showing 60 per page seems to fix the issue.....and nope! 
        }
        private const string ResValueNode = "//div[@class='img_resolution']/p";
        private const string ResUrl = "//a[@class='wall_img_a']/img[1]";
        private const string Homepage = "http://www.thepaperwall.com/";
        private const string ThumbNode = "//div[@class='single_thumbnail_cont']/a[@class='thumbnail_cont']";

        //private const string MaxRndNode =
            //"//div[@class='pagination']/div[@class='pagination']/div[@class='pagination_inner']/a";

        public override string SiteName => "Thepaperwall";

        //set it through UpdateMaxRnd(int) to avoid repetive calling of IsMaxRandomUpdated, MaxRnd = x
        public override int MaxRnd { get; protected set; } = 3579;

        protected override string RndUrlTemplate
            => "http://www.thepaperwall.com/all.php?action=catcontent&c=1&r=a&l=60&page=@";

        public override bool IsJpgUrlNeedParsed { get; } = false;

        public override void UpdateMaxRnd(string html)
        {
            //UpdateMaxRnd(ExtractMaxRnd(html, MaxRndNode));
            UpdateMaxRnd(MaxRnd);
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
                ResolutionUrl = Homepage + resUrlNode.Attributes["src"].Value,
                ResolutionValue = resValueNode.InnerText
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
                PageUrl = Homepage + node.Attributes["href"].Value,
                ThumbUrl = Homepage + node.Element("img").Attributes["src"].Value.Replace("amp;", null)
            }).ToList();
            ThumbPerPage = list.Count;
            return list.Count < 1 ? null : list;
        }
    }
}