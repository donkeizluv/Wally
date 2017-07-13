using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Label = System.Windows.Controls.Label;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace Wally.Day_Dream.Controls
{
    /// <summary>
    ///     Interaction logic for BCanvas.xaml
    /// </summary>
    internal partial class Curtain : Grid
    {
        public delegate void LabelClickedDelegate(string jpgUrl);

        private const int FakeProgress = 25;

        public Curtain()
        {
            InitializeComponent();
            Background.Opacity = 0.9;
            Preview = PreviewImage;
            AddLabelToList();
            //BeginStoryboard((Storyboard) TryFindResource("AnimateFade"));
            foreach (var lb in ResolutionLabels)
                lb.MouseLeftButtonDown += Lb_MouseLeftButtonDown;
        }

        //public ComboBox StyleCombobox { get; private set; }
        //public CheckBox SaveCbox { get; private set; }
        //public System.Windows.Controls.ProgressBar LoadingAnimation { get; private set; }
        public ImageCell Preview { get; private set; }
        public List<Label> ResolutionLabels { get; } = new List<Label>();
        public bool IsCurtainShown { get; private set; }

        public event LabelClickedDelegate LabelClicked;
        protected virtual void RaiseLabelClicked(string jpgUrl) => LabelClicked?.Invoke(jpgUrl);

        public void AnimateProgressBar(DownloadProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.TotalBytesToReceive < 1)
                {
                    progressBar.Value += FakeProgress;
                }
                else
                {
                    progressBar.Value = e.ProgressPercentage;
                }
            }));
        }

        private void AddLabelToList()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var itemsInFirstRow = labelGrid.Children
                        .Cast<Label>()
                        .Where(element => GetRow(element) == i && GetColumn(element) == j);
                    itemsInFirstRow.ToList().ForEach(item => ResolutionLabels.Add(item));
                }
            }
        }

        public void ShowResolutionLabels(IEnumerable<ResolutionCapsule> resolutions)
        {
            int i = 0;
            foreach (var res in resolutions)
            {
                var lb = ResolutionLabels[i];
                lb.Content = res.ResolutionValue;
                var myBrush = new SolidColorBrush(ResolutionRanking(res.ResolutionValue));
                lb.Foreground = myBrush;
                lb.Visibility = Visibility.Visible;
                lb.Opacity = 1;
                lb.DataContext = res.ResolutionUrl;
                i++;
            }
        }

        private void Lb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var label = (Label) sender;
            RaiseLabelClicked(label.DataContext.ToString()); //TODO: use Dproperty
        }

        public void StartLoadingAnimation()
        {
            ResolutionLabels.ForEach(label => label.Opacity = 0);
            ShowProgessBar();
        }

        public void StopLoadingAnimation()
        {
            ResolutionLabels.ForEach(label => label.Opacity = 1);
            HideProgressBar();
        }

        private void ShowProgessBar()
        {
            progressBar.Visibility = Visibility.Visible;
            //progressBar.RandomizeForegroundColor();
        }

        private void HideProgressBar()
        {
            //Preview.HideProgressBar();
            progressBar.Visibility = Visibility.Hidden;
            progressBar.Value = 0;
        }

        private void HideAllResLabels()
        {
            foreach (var lb in ResolutionLabels)
                lb.Visibility = Visibility.Hidden;
        }

        public void CurtainUp()
        {
            IsCurtainShown = false;
            HideAllResLabels();
            Visibility = Visibility.Hidden;
            ErrorLabel.Visibility = Visibility.Hidden;
        }

        public void CurtainDown()
        {
            IsCurtainShown = true;
            Visibility = Visibility.Visible;
        }

        public void ShowError()
        {
            ErrorLabel.Content = "Something went wrong, check back later.";
            ErrorLabel.Visibility = Visibility.Visible;
        }
        private static Color ResolutionRanking(string res)
        {
            var w = res.Split('x');

            float width = float.Parse(w[0]);
            float height = float.Parse(w[1]);
            float ratio = width/height;

            float screenWidth = Convert.ToSingle(Screen.PrimaryScreen.Bounds.Width);
            float screenHeight = Convert.ToSingle(Screen.PrimaryScreen.Bounds.Height);
            float screenRatio = screenWidth/screenHeight;

            if (width + height >= screenWidth + screenHeight)
            {
                if (ratio == screenRatio)
                {
                    return Color.FromRgb(0, 255, 0); //best
                }
                return Math.Abs(ratio - screenRatio) < 0.2 ? Color.FromRgb(50, 225, 50) : Color.FromRgb(200, 200, 200);
            }
            if (ratio == screenRatio)
            {
                return Color.FromRgb(50, 200, 50); // a bit blurry
            }
            return Math.Abs(ratio - screenRatio) < 0.2 ? Color.FromRgb(100, 150, 25) : Color.FromRgb(120, 100, 100);
        }
    }

    internal class ProgressToAngleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = (double) values[0];
            var bar = values[1] as ProgressBar;

            return 359.999*(progress/(bar.Maximum - bar.Minimum));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class Arc : Shape
    {
        // Using a DependencyProperty as the backing store for StartAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc),
                new UIPropertyMetadata(0.0, UpdateArc));

        // Using a DependencyProperty as the backing store for EndAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(Arc), new UIPropertyMetadata(90.0, UpdateArc));

        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(SweepDirection), typeof(Arc),
                new UIPropertyMetadata(SweepDirection.Clockwise));

        public static readonly DependencyProperty OriginRotationDegreesProperty =
            DependencyProperty.Register("OriginRotationDegrees", typeof(double), typeof(Arc),
                new UIPropertyMetadata(270.0, UpdateArc));

        public double StartAngle
        {
            get { return (double) GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public double EndAngle
        {
            get { return (double) GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        //This controls whether or not the progress bar goes clockwise or counterclockwise
        public SweepDirection Direction
        {
            get { return (SweepDirection) GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        //rotate the start/endpoint of the arc a certain number of degree in the direction
        //ie. if you wanted it to be at 12:00 that would be 270 Clockwise or 90 counterclockwise
        public double OriginRotationDegrees
        {
            get { return (double) GetValue(OriginRotationDegreesProperty); }
            set { SetValue(OriginRotationDegreesProperty, value); }
        }

        protected override Geometry DefiningGeometry
        {
            get { return GetArcGeometry(); }
        }

        protected static void UpdateArc(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var arc = d as Arc;
            arc.InvalidateVisual();
        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    drawingContext.DrawGeometry(null, new Pen(Stroke, StrokeThickness), GetArcGeometry());
        //}
        protected override void OnRender(DrawingContext drawingContext)
        {
            // half the width and height of the Arc
            double radiusX = RenderSize.Width/2;
            double radiusY = RenderSize.Height/2;

            // the outlines of the "original" Arc geometry
            var clip = GetArcGeometry().GetWidenedPathGeometry(
                new Pen(Stroke, StrokeThickness));

            // draw only in the area of the original arc
            drawingContext.PushClip(clip);
            drawingContext.DrawEllipse(Stroke, null, new Point(radiusX, radiusY), radiusX, radiusY);
            drawingContext.Pop();
        }

        private Geometry GetArcGeometry()
        {
            var startPoint = PointAtAngle(Math.Min(StartAngle, EndAngle), Direction);
            var endPoint = PointAtAngle(Math.Max(StartAngle, EndAngle), Direction);

            var arcSize = new Size(Math.Max(0, (RenderSize.Width - StrokeThickness)/2),
                Math.Max(0, (RenderSize.Height - StrokeThickness)/2));
            bool isLargeArc = Math.Abs(EndAngle - StartAngle) > 180;

            var geom = new StreamGeometry();
            using (var context = geom.Open())
            {
                context.BeginFigure(startPoint, false, false);
                context.ArcTo(endPoint, arcSize, 0, isLargeArc, Direction, true, false);
            }
            geom.Transform = new TranslateTransform(StrokeThickness/2, StrokeThickness/2);
            return geom;
        }

        private Point PointAtAngle(double angle, SweepDirection sweep)
        {
            double translatedAngle = angle + OriginRotationDegrees;
            double radAngle = translatedAngle*(Math.PI/180);
            double xr = (RenderSize.Width - StrokeThickness)/2;
            double yr = (RenderSize.Height - StrokeThickness)/2;

            double x = xr + xr*Math.Cos(radAngle);
            double y = yr*Math.Sin(radAngle);

            if (sweep == SweepDirection.Counterclockwise)
            {
                y = yr - y;
            }
            else
            {
                y = yr + y;
            }

            return new Point(x, y);
        }
    }
}