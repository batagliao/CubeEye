using Avalonia.Media;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TheApplication.Interop
{
    public class FolderManager
    {
        public static IImage GetImageSource(string directory, ItemState folderType)
        {
            return GetImageSource(directory, new Size(16, 16), folderType);
        }

        public static IImage GetImageSource(string directory, Size size, ItemState folderType)
        {
            using(var icon = ShellManager.GetIcon(directory, ItemType.Folder, IconSize.Large, folderType))
            {
                using(var bitmap = icon.ToBitmap())
                {
                    using var resized = new Bitmap(bitmap, new Size(size.Width, size.Height));
                    using var ms = new MemoryStream();
                    resized.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    return new Avalonia.Media.Imaging.Bitmap(ms);
                }
            }
        }
    }
}
