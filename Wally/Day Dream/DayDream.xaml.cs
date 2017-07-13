using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Wally.Day_Dream.Controls;
using Wally.Day_Dream.Favorite;
using Wally.Day_Dream.Helpers;
using Wally.Day_Dream.Interfaces;
using Wally.Day_Dream.Scrape;

namespace Wally.Day_Dream
{
    /// <summary>
    ///     Interaction logic for DayDream.xaml
    /// </summary>
    internal partial class DayDream
    {
        private const int CellHeight = 180;
        private const int CellWidth = 320;
        private const int MaxRows = 16;
        private const int MaxColumns = 3;
        private const int Buffer = 0; //0 means only last row will load
        //wait n mili sec bw each download to prevent creating too much request at the same time
        private const int DefaultThumbDownloadDelay = 176;
        //downloaded n of thumbs in loading row before allow to load more
        private const int DownloadNewThumbThreshold = 2;
        private const int CacheLimit = 150;
        private readonly int _totalCells = MaxColumns*MaxRows;

        //other stuff
        private readonly Queue<PictureData> _cachedData = new Queue<PictureData>();

        private readonly object _cacheLock = new object();
        private ImageCell _clickedPictureCell;
        private IPictureDataProvider _daydreamProvider;
        private IDownloader _downloader;
        private readonly Queue<ImageCell> _emptyCells = new Queue<ImageCell>();
        private readonly List<int> _emptyInfinyRowIndexes = new List<int>();
        private Favorites _favoriteSaver;
        private readonly Queue<ImageCell> _fixedEmptyCells = new Queue<ImageCell>();
        private bool _isCancelable = true;
        private bool _isDaydreamInnit;
        //threshold before scroll down is enable
        private int _scrollableThreshold;
        private MouseButtonEventHandler _thumbClickedHandler;
        private int _thumbDownloadDelay = DefaultThumbDownloadDelay;

        //normal mode
        public List<List<ImageCell>> DayDreamRows = new List<List<ImageCell>>();
        //fixed set mode
        public List<List<ImageCell>> FixedSourceRows = new List<List<ImageCell>>();

        public DayDream()
        {
#if DEBUG
            DebugMode = true;
#endif
            InitializeComponent();
            Innit();
        }

        public bool IsDayDreaming { get; private set; }
        public bool IsShowingFavorite { get; private set; }
        public bool Save { get; set; } = true;
        public bool IsSettingWallpaper { get; private set; }
        public bool DebugMode { get; set; } = false;
        //project is using NET 4.5 now so all OS support Fill style.
        public StretchStyle StretchTo { get; set; } =
            SupportFitFillWallpaperStyles ? StretchStyle.Fill : StretchStyle.Stretched;


        public int TotalRows => DayDreamRows.Count;
        public int TotalWallpapers => Scraper.TotalWallpapers;
        private static bool SupportFitFillWallpaperStyles => Environment.OSVersion.Version >= new Version(6, 1);

        private void Innit()
        {
            _downloader = new Downloader();
            _downloader.ProgressChanged += (s, e) => { Curtain.AnimateProgressBar(e); };
            _daydreamProvider = new DayDreamDataProvider();
            _thumbClickedHandler = ThumbImage_MouseLeftButtonUp;
            Scraper.InstanciateAllDerivedTypes();
            _favoriteSaver = new FavoritesSaver();
            //Scraper.InstanciateSpecificType(new WallpaperScraper.Derived.Suwalls());
            InnitCurtain();
        }

        private void InnitCurtain()
        {
            Curtain.CurtainUp();
            Curtain.LabelClicked += Curtain_LabelClicked;
            Curtain.PreviewImage.Saver = _favoriteSaver;
        }

        private void AddHandlerAndMapToQueue(List<List<ImageCell>> rowsCollection, Queue<ImageCell> queue,
            MouseButtonEventHandler eventHandler)
        {
            foreach (var row in rowsCollection)
            {
                foreach (var element in row)
                {
                    element.MouseLeftButtonUp += eventHandler;
                    queue.Enqueue(element);
                }
            }
        }

        public async Task StartFavorite()
        {
            if (!_isCancelable || Curtain.IsCurtainShown || IsSettingWallpaper) return;
            ShowFavorite();
            if (_favoriteSaver.FavoriteCount < 1) return;
            CleanFavorite();
            AddTableToGrid(FixedGrid, (_favoriteSaver.FavoriteCount - 1)/MaxColumns + 1, MaxColumns);
            AddCellsAndMapToCollection(FixedGrid, FixedSourceRows);
            AddHandlerAndMapToQueue(FixedSourceRows, _fixedEmptyCells, _thumbClickedHandler);
            await FillImages(
                _favoriteSaver.FavoriteCount,
                _fixedEmptyCells,
                (IPictureDataProvider) _favoriteSaver).ConfigureAwait(false);
        }

        private void CleanFavorite()
        {
            FixedGrid.ColumnDefinitions.Clear();
            FixedGrid.RowDefinitions.Clear();
            FixedGrid.Children.Clear();
            _fixedEmptyCells.Clear();
            FixedSourceRows.ForEach(r => r.ForEach(e => e.MouseLeftButtonUp -= _thumbClickedHandler));
            FixedSourceRows.Clear();
        }

        private void ShowFavorite()
        {
            DayDreamScrollViewer.Visibility = Visibility.Hidden;
            FixedScrollViewer.Visibility = Visibility.Visible;
            IsShowingFavorite = true;
            IsDayDreaming = false;
            RaiseFavoriteShown();
        }

        private void ShowDayDream()
        {
            DayDreamScrollViewer.Visibility = Visibility.Visible;
            FixedScrollViewer.Visibility = Visibility.Hidden;
            IsShowingFavorite = false;
            IsDayDreaming = true;
            RaiseDayDreamShown();
        }

        /// <summary>
        ///     Fill all cell (start the main function)
        /// </summary>
        /// <returns></returns>
        public async Task StartDayDreaming()
        {
            if (!_isCancelable || Curtain.IsCurtainShown || IsSettingWallpaper) return;
            if (!_isDaydreamInnit)
            {
                _scrollableThreshold = _totalCells / 2;
                AddTableToGrid(DayDreamGrid, MaxRows, MaxColumns);
                AddCellsAndMapToCollection(DayDreamGrid, DayDreamRows);
                AddHandlerAndMapToQueue(DayDreamRows, _emptyCells, _thumbClickedHandler);
                _isDaydreamInnit = true;
                await FillImages(DayDreamGrid.Children.Count, _emptyCells, _daydreamProvider);
            }
            else
            {
                ShowDayDream();
            }
        }

        /// <summary>
        ///     Fill n images to queue
        /// </summary>
        /// <param name="count">number of images to load</param>
        /// <param name="queue">queue of ImageCell to fill</param>
        /// <param name="provider">data provider</param>
        /// <returns></returns>
        private async Task FillImages(int count, Queue<ImageCell> queue, IPictureDataProvider provider)
        {
            if (count < 1) ExManager.Ex(new InvalidOperationException("FillImageQueue is passed with 0"));
            var tasks = new List<Task>();
            for (int i = count; i > 0; i--)
            {
                tasks.Add(
                    DisplayAnImage(
                        GetCachedData(await provider.GetData().ConfigureAwait(false)), queue, provider));
                await Task.Delay(_thumbDownloadDelay);
            }
            await Task.WhenAll(tasks);
        }

        //download and display cell
        private async Task DisplayAnImage(PictureData pictureData, Queue<ImageCell> queue, IPictureDataProvider provider)
        {
            var bitmap = pictureData.ThumbBitmap ?? await pictureData.GetThumbBitmap().ConfigureAwait(false);
            if (bitmap == null)
            {
                Debug.WriteLine("Thumb download failed, scraper: " + pictureData.Scraper.SiteName);
                if (provider.CanSupplyWhenFail)
                    //replace with another image
                {
                    Debug.WriteLine("Replace fail thumb...");
                    await FillImages(1, queue, provider).ConfigureAwait(false);
                }
                else
                //fixed set, cant replace, displays default error image
                    UiInvoke(() => { throw new NotImplementedException(); });
                return;
            }
            bool needCached = CacheData(pictureData);
            if (queue.Count == 0)
            {
                Debug.WriteLine("Queue is empty, can not display image.");
                return;
            }
            var cell = queue.Dequeue();
            //when a favorited data is randomly loaded marks it as so
            pictureData.IsFavorite = _favoriteSaver.CheckFavorite(pictureData);
            UiInvoke(() =>
            {
                //cell.SetValue(ImageCell.IsImageDownloaded, true);
                cell.SetData(pictureData);
                //no effect when data is taken from cached
                if (needCached)
                    cell.BeginStoryboard((Storyboard) TryFindResource("fadeInEffect"));
            });

            RaiseThumbDowloaded();
        }

        /// <summary>
        ///     return true cache is needed, otherwise false
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool CacheData(PictureData data)
        {
            lock (_cacheLock)
            {
                bool isCached = _cachedData.ToList().Exists(c => c.PageUrl == data.PageUrl);
                if (!isCached)
                    _cachedData.Enqueue(data);
                if (_cachedData.Count > CacheLimit)
                    _cachedData.Dequeue();
                return !isCached;
            }
        }

        private PictureData GetCachedData(PictureData data)
        {
            lock (_cacheLock)
            {
                var cachedData = _cachedData.ToList().FirstOrDefault(d => d.PageUrl == data.PageUrl);
                if (cachedData == null)
                {
                    //download new bitmap, delays throttle rq
                    _thumbDownloadDelay = DefaultThumbDownloadDelay;
                    cachedData = data;
                }
                else
                {
                    //load from cache
                    _thumbDownloadDelay = 0;
                }
                return cachedData;
            }
        }

        //scrolling down
        //MAGIX: this event fire once when the app starts
        private async void ScrollViewerDayDream_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //no scrolling down when theres nothing
            //TODO: when too many scrapers fail to scrape rnd data, downloaded will never reach threshold
            //so the whole thing will never scroll down
            if (DayDreamDownloadedThumbs() <= _scrollableThreshold /*|| IsFirstStarted*/)
            {
                DayDreamScrollViewer.LineUp();
                return;
            }
            //checks if scroll to the end
            if (Math.Abs(DayDreamScrollViewer.VerticalOffset - DayDreamScrollViewer.ScrollableHeight) > 1)
                return;
            if (_emptyInfinyRowIndexes.Count < 1) //no rows are loading, go loading some
                goto LoadNewImage;
            //some rows are loading, check if the lastest row has loaded
            int downloaded = 0;
            foreach (var element in DayDreamRows[_emptyInfinyRowIndexes.Max()])
            {
                downloaded += element.HasSource ? 1 : 0;
                if (downloaded == DownloadNewThumbThreshold)
                {
                    _emptyInfinyRowIndexes.Clear();
                    goto LoadNewImage;
                }
            }
            DayDreamScrollViewer.LineUp();
            return;
            LoadNewImage:
            DayDreamScrollViewer.LineUp();
            await Advance().ConfigureAwait(false);
        }

        private int DayDreamDownloadedThumbs()
        {
            int count = 0;
            foreach (var row in DayDreamRows)
            {
                foreach (var element in row)
                {
                    if (element.HasSource)
                        count++;
                }
            }
            return count;
        }

        private async Task Advance()
        {
            int toLoad = 0;
            for (int index = 0; index < DayDreamRows.Count; index++)
            {
                var row = DayDreamRows[index];
                int rowIndex = Grid.GetRow(row[0]);
                if (rowIndex == 0) //determine if first row
                {
                    MoveRowTo(row, TotalRows - 1);
                    ClearRow(row, index);
                    toLoad += row.Count;
                    continue;
                }
                MoveRowTo(row, rowIndex - 1);
                if (DayDreamRows.Count - 1 - rowIndex >= Buffer) continue;
                ClearRow(row, index);
                toLoad += row.Count;
            }
            RaiseLoadMoreThumb();
            await FillImages(toLoad, _emptyCells, _daydreamProvider);
        }

        private void ClearRow(IEnumerable<ImageCell> row, int indexToMark)
        {
            foreach (var c in row)
            {
                _emptyCells.Enqueue(c);
                c.Clear();
            }
            _emptyInfinyRowIndexes.Add(indexToMark);
        }

        private static void MoveRowTo(List<ImageCell> row, int newIndex)
            => row.ForEach(c => Grid.SetRow(c, newIndex));

        //user select an image
        private async void ThumbImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Polygon)
            {
                //clicked on favorite
                e.Handled = true;
                return;
            }
            _clickedPictureCell = (ImageCell) sender;
            //prevent clicking on empty cells
            if (!_clickedPictureCell.HasSource) return;
            _isCancelable = false;
            Curtain.CurtainDown();
            Curtain.StartLoadingAnimation();
            Curtain.PreviewImage.SetData(_clickedPictureCell.Data);
            _clickedPictureCell.Data.DownloadProgressChanged += (s, args) => { Curtain.AnimateProgressBar(args); };
            RaiseThumbClicked();
            var resolutions = await _clickedPictureCell.Data.GetResolutions().ConfigureAwait(false);
            if (resolutions == null)
            {
                //MessageBox.Show("Operation failed, please try another picture or contact maker.",
                //    "Something happened", MessageBoxButton.OK);
                UiInvoke(Curtain.ShowError);
            }
            else
                UiInvoke(() => Curtain.ShowResolutionLabels(resolutions));
            UiInvoke(Curtain.StopLoadingAnimation);
            _isCancelable = true;
        }

        //res label clicked
        private async void Curtain_LabelClicked(string jpgLink)
        {
            if (IsSettingWallpaper) return;
            IsSettingWallpaper = true;
            Curtain.StartLoadingAnimation();
            RaiseResClicked();
            try
            {
                if (_clickedPictureCell.Data.Scraper.IsJpgUrlNeedParsed)
                {
                    string html =
                        (await _downloader.DownloadDataAsync(jpgLink).ConfigureAwait(false)).ConvertToString();
                    jpgLink = _clickedPictureCell.Data.Scraper.JpgLink(html);
                }
                var bitmap = new Bitmap(await _downloader.DownloadDataAsync(jpgLink).ConfigureAwait(false));
                string name = _clickedPictureCell.Data.WallpaperName;
                if (string.IsNullOrEmpty(name))
                {
                    var split = jpgLink.Split('/');
                    name = split[split.Length - 1];
                }
                RaiseDownloadWallpaperCompleted(new DownloadWallpaperCompletedEventArgs(bitmap, name));
                DesktopWallpaperHelper.SetWallpaper(bitmap, StretchTo);
            }
            catch (Exception ex)
            {
                ExManager.Ex(ex);
                //MessageBox.Show("Failed to set new wallpaper.");
                UiInvoke(Curtain.ShowError);
            }
            finally
            {
                UiInvoke(Curtain.StopLoadingAnimation);
                IsSettingWallpaper = false;
            }
        }

        private void AddCellsAndMapToCollection(Grid grid, List<List<ImageCell>> collection)
        {
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                var row = new List<ImageCell>();
                for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                {
                    var cell = new ImageCell(_favoriteSaver)
                    {
                        Height = CellHeight,
                        Width = CellWidth,
                        ShowSiteName =  DebugMode
                        //ShowSiteName = true
                    };
                    //cell.SetValue(ImageCell.IsImageDownloaded, false);
                    grid.Children.Add(cell);
                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    row.Add(cell);
                }
                collection.Add(row);
            }
        }

        private void AddTableToGrid(Grid grid, int rows, int columns)
        {
            for (int i = 0; i < rows; i++)
            {
                var row = new RowDefinition
                {
                    Height = new GridLength(CellHeight)
                };
                grid.RowDefinitions.Add(row);
            }
            for (int i = 0; i < columns; i++)
            {
                var col = new ColumnDefinition
                {
                    Width = new GridLength(CellWidth)
                };
                grid.ColumnDefinitions.Add(col);
            }
        }

        private void Curtain_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() != typeof(Curtain))
                e.Handled = true;
            //if (e.OriginalSource.GetType() == typeof(TextBlock) || e.OriginalSource.GetType() == typeof(Border) ||
            //    e.OriginalSource.GetType() == typeof(ImageCell) || e.OriginalSource.GetType() == typeof(Polygon))
            //    e.Handled = true;
            else
                Back();
        }

        public bool Back()
        {
            if (!_isCancelable || !Curtain.IsCurtainShown || IsSettingWallpaper) return false;
            //_clickedPictureCell.IsFavorite = Curtain.PreviewImage.IsFavorite;
            Curtain.CurtainUp();
            RaiseCurtainUp();
            return true;
        }

        private static void UiInvoke(Action a) => Application.Current.Dispatcher.Invoke(a);
        private static void UiInvoke(Action<int> a) => Application.Current.Dispatcher.Invoke(a);

        #region Events

        //implement custom Args as needed
        public event EventHandler ThumbClicked;
        protected virtual void RaiseThumbClicked() => ThumbClicked?.Invoke(this, EventArgs.Empty);

        public event EventHandler ResClicked;
        protected virtual void RaiseResClicked() => ResClicked?.Invoke(this, EventArgs.Empty);

        public event EventHandler<DownloadWallpaperCompletedEventArgs> DownloadWallpaperCompleted;

        protected virtual void RaiseDownloadWallpaperCompleted(DownloadWallpaperCompletedEventArgs agrs)
            => DownloadWallpaperCompleted?.Invoke(this, agrs);

        public event EventHandler ThumbDowloaded;
        protected virtual void RaiseThumbDowloaded() => ThumbDowloaded?.Invoke(this, EventArgs.Empty);

        //public event EventHandler RandomPagesDownloaded;
        //protected virtual void RaiseRandomPagesDownloaded() => RandomPagesDownloaded?.Invoke(this, EventArgs.Empty);

        public event EventHandler CurtainUp;
        protected virtual void RaiseCurtainUp() => CurtainUp?.Invoke(this, EventArgs.Empty);

        public event EventHandler LoadMoreThumb;
        protected virtual void RaiseLoadMoreThumb() => LoadMoreThumb?.Invoke(this, EventArgs.Empty);

        public event EventHandler FavoriteShown;
        protected virtual void RaiseFavoriteShown() => FavoriteShown?.Invoke(this, EventArgs.Empty);

        public event EventHandler DayDreamShown;
        protected virtual void RaiseDayDreamShown() => DayDreamShown?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}