using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Image_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constants
        private const string rootTitleText = "[ROOT FOLDER]";
        private const string prevDirTitleText = "[THIS FOLDER]";

        SolidColorBrush defaultTextColor = new SolidColorBrush(Colors.White);
        SolidColorBrush setTextColor = new SolidColorBrush(Colors.Orange);
        SolidColorBrush artistTextColor = new SolidColorBrush(Colors.Yellow);
        SolidColorBrush mangaTextColor = new SolidColorBrush(Colors.MediumPurple);


        // Variables
        public static List<string> filepaths = new List<string>();
        public static List<string> newFiles = new List<string>();
        public static List<string> folderPaths = new List<string>();

        public static Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();
        private List<BitmapImage> cachedImages = new List<BitmapImage>();

        private static int currentImageNum = 0;
        private string currentContentType = "null";

        private bool setFocus = false;
        private bool allowSubDir = true;

        private bool establishedRoot = false;
        private string rootFolder;
        private string currentFolder;


        public MainWindow()
        {
            InitializeComponent();

            // Remove harmless error messages from output
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;

        }

        private void MakeArchiveTree(string folder)
        {
            DirectoryTreeList.Items.Clear();
            DirectoryTreeList.Items.Clear();

            string compareRoot = new DirectoryInfo(rootFolder).FullName;

            // Converts string to valid file path
            currentFolder = currentFolder.Replace("System.Windows.Controls.ListBoxItem: ", "");
            folder = folder.Replace("System.Windows.Controls.ListBoxItem: ", "");

            if (compareRoot != new DirectoryInfo(currentFolder).FullName &&
                new DirectoryInfo(currentFolder).FullName != compareRoot + "\\")
            {
                DirectoryTreeList.Items.Add(new ListBoxItem
                {
                    Content = prevDirTitleText,
                    FontWeight = FontWeights.Bold
                });
            }
            else
            {
                DirectoryTreeList.Items.Add(new ListBoxItem
                {
                    Content = rootTitleText,
                    FontWeight = FontWeights.Bold
                });
            }

            foreach (string foundFolder in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
            {
                string shortDir = Path.GetFileName(foundFolder);
                SolidColorBrush color = new SolidColorBrush(Colors.White);

                // Color directories based on content
                if (shortDir.Contains("[Set]"))
                {
                    color = setTextColor;
                }
                else if (shortDir.Contains("[Artist]"))
                {
                    color = artistTextColor;
                }
                else if (shortDir.Contains("[Manga]"))
                {
                    color = mangaTextColor;
                }

                DirectoryTreeList.Items.Add(new ListBoxItem
                {
                    Content = shortDir,
                    Foreground = color
                });
            }
            DirectoryTreeList.Items.Refresh();
        }




        public static int returnCurrentImageNum()
        {
            return currentImageNum;
        }

        // Return type of file as a string
        public static string FileType(string inputFile)
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
            CacheHandler.UpdateCache();
            CacheHandler.lastPos = currentImageNum;

            // Don't display an empty directory
            if (filepaths.Count == 0)
            {
                return;
            }

            string curItem = filepaths[currentImageNum];
            currentContentType = FileType(curItem);

            UpdateTitle();            

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

        private void UpdateTitle()
        {
            string curItem = filepaths[currentImageNum];
            Title = allowSubDir ? "(" + (currentImageNum + 1) + "/" + filepaths.Count + ") " + Path.GetFileName(curItem) :
                "(" + (currentImageNum + 1) + "/" + filepaths.Count + ") " + " -subdir | " + Path.GetFileName(curItem);
        }

        // Adds all valid files to a list
        private void InitializeValidFiles()
        {
            foreach (var item in newFiles)
            {
                if (FileType(item) == "image" || FileType(item) == "text" || FileType(item) == "video")
                {
                    filepaths.Add(item);
                }
            }
            newFiles.Clear();
        }

        // Recursively finds all files and subfolders in a folder
        private void FindFilesInSubfolders(string[] folder)
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
                    if (allowSubDir)
                    {
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
                    else
                    {
                        foreach (string foundFile in Directory.GetFiles(s, "*.*", SearchOption.TopDirectoryOnly))
                        {
                            if (!filepaths.Contains(foundFile))
                            {
                                newFiles.Add(foundFile);
                            }
                        }
                        // Folders
                        foreach (string foundFolder in Directory.GetDirectories(s, "*", SearchOption.TopDirectoryOnly))
                        {
                            if (!folderPaths.Contains(foundFolder))
                            {
                                folderPaths.Add(foundFolder);
                            }
                        }
                    }
                }
                else if (File.Exists(s))
                {
                    // Sets root folder to work with
                    if (establishedRoot == false)
                    {
                        establishedRoot = true;
                        rootFolder = currentFolder = Directory.GetParent(s).ToString();

                    }
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

        // Handler for drag-dropping files
        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            // Finds all filepaths of a dropped object
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            CreateNewContext(folder);
        }

        private void CreateNewContext(string[] folder)
        {
            RemoveOldContext();

            FindFilesInSubfolders(folder);

            InitializeValidFiles();

            UpdateContent();

            MakeArchiveTree(currentFolder);
        }

        private void RemoveOldContext()
        {
            imageViewer.Source = null;
            currentImageNum = 0;
            Title = "Image Manager";
            cache.Clear();
            folderPaths.Clear();
            filepaths.Clear();
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
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

        private void ControlWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Toggle focus
                case Key.Enter:
                    ToggleFocus();
                    break;

                case Key.LeftShift:
                    allowSubDir = !allowSubDir;
                    UpdateTitle();
                    break;

                case Key.Tab:
                    Visibility vis = (DirectoryTreeList.Visibility == Visibility.Hidden) ?
                        (Visibility.Visible) : (Visibility.Hidden);
                    DirectoryTreeList.Visibility = vis;
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

        // Double click for special action
        private void ControlWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleFocus();
            }
        }

        // A right click opens the selected directory in the gallery
        private void DirectoryTreeList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                string[] folder = new string[1];
                currentFolder = currentFolder + "\\" + item.Content;

                if ((string)item.Content == rootTitleText)
                {
                    currentFolder = rootFolder;
                }
                if ((string)item.Content == prevDirTitleText)
                {
                    currentFolder = Path.GetFullPath(Path.Combine(currentFolder, "..\\"));
                }

                folder[0] = currentFolder;
                CreateNewContext(folder);
            }
            //e.Handled = true; 
        }

        // Explores the selected gallery
        private void DirectoryTreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DirectoryTreeList.SelectedIndex != 0)
            {
                currentFolder = currentFolder + "\\" + DirectoryTreeList.SelectedItem;
                MakeArchiveTree(currentFolder);
            }
            else
            {
                ListBoxItem lb = (ListBoxItem) DirectoryTreeList.SelectedItem;
                if ((string) lb.Content != rootTitleText)
                {
                    currentFolder = Path.GetFullPath(Path.Combine(currentFolder, "..\\"));
                    MakeArchiveTree(currentFolder);
                }
            }
        }

        private void ControlWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Toggles the directory box with a mouse wheel click
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                Visibility vis = (DirectoryTreeList.Visibility == Visibility.Hidden) ?
                        (Visibility.Visible) : (Visibility.Hidden);
                DirectoryTreeList.Visibility = vis;
            }
        }
    }
}