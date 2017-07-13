using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wally.Day_Dream.Favorite;

namespace Wally.Day_Dream.Controls
{
    /// <summary>
    ///     Interaction logic for Cell.xaml
    /// </summary>
    internal partial class ImageCell : Grid
    {
        private bool _highLight;
        //public static readonly DependencyProperty IsImageDownloaded =
        //    DependencyProperty.Register("IsImageDownloaded", typeof(bool),
        //        typeof(ImageCell), new UIPropertyMetadata(false));

        private bool? _isFavorite;

        public ImageCell() : this(null)
        {
        }

        public ImageCell(Favorites favoritesSaver)
        {
            InitializeComponent();
            SiteNameLabel.Visibility = Visibility.Hidden;
            Saver = favoritesSaver;
            WhiteRec.SetValue(ZIndexProperty, 100);
            WhiteRec.Visibility = Visibility.Hidden;
            HideProgressBar();
            HideStar();
        }

        public bool? IsFavorite
        {
            get { return _isFavorite; }
            private set
            {
                _isFavorite = value;
                if (_isFavorite == null)
                    HideStar();
                else
                {
                    ShowStar();
                    if (_isFavorite ?? false)
                        YellowStar();
                    else
                        WhiteStar();
                }
            }
        }

        public bool HighLight
        {
            get { return _highLight; }
            set
            {
                _highLight = value;
                if (_highLight)
                    WhiteRec.Visibility = Visibility.Visible;
                else
                    WhiteRec.Visibility = Visibility.Hidden;
            }
        }

        public bool ShowSiteName { get; set; }
        public Favorites Saver { get; set; }
        public PictureData Data { get; private set; }
        public ImageSource GetImageSource => image.Source;
        public bool HasSource { get; private set; }

        public void SetData(PictureData data)
        {
            HasSource = true;
            HideProgressBar();
            Data = data;
            image.Source = data.ThumbBitmap.ToImageSource();
            HighLight = true;
            IsFavorite = data.IsFavorite;
            SiteNameLabel.Content = data.Scraper.SiteName;
            SiteNameLabel.Visibility = ShowSiteName ? Visibility.Visible : Visibility.Hidden;
            Favorites.AddedToFavoriteEvent += Saver_AddedToFavoriteEvent;
            Favorites.RemovedFromFavorite += Saver_RemovedFromFavorite;
        }

        private void ClearData()
        {
            Data = null;
            Favorites.AddedToFavoriteEvent -= Saver_AddedToFavoriteEvent;
            Favorites.RemovedFromFavorite -= Saver_RemovedFromFavorite;
        }

        public void ShowProgressBar()
        {
            progressBar.Visibility = Visibility.Visible;
            progressBar.RandomizeForegroundColor();
        }

        public void HideProgressBar()
        {
            progressBar.Visibility = Visibility.Hidden;
        }

        public void Clear()
        {
            HasSource = false;
            Opacity = 0;
            image.Source = null;
            SiteNameLabel.Visibility = Visibility.Hidden;
            //SetValue(IsImageDownloaded, false);
            ClearData();
            IsFavorite = null;
            ShowProgressBar();
            HighLight = false;
        }

        private void HideStar()
        {
            favGrid.Visibility = Visibility.Hidden;
        }

        private void ShowStar()
        {
            if (_isFavorite == null) throw new Exception("Show favorite button while flag is null.");
            favGrid.Visibility = Visibility.Visible;
        }

        //favorite star clicked
        private void favStar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsFavorite ?? false)
                //do delete
                if (DeleteFavorite())
                {
                    Data.IsFavorite = false;
                    IsFavorite = false;
                }

                else
                    MessageBox.Show("Failed to delete from favorite");
            else
            //favor this image
                if (SaveFavorite())
                {
                    Data.IsFavorite = true;
                    IsFavorite = true;
                }
                else
                    MessageBox.Show("Failed to add to favorite");
        }

        private void Saver_RemovedFromFavorite(object sender, RemovedFromFavoriteEventAgrs e)
        {
            if (e.TargetPageUrl == Data.PageUrl)
                IsFavorite = false;
        }

        private void Saver_AddedToFavoriteEvent(object sender, AddedToFavoriteEventAgrs e)
        {
            if (e.TargetPageUrl == Data.PageUrl)
                IsFavorite = true;
        }

        private bool SaveFavorite()
        {
            return Saver.Save(Data);
        }

        private bool DeleteFavorite()
        {
            return Saver.Delete(Data);
        }

        private void YellowStar()
        {
            buttonFav.Background = new SolidColorBrush(Color.FromRgb(255, 255, 0));
        }

        private void WhiteStar()
        {
            buttonFav.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }
    }
}