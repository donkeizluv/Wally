using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Wally.Day_Dream.Scrape.Helpers;
using HtmlAgilityPack;

namespace Wally.Day_Dream.Scrape
{
    /// <summary>
    ///     Base scraper
    /// </summary>
    internal abstract class Scraper
    {
        #region static stuff

        public static bool IsInitiated { get; private set; }
        //use Collection for extensibility
        public static IEnumerable<Scraper> InstanciatedScrapers { get; private set; } = new Collection<Scraper>();

        public static int TotalWallpapers
            => !IsInitiated ? 0 : InstanciatedScrapers.Sum(p => p.ThumbPerPage*p.MaxRnd);

        /// <summary>
        ///     for debug specific type of scraper
        /// </summary>
        /// <param name="scraper"></param>
        public static void InstanciateSpecificType(Scraper scraper)
        {
            var list = new List<Scraper> {scraper};
            InstanciatedScrapers = list;
            IsInitiated = true;
        }

        public static bool InstanciateAllDerivedTypes()
        {
            //if (IsInitiated) return false;
            //prevent duplicate parser
            //InstanciatedParsers.Clear();
            var list = ReflectiveEnumerator.GetEnumerableOfType<Scraper>();
            InstanciatedScrapers = from scraper
                in list
                where scraper.Ignore == false
                select scraper;
            IsInitiated = true;
            return true;
        }

        public static Scraper GetScraperByName(string name)
        {
            return InstanciatedScrapers.First(s => s.SiteName == name);
        }

        #endregion

        #region abstract stuff

        public abstract string SiteName { get; }
        public abstract int MaxRnd { get; protected set; }
        protected abstract string RndUrlTemplate { get; }
        public abstract List<PictureData> ExtractImages(string html);
        public abstract List<ResolutionCapsule> ExtractResolutions(string html);
        public abstract string JpgLink(string html);
        public abstract void UpdateMaxRnd(string html);
        public abstract bool IsJpgUrlNeedParsed { get; }

        #endregion

        #region inherit stuff
        public string LastDownloadRndUrl;
        public bool Ignore { get; set; } = false;
        public int ThumbPerPage { get; protected set; } = 0;
        public bool IsMaxRandomUpdated { get; private set; }
        //consider changing this to Property
        public virtual string GetRandomPageUrl()
        {
            LastDownloadRndUrl = RndUrlTemplate.Replace("@", CrytoRng.Next(1, MaxRnd).ToString());
            return LastDownloadRndUrl;
        }

        protected virtual int ExtractMaxRnd(string html, string xpath)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.SelectNodes(xpath);
            if (nodes.Count < 1)
                return 0;

            int temp = 0;
            var intArray = (from n in nodes
                where int.TryParse(n.InnerText, out temp)
                select 0).ToArray();
            //if (intArray.Length == 0)
            //    throw new ArgumentException("Extract max rnd fail.");
            //return intArray.Max();

            return intArray.Length > 0 ? intArray.Max() : throw new ArgumentException("Extract max rnd fail.");
        }

        protected void UpdateMaxRnd(int max)
        {
            MaxRnd = max;
            IsMaxRandomUpdated = true;
        }

        #endregion
    }
}