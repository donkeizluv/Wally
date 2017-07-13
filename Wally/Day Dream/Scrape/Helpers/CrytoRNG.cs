using System;
using System.Security.Cryptography;

namespace Wally.Day_Dream.Scrape.Helpers
{
    internal static class CrytoRng
    {
        private static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        private static double NextDouble()
        {
            var b = new byte[4];
            rng.GetBytes(b);
            return (double) BitConverter.ToUInt32(b, 0)/uint.MaxValue;
        }

        public static int Next(int minValue, int maxValue)
        {
            return (int) Math.Round(NextDouble()*(maxValue - minValue + 1)) + minValue;
        }
    }
}