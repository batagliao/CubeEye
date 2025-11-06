using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.IO;
using System.Linq;
using TheApplication.Interop;

namespace TheApplication
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Application.Current!.ActualThemeVariantChanged += MainWindow_ActualThemeVariantChanged;
            InitializeComponent();
            InitializeFileSystemObjects();
        }

        private void MainWindow_ActualThemeVariantChanged(object? sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void InitializeFileSystemObjects()
        {
            var drives = DriveInfo.GetDrives();
            DriveInfo.GetDrives().ToList().ForEach(drive =>
            {
                var fileSystemObject = new FileSystemObjectInfo(drive);
                fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                treeView.Items.Add(fileSystemObject);
            });
        }

        private void FileSystemObject_AfterExplore(object sender, System.EventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        private void FileSystemObject_BeforeExplore(object sender, System.EventArgs e)
        {
            Cursor = new Cursor(StandardCursorType.Wait);
        }
    }
}