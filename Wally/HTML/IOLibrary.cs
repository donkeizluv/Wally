using System.IO;

namespace Wally.HTML
{
    internal struct IOLibrary
    {
        internal static void CopyAlways(string source, string target)
        {
            if (!File.Exists(source))
            {
                return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            MakeWritable(target);
            File.Copy(source, target, true);
        }

        internal static void MakeWritable(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            File.SetAttributes(path,
                File.GetAttributes(path) &
                (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive |
                 FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary | FileAttributes.SparseFile |
                 FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline |
                 FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream |
                 FileAttributes.NoScrubData));
        }
    }
}