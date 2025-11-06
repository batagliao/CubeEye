using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TheApplication.Interop
{
    public static class FileManager
    {
        public static IImage GetImageSource(string filename)
        {
            return GetImageSource(filename, new Size(16, 16));
        }

        public static IImage GetImageSource(string filename, Size size)
        {
            using (var icon = ShellManager.GetIcon(Path.GetExtension(filename), ItemType.File, IconSize.Small, ItemState.Undefined))
            {
                using (var bitmap = icon.ToBitmap())
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
