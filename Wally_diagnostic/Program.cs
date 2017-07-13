using System;
using System.Collections.Generic;
using System.Linq;
using Wally.Day_Dream;
using Wally.Day_Dream.Scrape;

namespace Wally_diagnostic
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"Wally Diagnosis - Press anykey to start.");
            Console.ReadLine();
            int n = 1;
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(@"Take: " + n);
                if (InnitParser())
                {
                    Console.WriteLine(@"     Number of parsers: " + Scraper.InstanciatedScrapers.ToList().Count);
                }
                TwoLine();
                if (TestDownloadRndPage())
                {
                    Console.WriteLine(@"     Number of pages: " + _capsuleList.Count);
                }
                else
                {
                    WriteLinenReadLine(@"        Random pages : FAILED");
                    return;
                }
                TwoLine();
                if (TestParseRndPage())
                {
                    int i = 0;
                    _capsuleList.ForEach(a => i += a.ImgInfoList.Count);
                    Console.WriteLine(@"     Number of picture/thumb: " + i);
                }
                else
                {
                    WriteLinenReadLine(@"    Parsing thumb/info : FAILED");
                    return;
                }
                TwoLine();
                TestUpdateMaxRnd();
                TwoLine();
                TestDownloadThumb();
                TwoLine();
                TestDownloadResList();
                TwoLine();
                TestParseResList();
                TwoLine();
                TestDownloadWallpaper();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(@"         TEST COMPLEDTED");
                Console.WriteLine(@"Run the test again? Y/N");
                if(string.Compare(Console.ReadLine(),"y",true) != 0)
                {
                    break;
                }
                Console.Clear();
                n++;
                _capsuleList = new List<DataCapsule>();
            }
            //end
        }
        static void WriteLinenReadLine(string s)
        {
            Console.WriteLine(s);
            Console.ReadLine();
        }
        static void TwoLine()
        {
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
