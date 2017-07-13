using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Wally.Day_Dream;
using Wally.Properties;

namespace Wally
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isinFav;
        private int _totalWallpapers = Settings.Default.TotalWallpapers;
        //TimeSpan ts = DateTime.Today - Settings.Default.LastUpdate;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void mainform_Loaded(object sender, RoutedEventArgs e)
        {
            CheckInternetConnection();
            SetRenderFPS(24);
            LoadSetting();
            SubscribeEvents();
            await DayDreamView.StartDayDreaming().ConfigureAwait(false);
            //await DayDreamView.StartFavorite();
        }

        private void SubscribeEvents()
        {
            DayDreamView.ThumbDowloaded += DayDreamView_ThumbDowloaded;
            DayDreamView.DownloadWallpaperCompleted += DayDreamView_DownloadWallpaperCompleted;
            DayDreamView.FavoriteShown += DayDreamView_FavoriteShown;
            DayDreamView.DayDreamShown += DayDreamView_DayDreamShown;
        }

        private void DayDreamView_DayDreamShown(object sender, EventArgs e)
        {
            lbFav.Content = "Favorites →";
        }

        private void DayDreamView_FavoriteShown(object sender, EventArgs e)
        {
            lbFav.Content = "← Back";
        }

        private void DayDreamView_ThumbDowloaded(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(UpdateCount));
        }

        private void DayDreamView_DownloadWallpaperCompleted(object sender, DownloadWallpaperCompletedEventArgs e)
        {
            SaveImage(e.Image, e.Name);
        }

        private static void SaveImage(Image img, string filename)
        {
            string path = Path.Combine(Extentions.GetCurrentExeLoccation(), "Wallpapers");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            img.Save(Path.Combine(path, filename.EndsWith(".jpg") ? filename : filename + ".jpg"), ImageFormat.Jpeg);
        }

        private void UpdateCount()
        {
            if (DayDreamView.TotalWallpapers > _totalWallpapers)
            {
                _totalWallpapers = DayDreamView.TotalWallpapers;
                lbTotal.Content = string.Format("{0:n0}", _totalWallpapers) + " Wallpapers";
            }
        }

        private void CheckInternetConnection()
        {
            while (true)
            {
                if (!Extentions.ConnectionAvailable("http://bing.com"))
                {
                    var res = MessageBox.Show("Problem with internet, retry?", "No Internet",
                        MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
                    if (res == MessageBoxResult.Cancel)
                        Close();
                }
                else break;
            }
        }

        private void SetRenderFPS(int fps)
        {
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata {DefaultValue = fps});
        }

        private void LoadSetting()
        {
            if (_totalWallpapers == 0)
                lbTotal.Content = string.Empty;
            else
                lbTotal.Content = string.Format("{0:n0}", Settings.Default.TotalWallpapers) + " Wallpapers";
        }

        private void mainform_Closing(object sender, CancelEventArgs e)
        {
            Settings.Default.TotalWallpapers = _totalWallpapers;
            Settings.Default.Save();
        }

        private void lbCreator_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (
                MessageBox.Show("Open creator's facebook?", "Confirmation", MessageBoxButton.OKCancel,
                    MessageBoxImage.Question) == MessageBoxResult.OK)
                Process.Start("https://www.facebook.com/pinkypink760");
        }

        private async void lbFav_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isinFav)
            {
                _isinFav = false;
                await DayDreamView.StartDayDreaming().ConfigureAwait(false);
            }
            else
            {
                _isinFav = true;
                await DayDreamView.StartFavorite().ConfigureAwait(false);
            }
        }
    }
}