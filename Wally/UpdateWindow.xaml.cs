using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;

namespace Wally
{
    /// <summary>
    ///     Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private const int MaxReTry = 3;
        private int _tries;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void UpdateWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            RunUpdate();
        }

        private void RunUpdate()
        {
            string path = Path.Combine(GetCurrentExeLoccation(), "temp");
            using (var wd = new WebClient())
            {
                wd.DownloadProgressChanged += Client_DownloadProgressChanged;
                wd.DownloadFileCompleted += (s, e) => client_DownloadFileCompleted(s, e, path);
                wd.DownloadFileAsync(new Uri(Properties.Resources.exeUrl2), path);
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressbar1.Value = e.ProgressPercentage;
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, string filePath)
        {
            if (CheckValidExe(filePath) && e.Error == null)
            {
                //succsess
                CleanUpThenRun();
            }
            else
            {
                //fail
                if (_tries == MaxReTry)
                {
                    //inform then start current ver
                    MessageBox.Show("Failed to update. Starting current version...", "Fail", MessageBoxButton.OK);
                    var window = new MainWindow();
                    window.Show();
                    Close();
                }
                //retry update
                RunUpdate();
                _tries++;
            }
        }

        private bool CheckValidExe(string url)
        {
            return ExeChecker.ExeChecker.IsValidExe(GetCurrentExeLoccation() + "\\temp");
        }

        private void CleanUpThenRun()
        {
            //self delete code :))
            var cmd = new ProcessStartInfo();
            cmd.Arguments = "/C choice /C Y /N /D Y /T 2 & Del " + '"' + Assembly.GetExecutingAssembly().Location + '"' +
                            " & "; //ok
            //rename
            cmd.Arguments += "ren " + '"' + GetCurrentExeLoccation() + "\\temp" + '"' + " Wally.exe" + " & "; //ok
            cmd.Arguments += "start Wally.exe"; //ok
            cmd.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.CreateNoWindow = true;
            cmd.FileName = "cmd.exe";
            Process.Start(cmd);
            //Process.GetCurrentProcess().Kill();
            Close();
        }

        public static string GetCurrentExeLoccation()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(filePath);
        }
    }
}