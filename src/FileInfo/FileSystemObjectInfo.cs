using TheApplication.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Avalonia.Media;
using Avalonia.Styling;

namespace TheApplication.FileInfo
{
    public class FileSystemObjectInfo : BaseObject
    {
        private ThemeVariant currentTheme;

        private string[] _allowed_extensions = [".obj", ".stl", ".3mf"];

        public FileSystemObjectInfo(FileSystemInfo info) 
        {
            if (this is DummyFileSystemObjectInfo)
            {
                return;
            }

            Children = new ObservableCollection<FileSystemObjectInfo>();
            FileSystemInfo = info;

            SetIcon();

            if (info is DirectoryInfo)
            {
                AddDummy();
            }           

            PropertyChanged += new PropertyChangedEventHandler(FileSystemObjectInfo_PropertyChanged);
        }

        public FileSystemObjectInfo(DriveInfo drive) : this(drive.RootDirectory) { }

        #region events
        public event EventHandler? BeforeExpand;
        public event EventHandler? AfterExpand;
        public event EventHandler? BeforeExplore;
        public event EventHandler? AfterExplore;

        private void RaiseBeforeExpand()
        {
            BeforeExpand?.Invoke(this, EventArgs.Empty);
            SetIcon();
        }

        private void RaiseAfterExpand()
        {
            AfterExpand?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseBeforeExplore()
        {
            BeforeExplore?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseAfterExplore()
        {
            AfterExplore?.Invoke(this, EventArgs.Empty);
        }


        void FileSystemObjectInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (FileSystemInfo is DirectoryInfo)
            {
                if (string.Equals(e.PropertyName, nameof(IsExpanded), StringComparison.CurrentCultureIgnoreCase))
                {
                    RaiseBeforeExpand();
                    if (IsExpanded)
                    {
                        if (HasDummy())
                        {
                            RaiseBeforeExplore();
                            RemoveDummy();
                            ExploreDirectories();
                            ExploreFiles();
                            RaiseAfterExplore();
                        }
                    }
                    RaiseAfterExpand();
                }
            }
        }
        #endregion

        private void SetIcon()
        {
            object resource;            
            if (FileSystemInfo is DirectoryInfo) 
            {
                var dirinfo = FileSystemInfo as DirectoryInfo;
                if (dirinfo!.Parent == null)
                {
                    Avalonia.Application.Current!.TryGetResource("DriveIconGeometry", currentTheme, out resource!);
                }
                else if (IsExpanded)
                {
                    Avalonia.Application.Current!.TryGetResource("FolderOpenGeometry", currentTheme, out resource!);
                }
                else
                {
                    Avalonia.Application.Current!.TryGetResource("FolderClosedGeometry", currentTheme, out resource!);
                }
            }
            else //(FileSystemInfo is FileInfo)
            {
                Avalonia.Application.Current!.TryGetResource("FileIconGeometry", currentTheme, out resource!);
            }
  
            IconGeometry = resource as Avalonia.Media.Geometry;
        }

        public ObservableCollection<FileSystemObjectInfo> Children
        {
            get => GetValue<ObservableCollection<FileSystemObjectInfo>>(nameof(Children));
            set => SetValue(nameof(Children), value);
        }

        public Avalonia.Media.Geometry IconGeometry { 
            get => GetValue<Avalonia.Media.Geometry>(nameof(IconGeometry));
            set => SetValue(nameof(IconGeometry), value);
        }

        public bool IsExpanded
        {
            get => GetValue<bool>(nameof(IsExpanded));
            set => SetValue(nameof(IsExpanded), value);
        }

        public FileSystemInfo FileSystemInfo
        {
            get => GetValue<FileSystemInfo>(nameof(FileSystemInfo));
            set => SetValue(nameof(FileSystemInfo), value);
        }

        private DriveInfo Drive
        {
            get => GetValue<DriveInfo>(nameof(Drive));
            set => SetValue(nameof(Drive), value);
        }


        private void AddDummy()
        {
            Children.Add(new DummyFileSystemObjectInfo());
        }

        private DummyFileSystemObjectInfo? GetDummy()
        {
            return Children.OfType<DummyFileSystemObjectInfo>().FirstOrDefault();
        }

        private void RemoveDummy()
        {
            Children.Remove(GetDummy()!);
        }

        private bool HasDummy()
        {
            return GetDummy() is not null;
        }

        private void ExploreDirectories()
        {
            if (Drive?.IsReady == false) return;

            if (FileSystemInfo is DirectoryInfo dirInfo)
            {
                var directories = dirInfo.GetDirectories();
                foreach (var directory in directories.OrderBy(d => d.Name))
                {
                    // TODO: Have an option to show system and hidden
                    if ((directory.Attributes & FileAttributes.System) != FileAttributes.System &&
                        (directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        var fileSystemObject = new FileSystemObjectInfo(directory);
                        fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                        fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                        Children.Add(fileSystemObject);
                    }
                }
            }
        }

        private void FileSystemObject_AfterExplore(object? sender, EventArgs e)
        {
            RaiseAfterExplore();
        }

        private void FileSystemObject_BeforeExplore(object? sender, EventArgs e)
        {
            RaiseBeforeExplore();
        }

        private void ExploreFiles()
        {
            if (Drive?.IsReady == false)
            {
                return;
            }
            if (FileSystemInfo is DirectoryInfo)
            {
                var files = ((DirectoryInfo)FileSystemInfo).GetFiles().Where(f => _allowed_extensions.Contains(f.Extension));
                foreach (var file in files.OrderBy(d => d.Name))
                {
                    // TODO: have an option to show system and hidden
                    if ((file.Attributes & FileAttributes.System) != FileAttributes.System &&
                        (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        Children.Add(new FileSystemObjectInfo(file));
                    }
                }
            }
        }
    }
}
