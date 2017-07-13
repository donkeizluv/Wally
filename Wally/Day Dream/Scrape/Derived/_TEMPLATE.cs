using System;
using System.Collections.Generic;

namespace Wally.Day_Dream.Scrape.Derived
{
    internal class TemplateScraper : Scraper
    {
        public override string SiteName => "TestParserName";

        //set it through UpdateMaxRnd(int) to avoid repetive calling of IsMaxRandomUpdated, MaxRnd = x
        public override int MaxRnd { get; protected set; } = 999999;

        protected override string RndUrlTemplate => "TemplateUrl";

        public override bool IsJpgUrlNeedParsed { get; } = true;

        public override void UpdateMaxRnd(string html)
        {
            UpdateMaxRnd(ExtractMaxRnd(html, "node"));
        }

        public override string JpgLink(string html)
        {
            throw new NotImplementedException();
        }

        public override List<ResolutionCapsule> ExtractResolutions(string html)
        {
            throw new NotImplementedException();
        }

        public override List<PictureData> ExtractImages(string html)
        {
            throw new NotImplementedException();
            //List.Count to calculate total wallpaper
#pragma warning disable CS0162
            ThumbPerPage = 999;
        }
    }
}