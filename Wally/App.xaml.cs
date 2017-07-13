using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Wally
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //not quite sure how this works but it allows only 1 instance of app.
        private static readonly Mutex _mutex = new Mutex(true, "{760119-B9A1-45fd-A8CF-72F04E6BDE8F}");
        private bool _forceUpdate;
        private bool _skipUpdate;

        protected void ForceOneInstance()
        {
            if (_mutex.WaitOne(TimeSpan.Zero, true)) //check for running instance
                _mutex.ReleaseMutex();
            else
                Process.GetCurrentProcess().Kill();
        }

        private void SetFlags(StartupEventArgs e)
        {
            foreach (string arg in e.Args) //skip check for update
            {
                if (arg == "-s")
                    _skipUpdate = true;
                if (arg == "-f")
                    _forceUpdate = true;
            }
        }

        private bool CanUpdate()
        {
            return GetLastVer() != GetCurrentVer() && !_skipUpdate;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ForceOneInstance();
            SetFlags(e);
            if (CanUpdate() || _forceUpdate)
            {
                var r = MessageBox.Show("A newer version is available. Do you want to update?", "Update?",
                    MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                if (r == MessageBoxResult.Yes)
                    StartupUri = new Uri("/Wally;component/UpdateWindow.xaml", UriKind.Relative);
            }
            //else
            //    StartupUri = new Uri("/Wally;component/MainWindow.xaml", UriKind.Relative);
            base.OnStartup(e);
        }

        private ulong GetCurrentVer() //change this to use Version class instead
        {
            return ulong.Parse(
                Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(".", null));
        }

        private ulong GetLastVer()
        {
            try
            {
                using (var wc = new WebClient())
                {
                    return ulong.Parse(wc.DownloadString(Wally.Properties.Resources.verUrl).Replace(".", null));
                }
            }
            catch //skip and run current ver
            {
                return 0;
            }
        }
    }
}