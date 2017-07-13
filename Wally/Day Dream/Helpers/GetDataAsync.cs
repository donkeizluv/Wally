using System;
using System.Net;
using System.Threading;

namespace Wally.Day_Dream.Helpers
{
    /// <summary>
    ///     OBSOLATED use Downloader
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal class GetDataAsync
    {
        public delegate void GetDataComplete(object sender, OpenReadCompletedEventArgs e);

        public GetDataAsync(string url)
        {
            Link = url;
            ThreadPool.QueueUserWorkItem(_threadDownload, url);
        }

        public string Link { get; private set; }
        public event GetDataComplete DownloadComplete;

        protected virtual void RaiseDownloadComplete(OpenReadCompletedEventArgs e) => DownloadComplete?.Invoke(this, e);

        private void _threadDownload(object link)
        {
            Link = Extentions.HandleWeirdFormat((string) link);
            var u = new Uri(Link);
            var client = new WebDownload();
            //Clients.Add(client);
            client.OpenReadCompleted += client_DownloadDataCompleted;
            client.OpenReadAsync(u);
        }

        private void client_DownloadDataCompleted(object sender, OpenReadCompletedEventArgs e)
            => RaiseDownloadComplete(e);
    }

    internal class WebDownload : WebClient
    {
        private const int timeOut = 10000;

        public WebDownload()
        {
            Timeout = timeOut;
            Headers[HttpRequestHeader.UserAgent] =
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/48.0.2564.103 Safari/535.2";
        }

        /// <summary>
        ///     Time in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest) base.GetWebRequest(address);
            if (request == null) return null;
            request.Referer = address.AbsoluteUri; //solved wallpaperwide anti crawler
            request.Timeout = Timeout;
            request.Proxy = null;
            //request.KeepAlive = false; // keep conns count low but reduce too much speed
            return request;
        }
    }
}