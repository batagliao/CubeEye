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

namespace TheApplication.Interop
{
    public class FileSystemObjectInfo : BaseObject
    {
        public FileSystemObjectInfo(FileSystemInfo info) 
        {
            if (this is DummyFileSystemObjectInfo)
            {
                return;
            }

            Children = new ObservableCollection<FileSystemObjectInfo>();
            FileSystemInfo = info;

            if (info is DirectoryInfo)
            {
                ImageSource = FolderManager.GetImageSource(info.FullName, ItemState.Close);
                AddDummy();
            }
            else if (info is FileInfo)
            {
                ImageSource = FileManager.GetImageSource(info.FullName);
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
                        ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Open);
                        if (HasDummy())
                        {
                            RaiseBeforeExplore();
                            RemoveDummy();
                            ExploreDirectories();
                            ExploreFiles();
                            RaiseAfterExplore();
                        }
                    }
                    else
                    {
                        ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Close);
                    }
                    RaiseAfterExpand();
                }
            }
        }
        #endregion

        public ObservableCollection<FileSystemObjectInfo> Children
        {
            get => GetValue<ObservableCollection<FileSystemObjectInfo>>(nameof(Children));
            set => SetValue(nameof(Children), value);
        }

        public IImage ImageSource
        {
            get => GetValue<IImage>(nameof(ImageSource));
            set => SetValue(nameof(ImageSource), value);
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
                var files = ((DirectoryInfo)FileSystemInfo).GetFiles();
                foreach (var file in files.OrderBy(d => d.Name))
                {
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
