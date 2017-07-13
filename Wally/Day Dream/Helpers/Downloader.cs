using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Wally.Day_Dream.Helpers
{
    internal interface IDownloader
    {
        Task<Stream> OpenReadAsync(string url);
        Task<Stream> DownloadDataAsync(string url);
        event DownloadProgressChangedEventHandler ProgressChanged;
    }

    internal class Downloader : IDownloader
    {
        public async Task<Stream> OpenReadAsync(string url)
        {
            using (var wd = new WebDownload())
            {
                return await wd.OpenReadTaskAsync(Extentions.HandleWeirdFormat(url));
            }
        }

        /// <summary>
        ///     Use this for progress track support
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<Stream> DownloadDataAsync(string url)
        {
            using (var wd = new WebDownload())
            {
                wd.DownloadProgressChanged += (s, e) => { ProgressChanged?.Invoke(this, e); };
                return new MemoryStream(await wd.DownloadDataTaskAsync(Extentions.HandleWeirdFormat(url)));
            }
        }

        public event DownloadProgressChangedEventHandler ProgressChanged;
    }
}