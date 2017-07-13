using System.Collections.Generic;
using Wally.HTML;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class Scrap5 : Scraper
    {
        private const string HomePage = "http://interfacelift.com";
        private const string LinkNodes = "//div[@class='item']/div[@class='preview']/div/a";
        private const string ThumbNodes = "//div[@class='item']/div[@class='preview']/div/a/img";
        private const string ResNodes43 = "//select/optgroup[@label='Fullscreen 4:3']/option";
        private const string ResNodes169 = "//select/optgroup[@label='Widescreen 16:9']/option";
        private const string ResNodes1610 = "//select/optgroup[@label='Widescreen 16:10']/option";
        private const string MaxRndNode = "//div[@class='pagenums_bottom']/div/a[@class='selector']";

        public override string SiteName => "Interfacelift";

        public override int MaxRnd { get; protected set; } = 369;

        protected override string RndUrlTemplate
            => "https://interfacelift.com/wallpaper/downloads/date/any/index@.html";

        public override bool IsJpgUrlNeedParsed { get; } = false;

        public override string JpgLink(string html)
        {
            return html;
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes43 = doc.DocumentNode.SelectNodes(ResNodes43);
            var nodes169 = doc.DocumentNode.SelectNodes(ResNodes169);
            var nodes1610 = doc.DocumentNode.SelectNodes(ResNodes1610);
            var resLink = doc.DocumentNode.SelectSingleNode(ThumbNodes);
            //its the same with random page thumb
            string cookedString = resLink.Attributes["src"].Value.Replace("previews", "7yz4ma1");
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
                    string resNum = resNumber.Attributes["value"].Value;
                    var aninfo = new ResolutionCapsule
                    {
                        ResolutionValue = resNum,
                        ResolutionUrl =
                            cookedString.Replace(".jpg", "_" + resNum + ".jpg")
                                .Replace("@2x", null)
                                .Replace("_672x420", null)
                    };
                    resList.Add(aninfo);
                }
            }
            return resList.Count < 1 ? null : resList;
        }

        public override List<PictureData> ExtractImages(string html)
        {
            var capsules = new List<PictureData>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var links = doc.DocumentNode.SelectNodes(LinkNodes);
            var thumbs = doc.DocumentNode.SelectNodes(ThumbNodes);
            int i = 0;
            if (links == null || thumbs == null) return capsules;
            foreach (var node in links)
            {
                var anInfo = new PictureData(this)
                {
                    ThumbUrl = thumbs[i].Attributes["src"].Value,
                    PageUrl = HomePage + node.Attributes["href"].Value
                };
                capsules.Add(anInfo);
                i++;
            }
            ThumbPerPage = capsules.Count;
            return capsules.Count < 1 ? null : capsules;
        }

        public override void UpdateMaxRnd(string html)
        {
            UpdateMaxRnd(ExtractMaxRnd(html, MaxRndNode));
        }
    }
}