using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wally.Day_Dream.Controls
{
    /// <summary>
    ///     Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///     Step 1a) Using this custom control in a XAML file that exists in the current project.
    ///     Add this XmlNamespace attribute to the root element of the markup file where it is
    ///     to be used:
    ///     xmlns:MyNamespace="clr-namespace:Wally.Day_Dream"
    ///     Step 1b) Using this custom control in a XAML file that exists in a different project.
    ///     Add this XmlNamespace attribute to the root element of the markup file where it is
    ///     to be used:
    ///     xmlns:MyNamespace="clr-namespace:Wally.Day_Dream;assembly=Wally.Day_Dream"
    ///     You will also need to add a project reference from the project where the XAML file lives
    ///     to this project and Rebuild to avoid compilation errors:
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///     Step 2)
    ///     Go ahead and use your control in the XAML file.
    ///     <MyNamespace:ColorfulBar />
    /// </summary>
    internal class ColorfulProgressBar : ProgressBar
    {
        private const int MinBrightness = 76;

        private static readonly Random rnd = new Random();

        static ColorfulProgressBar()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorfulBar),
            //    new FrameworkPropertyMetadata(typeof(ColorfulBar)));
        }

        public ColorfulProgressBar()
        {
            Foreground = new SolidColorBrush(RandomColor(MinBrightness));
        }

        private static Color RandomColor(byte minBrightness)
        {
            return
                Color.FromRgb(byte.Parse(rnd.Next(minBrightness, byte.MaxValue).ToString()),
                    byte.Parse(rnd.Next(minBrightness, byte.MaxValue).ToString()),
                    byte.Parse(rnd.Next(minBrightness, byte.MaxValue).ToString()));
        }

        public void RandomizeForegroundColor()
        {
            Foreground = new SolidColorBrush(RandomColor(MinBrightness));
        }
    }
}