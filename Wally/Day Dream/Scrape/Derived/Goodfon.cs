using System.Collections.Generic;
using HtmlAgilityPack;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap3 : Scraper
    {
        private const string HomePage = "http://www.goodfon.ru";

        private const string LinkPrefix = "https://avto.goodfon.ru";
            //wallpaper page prefix url ex:https://avto.goodfon.ru/wallpaper/ptica-sad-vetka-cvety-makro.html

        private const string LinkNodes = "//div/div[@class='tabl_td']/div/a";
        private const string ThumbNodes = "//div/div[@class='tabl_td']/div/a/img";
        private const string ResNodes43 = "//optgroup[@label='Fullscreen 4:3']/option";
        private const string ResNodes169 = "//optgroup[@label='Widescreen 16:9']/option";
        private const string ResNodes1610 = "//optgroup[@label='Widescreen 16:10']/option";
        private const string JpGnode = "//a[@id='im']/img";
        private const string JpgIdNode = "//*[@id='img']/img";
        private const string MaxRndNode = "//div[@class = 'pageinfo']/div";

        public override string SiteName => "Goodfon";

        public override int MaxRnd { get; protected set; } = 8181;

        protected override string RndUrlTemplate => "http://www.goodfon.ru/index-@.html";

        public override bool IsJpgUrlNeedParsed { get; } = true;

        public override string JpgLink(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectSingleNode(JpGnode);
            return node?.Attributes["src"].Value;
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes43 = doc.DocumentNode.SelectNodes(ResNodes43);
            var nodes169 = doc.DocumentNode.SelectNodes(ResNodes169);
            var nodes1610 = doc.DocumentNode.SelectNodes(ResNodes1610);
            var s = doc.DocumentNode.SelectSingleNode(JpgIdNode).Attributes["src"].Value.Split('/');
            string jpgId = s[s.Length - 1].Replace(".jpg", null);
            string reslink = string.Format("{0}/download/{1}/", HomePage,
                jpgId);

            var resNumberList = new List<HtmlNodeCollection>(3);
            var resList = new List<ResolutionCapsule>();

            if (nodes43 != null) resNumberList.Add(nodes43);
            if (nodes169 != null) resNumberList.Add(nodes169);
            if (nodes1610 != null) resNumberList.Add(nodes1610);
            if (resNumberList.Count == 0) return null;
            foreach (var nodeList in resNumberList)
            {
                foreach (var resNumber in nodeList)
                {
                    var aninfo = new ResolutionCapsule {ResolutionValue = resNumber.Attributes["value"].Value};
                    aninfo.ResolutionUrl = reslink + aninfo.ResolutionValue;
                    resList.Add(aninfo);
                }
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
                    PageUrl =
                        node.Attributes["href"].Value.StartsWith("/wall")
                            ? LinkPrefix + node.Attributes["href"].Value
                            : node.Attributes["href"].Value
                    //page has 2 types of wallpaper page url: some with prefix https://avto.goodfon.ru/ some dont.
                };
                info.Add(anInfo);
                i++;
            }
            ThumbPerPage = info.Count;
            return info.Count < 1 ? null : info;
        }

        public override void UpdateMaxRnd(string html)
        {
            //UpdateMaxRnd(ExtractMaxRnd(html, MaxRndNode));
            var info = new List<PictureData>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectNodes(MaxRndNode);
            int temp;
            if(int.TryParse(node.FindFirst("div").InnerHtml, out temp))
            {
                UpdateMaxRnd(temp);
            }
            UpdateMaxRnd(MaxRnd);
            //UpdateMaxRnd(MaxRnd);

        }
    }
}