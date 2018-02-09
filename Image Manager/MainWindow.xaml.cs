using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        public Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();
        private List<BitmapImage> cachedImages = new List<BitmapImage>();

        private int currentImageNum = 0;
        private string currentContentType = "null";

        private bool setFocus = false;


        public MainWindow()
        {
            InitializeComponent();
        }

        // Handler for dropping files
        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            // Finds all filepaths of a dropped object
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            FindFilesInSubfolders(e, folder);

            AddToCache();

            UpdateContent();

        }

        // Store added images in a cache
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
            if (FileType(curItem) == "image" && cache.ContainsKey(curItem))
            {
                imageViewer.Source = cache[curItem];
                currentContentType = "image";

                imageViewer.Visibility = Visibility.Visible;
                textViewer.Visibility = Visibility.Hidden;
            }
            else if (FileType(curItem) == "text")
            {
                textViewer.Text = "\n\n" + File.ReadAllText(curItem);
                currentContentType = "text";

                imageViewer.Visibility = Visibility.Hidden;
                textViewer.Visibility = Visibility.Visible;
            }
            else if (FileType(curItem) == "video")
            {
                imageViewer.Source = cache[curItem];
                currentContentType = "video";
                
                imageViewer.Visibility = Visibility.Visible;
                textViewer.Visibility = Visibility.Hidden;
            }
        }

        // Recursively finds all files and subfolders in a folder
        private void FindFilesInSubfolders(DragEventArgs e, string[] folder)
        {
            foreach (var s in folder)
            {
                if (Directory.Exists(s))
                {
                    foreach (string foundFile in Directory.GetFiles(s, "*.*", SearchOption.AllDirectories))
                    {
                        if (!filepaths.Contains(foundFile))
                        {
                            newFiles.Add(foundFile);
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