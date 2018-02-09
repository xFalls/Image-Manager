using System;
using System.Collections.Generic;
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

namespace Image_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> filepaths = new List<string>();
        private List<string> newFiles = new List<string>();

        public Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();
        private List<BitmapImage> cachedImages = new List<BitmapImage>();

        private int currentImageNum = 0;
        private string currentImageType = "null";


        public MainWindow()
        {
            InitializeComponent();
        }

        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            // Finds all filepaths of a dropped object
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            FindFilesInSubfolders(e, folder);

            AddToCache();

            UpdateContent();

        }

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
            }
            newFiles.Clear();
        }

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
            return "";
        }

        // Changes the currently displayed image
        private void UpdateContent()
        {
            string curItem = filepaths[currentImageNum];
            if (FileType(curItem) == "image" && cache.ContainsKey(curItem))
            {
                imageViewer.Source = cache[curItem];
                currentImageType = "image";

                imageViewer.Visibility = Visibility.Visible;
                textViewer.Visibility = Visibility.Hidden;
            }
            else if (FileType(curItem) == "text")
            {
                textViewer.AppendText("\n\n" + File.ReadAllText(curItem));
                currentImageType = "text";

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

        private void ControlWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (currentImageNum > 0)
                    {
                        currentImageNum--;
                        UpdateContent();
                    }
                    break;
                case Key.Right:
                    if (currentImageNum + 1 < filepaths.Count)
                    {
                        currentImageNum++;
                        UpdateContent();
                    }
                    break;
            }
        }

        private void ControlWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (currentImageType == "text")
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
    }
}
