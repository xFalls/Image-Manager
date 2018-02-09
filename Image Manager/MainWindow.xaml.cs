using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ThumbnailGenerator;

namespace Image_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variables
        private List<string> filepaths = new List<string>();
        private List<string> newFiles = new List<string>();
        private List<string> folderPaths = new List<string>();

        public Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();
        private List<BitmapImage> cachedImages = new List<BitmapImage>();

        private int currentImageNum = 0;
        private string currentContentType = "null";

        private bool setFocus = false;

        private bool establishedRoot = false;
        private string rootFolder;
        private string currentFolder;

        private string rootTitleText = "___________________";


        public MainWindow()
        {
            InitializeComponent();
        }

        private void MakeArchiveTree(string folder)
        {
            DirectoryTreeList.Items.Clear();
            DirectoryTreeList.Items.Clear();

            //DirectoryTreeList.Items.Add(new DirectoryInfo(currentFolder).Name);
            Console.WriteLine(new DirectoryInfo(currentFolder).FullName);
            Console.WriteLine(new DirectoryInfo(rootFolder).FullName);

            string compareRoot = new DirectoryInfo(rootFolder).FullName;

            if (compareRoot != new DirectoryInfo(currentFolder).FullName && 
                new DirectoryInfo(currentFolder).FullName != compareRoot + "\\")
            {
                DirectoryTreeList.Items.Add("BACK");
            }
            else
            {
                DirectoryTreeList.Items.Add(rootTitleText);
            }
            foreach (string foundFolder in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
            {
                string shortDir = Path.GetFileName(foundFolder);
                DirectoryTreeList.Items.Add(shortDir);
            }
            DirectoryTreeList.Items.Refresh();
        }

        // Store added images in a cache
        // TODO - Limit cache size
        private void AddToCache()
        {
            foreach (var item in newFiles)
            {
                if (FileType(item) == "image")
                {
                    filepaths.Add(item);
                    BitmapImage imageToCache = new BitmapImage(new Uri(item, UriKind.RelativeOrAbsolute));
                    cache.Add(item, imageToCache);
                }
                else if (FileType(item) == "text")
                {
                    filepaths.Add(item);
                }
                else if (FileType(item) == "video")
                {
                    filepaths.Add(item);

                    // Grab thumbnail from video and cache it
                    int THUMB_SIZE = 1024;
                    Bitmap thumbnail = WindowsThumbnailProvider.GetThumbnail(
                       item, THUMB_SIZE, THUMB_SIZE, ThumbnailOptions.BiggerSizeOk);

                    BitmapImage thumbnailImage = BitmapToImageSource(thumbnail);
                    cache.Add(item, thumbnailImage);

                    thumbnail.Dispose();
                }
            }
            newFiles.Clear();
        }

        // Return type of file as a string
        private string FileType(string inputFile)
        {
            string temp = inputFile.ToLower();
            if (temp.EndsWith(".jpg") || temp.EndsWith(".jpeg") || temp.EndsWith(".tif") ||
                    temp.EndsWith(".tiff") || temp.EndsWith(".png") || temp.EndsWith(".gif") ||
                    temp.EndsWith(".bmp") || temp.EndsWith(".ico") || temp.EndsWith(".wmf") ||
                    temp.EndsWith(".emf") || temp.EndsWith(".webp"))
            {
                return "image";
            }

            else if (temp.EndsWith(".txt"))
            {
                return "text";
            }

            else if (temp.EndsWith(".mp4") || temp.EndsWith(".mkv") || temp.EndsWith(".webm")
                 || temp.EndsWith(".wmv") || temp.EndsWith(".flv") || temp.EndsWith(".avi"))
            {
                return "video";
            }

            return "";
        }

        // Changes the currently displayed content
        private void UpdateContent()
        {
            string curItem = filepaths[currentImageNum];
            currentContentType = FileType(curItem);

            if ((currentContentType == "image" || currentContentType == "video") && cache.ContainsKey(curItem))
            {
                imageViewer.Source = cache[curItem];

                imageViewer.Visibility = Visibility.Visible;
                textViewer.Visibility = Visibility.Hidden;
            }
            else if (currentContentType == "text")
            {
                textViewer.Text = "\n\n" + File.ReadAllText(curItem);

                imageViewer.Visibility = Visibility.Hidden;
                textViewer.Visibility = Visibility.Visible;
            }
        }

        // Recursively finds all files and subfolders in a folder
        private void FindFilesInSubfolders(DragEventArgs e, string[] folder)
        {
            foreach (var s in folder)
            {
                if (Directory.Exists(s))
                {
                    // Sets initial root folder to work with
                    if (establishedRoot == false)
                    {
                        establishedRoot = true;
                        rootFolder = currentFolder = s;
                    }
                    // Files
                    foreach (string foundFile in Directory.GetFiles(s, "*.*", SearchOption.AllDirectories))
                    {
                        if (!filepaths.Contains(foundFile))
                        {
                            newFiles.Add(foundFile);
                        }
                    }
                    // Folders
                    foreach (string foundFolder in Directory.GetDirectories(s, "*", SearchOption.AllDirectories))
                    {
                        if (!folderPaths.Contains(foundFolder))
                        {
                            folderPaths.Add(foundFolder);
                        }
                    }
                }
                else if (File.Exists(s))
                {
                    // Add filepath
                    if (!filepaths.Contains(s))
                    {
                        newFiles.Add(s);
                    }
                }
            }
        }

        private void ToggleFocus()
        {
            // Text viewer
            if (currentContentType == "text")
            {
                setFocus = !setFocus;
                textViewer.IsEnabled = setFocus;
            }

            // Start video in default player
            else if (currentContentType == "video")
            {
                Process.Start(filepaths[currentImageNum]);
            }
        }

        // Handler for dropping files
        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            // Finds all filepaths of a dropped object
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            FindFilesInSubfolders(e, folder);

            AddToCache();

            UpdateContent();

            MakeArchiveTree(rootFolder);
        }

        private void ControlWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Toggle focus
                case Key.Enter:
                    ToggleFocus();
                    break;

                // Previous image
                case Key.Left:
                    if (currentImageNum > 0 && !(setFocus && currentContentType == "text"))
                    {
                        currentImageNum--;
                        UpdateContent();
                    }
                    break;

                // Next image
                case Key.Right:
                    if (currentImageNum + 1 < filepaths.Count && !(setFocus && currentContentType == "text"))
                    {
                        currentImageNum++;
                        UpdateContent();
                    }
                    break;
            }
        }

        private void ControlWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Disable switching image when on a focused text item
            if (setFocus && currentContentType == "text")
            {
                return;
            }
            if (e.Delta > 0 && currentImageNum > 0)
            {
                currentImageNum--;
                UpdateContent();
            }
            else if (e.Delta < 0 && currentImageNum + 1 < filepaths.Count)
            {
                currentImageNum++;
                UpdateContent();
            }
        }

        private void ControlWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleFocus();
            }
        }

        private void DirectoryTreeList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DirectoryTreeList.SelectedIndex != 0)
            {
                currentFolder = currentFolder + "\\" + DirectoryTreeList.SelectedItem;
                MakeArchiveTree(currentFolder);
            }
            else
            {
                if ((string) DirectoryTreeList.SelectedItem != rootTitleText)
                {
                    currentFolder = Path.GetFullPath(Path.Combine(currentFolder, "..\\"));
                    MakeArchiveTree(currentFolder);
                }
            }        
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}