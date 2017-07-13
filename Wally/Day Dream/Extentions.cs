using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wally.Day_Dream
{
    internal static class Extentions
    {
        private static readonly Random _rnd = new Random();

        public static Bitmap ConvertToBitmap(this Stream stream)
        {
            try
            {
                using (stream)
                    return new Bitmap(stream);
            }
            catch
            {
#if DEBUG
                throw;
#endif
                return null;
            }
        }

        public static string ConvertToString(this Stream stream)
        {
            try
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
#if DEBUG
                throw;
#endif
                return null;
            }
            return null;
        }

        public static int ConvertToPixel(this string res)
        {
            var temp = res.Split('x');
            return int.Parse(temp[0])*int.Parse(temp[1]);
        }

        public static bool IsResolution(this string s)
        {
            return s.IndexOf('x') == 4;
        }

        public static string GetCurrentExeLoccation()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(filePath);
        }

        public static void Shuffle<T>(this Stack<T> stack)
        {
            var reOrder = stack.OrderBy(order => _rnd.Next()).ToList();
            stack.Clear();
            for (int i = 0; i < reOrder.Count - 1; i++)
            {
                stack.Push(reOrder[i]);
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            var handle = bitmap.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        public static string HandleWeirdFormat(string url)
        {
            if (url.StartsWith("/"))
                return "http://" + url.TrimStart('/');
            if (!url.StartsWith("http"))
                return "http://" + url;
            return url;
        }

        public static bool ConnectionAvailable(string strServer)
        {
            try
            {
                var reqFP = (HttpWebRequest) WebRequest.Create(strServer);
                var rspFP = (HttpWebResponse) reqFP.GetResponse();
                if (HttpStatusCode.OK == rspFP.StatusCode)
                {
                    // HTTP = 200 - Internet connection available, server online
                    rspFP.Close();
                    return true;
                }
                // Other status - Server or connection not available
                rspFP.Close();
                return false;
            }
            catch (WebException)
            {
                // Exception - connection not available
#if DEBUG
                throw;
#endif
                return false;
            }
        }
    }
}