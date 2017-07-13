using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Wally.Day_Dream.Helpers
{
    internal enum StretchStyle
    {
        Tiled,
        Centered,
        Stretched,
        Fit, //windows 7
        Fill
    }

    /// <summary>
    ///     set wallpaper from file
    /// </summary>
    /// <returns></returns>
    internal static class DesktopWallpaperHelper
    {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        public static void SetWallpaper(Bitmap m, StretchStyle style)
        {
            var img = m;
            string tempPath = Path.Combine(Path.GetTempPath(), "temp.bmp");
            img.Save(tempPath, ImageFormat.Bmp);
            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            //set style
            switch (style)
            {
                case StretchStyle.Stretched:
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case StretchStyle.Centered:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case StretchStyle.Tiled:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "1");
                    break;
                case StretchStyle.Fit: // (Windows 7 and later) 
                    key.SetValue(@"WallpaperStyle", "6");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case StretchStyle.Fill: // (Windows 7 and later) 
                    key.SetValue(@"WallpaperStyle", "10");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
            }
            //set wall
            NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        }
    }
}