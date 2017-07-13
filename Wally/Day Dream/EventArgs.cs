using System;
using System.Drawing;

namespace Wally.Day_Dream
{
    internal class DownloadWallpaperCompletedEventArgs : EventArgs
    {
        public DownloadWallpaperCompletedEventArgs(Image image, string name)
        {
            Image = image;
            Name = name;
        }

        public Image Image { get; }
        public string Name { get; }
    }

    internal class AddedToFavoriteEventAgrs : EventArgs
    {
        public AddedToFavoriteEventAgrs(string target)
        {
            TargetPageUrl = target;
        }

        public string TargetPageUrl { get; }
    }

    internal class RemovedFromFavoriteEventAgrs : EventArgs
    {
        public RemovedFromFavoriteEventAgrs(string target)
        {
            TargetPageUrl = target;
        }

        public string TargetPageUrl { get; }
    }
}