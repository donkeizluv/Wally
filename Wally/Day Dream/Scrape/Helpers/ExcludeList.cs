using System;
using System.Collections.Generic;
using Wally.Day_Dream.Scrape.Derived;

namespace Wally.Day_Dream.Scrape.Helpers
{
    internal static partial class ReflectiveEnumerator
    {
        private static readonly List<Type> ExcludeList = new List<Type>
        {
            typeof(TemplateScraper)
            //typeof(WallpaperscraftScraper)
        };
    }
}