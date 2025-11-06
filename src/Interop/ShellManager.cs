using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheApplication.Interop
{
    public class ShellManager
    {
        public static Icon GetIcon(string path, ItemType type, IconSize iconSize, ItemState state)
        {
            var attributes = (uint)(type == ItemType.Folder ? FileAttribute.Directory : FileAttribute.File);
            var flags = (uint)(ShellAttribute.Icon | ShellAttribute.UseFileAttributes);

            if(type == ItemType.Folder && state == ItemState.Open)
            {
                flags |= (uint)ShellAttribute.OpenIcon;
            }
            
            if(iconSize == IconSize.Small)
            {
                flags |= (uint)ShellAttribute.SmallIcon;
            }
            else
            {
                flags |= (uint)ShellAttribute.LargeIcon;
            }

            var fileInfo = new ShellFileInfo();
            var size = (uint)Marshal.SizeOf(fileInfo);
            var result = Shell32.SHGetFileInfo(path, attributes, out fileInfo, size, flags);
            if(result == IntPtr.Zero)
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())!;
            }

            try
            {
                return (Icon)Icon.FromHandle(fileInfo.hIcon).Clone();
            }
            finally
            {
                Shell32.DestroyIcon(fileInfo.hIcon);
            }
        }
    }
}
