using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap2 : Scraper
    {
        private const string MaxRndNode = "//div[@class='pageLinksDiv']/span";
        //tbody is chrome shit
        //private const string ThumbNodes = "//div[@class='rbox']/div[@class='rboxInner']/table/tbody/tr"; 
        private const string ThumbNodes = "//div[@class='rbox']/div[@class='rboxInner']/table/tr";
        private const string ResNode = "//td[@id='middlecolumn']/table/tr/td/div/div/div/text()";
        private const string ShareLinkNode = "//tr/td/input[@class='sharetextbox']";

        private const string GetJpgUrlFormat =
            "{0}/get_wallpaper_download_url.php?id={1}&w={2}&h={3}";

        public override string SiteName => "Desktopnexus";

        public override int MaxRnd { get; protected set; } = 69581;

        protected override string RndUrlTemplate => "http://www.desktopnexus.com/all/@";

        public override bool IsJpgUrlNeedParsed { get; } = true;

        public override void UpdateMaxRnd(string html)
        {
            //sample: (69,581 Total Pages)
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectSingleNode(MaxRndNode);
            string stringNumber = node.InnerText.Split(' ')[0].Replace("(", null).Replace(",", null);
            UpdateMaxRnd(int.Parse(stringNumber));
        }

        public override string JpgLink(string html)
        {
            return html;
        }

        //@/get_wallpaper_download_url.php?id=@&w=@&h=@"
        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var list = new List<ResolutionCapsule>();
            string shareLink = string.Empty;
            string resValue = string.Empty;

            foreach (var inputElement in doc.DocumentNode.SelectNodes(ShareLinkNode))
            {
                //exp: //animals.desktopnexus.com/wallpaper/903890/
                if (!inputElement.Attributes["value"].Value.StartsWith(@"//")) continue;
                shareLink = inputElement.Attributes["value"].Value;
                break;
            }
            foreach (var text in doc.DocumentNode.SelectNodes(ResNode))
            {
                //ex: \n\t\tOriginal Resolution: 1600x1312
                if (!text.InnerText.Contains("Original Resolution")) continue;
                resValue = text.InnerText.Split(' ').Last();
                break;
            }
            if (shareLink == string.Empty || resValue == string.Empty)
                return null;
            var split = shareLink.Trim('/').Split('/');
            string homePage = split.First();
            string id = split.Last();
            //only offer 1 ogriginal res
            list.Add(new ResolutionCapsule
            {
                ResolutionValue = resValue,
                ResolutionUrl =
                    string.Format(GetJpgUrlFormat, homePage, id, resValue.Split('x').First(),
                        resValue.Split('x').Last())
            });
            return list.Count < 1 ? null : list;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var trNodes = doc.DocumentNode.SelectNodes(ThumbNodes);
            var capsules = (
                from tr in trNodes
                from child in tr.ChildNodes
                where child.Name == "td"
                select new PictureData(this)
                {
                    ThumbUrl = child.Element("img").Attributes["src"].Value,
                    PageUrl = child.Element("a").Attributes["href"].Value,
                    WallpaperName = child.Element("a").Attributes["alt"].Value
                }).ToList();
            ThumbPerPage = capsules.Count;
            return capsules.Count < 1 ? null : capsules;
        }
    }
}