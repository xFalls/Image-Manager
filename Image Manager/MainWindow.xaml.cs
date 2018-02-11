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
        SolidColorBrush selectionColor = new SolidColorBrush(Colors.Blue);


        // Variables
        public static List<string> filepaths = new List<string>();
        public static List<string> newFiles = new List<string>();
        public static List<string> folderPaths = new List<string>();

        public static Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();
        //private List<BitmapImage> cachedImages = new List<BitmapImage>();

        private static int currentImageNum = 0;
        private string currentContentType = "null";

        private bool setFocus = false;
        private bool allowSubDir = true;

        private bool establishedRoot = false;
        private string rootFolder;
        private string currentFolder;

        private int guiSelection = 0;
        CacheHandler cacheHandler = new CacheHandler();


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

            guiSelection = 0;

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

            repaintSelector();
            DirectoryTreeList.Items.Refresh();
        }

        private void repaintSelector()
        {
            foreach (ListBoxItem item in DirectoryTreeList.Items)
            {
                item.Background = new SolidColorBrush(Colors.Transparent);
            }

            ListBoxItem selectedBox = (ListBoxItem)DirectoryTreeList.Items[guiSelection];
            selectedBox.Background = selectionColor;
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
            cacheHandler.UpdateCache();
            cacheHandler.lastPos = currentImageNum;

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
            if (filepaths.Count > 0)
            {
                string curItem = filepaths[currentImageNum];
                Title = allowSubDir ? "(" + (currentImageNum + 1) + "/" + filepaths.Count + ") " + Path.GetFileName(curItem) :
                    "(" + (currentImageNum + 1) + "/" + filepaths.Count + ") " + " -subdir | " + Path.GetFileName(curItem);
            }
            else
            {
                Title = "Image Manager";
                if (!allowSubDir)
                {
                    Title = "Image Manager -subdir";
                }
            }
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
            textViewer.Visibility = Visibility.Hidden;

            currentImageNum = 0;
            guiSelection = 0;
            UpdateTitle();
            DirectoryTreeList.Items.Clear();
            cache.Clear();
            folderPaths.Clear();
            filepaths.Clear();
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        private void MoveFile()
        {

            if (establishedRoot == false || filepaths.Count == 0)
            {
                return;
            }

            ListBoxItem selectedBoxItem = (ListBoxItem)DirectoryTreeList.Items[guiSelection];
            string currentFileName = Path.GetFileName(filepaths[currentImageNum]);
            string originalPath = filepaths[currentImageNum];
            string newFileName = currentFolder + "\\" + selectedBoxItem.Content.ToString() + "\\" + currentFileName;
            string ext = Path.GetExtension(currentFileName);
            newFileName = newFileName.Replace(rootTitleText, "");
            newFileName = newFileName.Replace(prevDirTitleText, "");

            bool isTopDir = false;

            if (selectedBoxItem.Content.ToString() == rootTitleText || selectedBoxItem.Content.ToString() == prevDirTitleText)
            {
                isTopDir = true;
            }

            // Renames file if file with same name already exists
            // Also prevents the file from being moved into the same folder
            while (true)
            {
                if (File.Exists(newFileName))
                {
                    if (isTopDir == false)
                    {
                        Console.WriteLine(NormalizePath(currentFolder + "\\" + selectedBoxItem.Content.ToString()));
                        Console.WriteLine(NormalizePath(originalPath.TrimEnd('\\').Replace(currentFileName, "").ToString()));
                        
                        string pathToCompare1 = NormalizePath(currentFolder + "\\" + selectedBoxItem.Content.ToString());
                        string pathToCompare2 = NormalizePath(originalPath.TrimEnd('\\').Replace(currentFileName, "").ToString());

                        if (pathToCompare1 == pathToCompare2) break;
                        newFileName = currentFolder + "\\" + selectedBoxItem.Content.ToString() + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
                    }
                    else
                    {
                        if (currentFolder.ToString().TrimEnd('\\') ==
                            newFileName.Replace(currentFileName, "").ToString().TrimEnd('\\')) break;
                        newFileName = currentFolder + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
                    }
                }
                else
                {
                    break;
                }
            }



            File.Move(originalPath, newFileName);

            filepaths.RemoveAt(currentImageNum);

            // When last file has been moved
            if (filepaths.Count == 0)
            {
                string[] refreshFolder = new string[1];
                refreshFolder[0] = rootFolder;
                RemoveOldContext();
                currentFolder = rootFolder;
                CreateNewContext(refreshFolder);
            } else if (currentImageNum == filepaths.Count)
            {
                currentImageNum--;
            }

            UpdateContent();
            UpdateTitle();

            cache.Remove(originalPath);
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

        // Various keyboard shortcuts
        private void ControlWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // Toggle focus, enter selected directory
                case Key.Enter:

                    ToggleFocus();

                    break;

                case Key.E:
                    if (establishedRoot == false)
                    {
                        return;
                    }
                    if (DirectoryTreeList.Visibility == Visibility.Visible)
                    {
                        ListBoxItem selectedBox = (ListBoxItem)DirectoryTreeList.Items[guiSelection];
                        if (guiSelection != 0)
                        {
                            currentFolder = currentFolder + "\\" + selectedBox.Content;
                            MakeArchiveTree(currentFolder);
                        }
                        else
                        {
                            if ((string)selectedBox.Content != rootTitleText)
                            {
                                currentFolder = Path.GetFullPath(Path.Combine(currentFolder, "..\\"));
                                MakeArchiveTree(currentFolder);
                            }
                        }
                    }
                    break;

                case Key.R:
                    MoveFile();
                    break;

                case Key.Q:
                    if (establishedRoot == false)
                    {
                        return;
                    }
                    ListBoxItem firstBox = (ListBoxItem)DirectoryTreeList.Items[0];
                    if (DirectoryTreeList.Visibility == Visibility.Visible && (string)firstBox.Content != rootTitleText)
                    {
                        currentFolder = Path.GetFullPath(Path.Combine(currentFolder, "..\\"));
                        MakeArchiveTree(currentFolder);
                    }
                    break;


                case Key.Space:
                    if (establishedRoot == false)
                    {
                        return;
                    }
                    if (DirectoryTreeList.Visibility == Visibility.Visible)
                    {
                        ListBoxItem selectedBox = (ListBoxItem)DirectoryTreeList.Items[guiSelection];
                        string[] folder = new string[1];
                        currentFolder = currentFolder + "\\" + selectedBox.Content;

                        if ((string)selectedBox.Content == rootTitleText)
                        {
                            currentFolder = rootFolder;
                        }
                        if ((string)selectedBox.Content == prevDirTitleText)
                        {
                            currentFolder = Path.GetFullPath(Path.Combine(currentFolder, "..\\"));
                        }
                        folder[0] = currentFolder;
                        CreateNewContext(folder);
                    }
                    UpdateTitle();
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

                case Key.S:
                    if (DirectoryTreeList.Visibility == Visibility.Visible &&
                        guiSelection + 1 < DirectoryTreeList.Items.Count)
                    {
                        guiSelection++;
                        repaintSelector();
                    }
                    break;

                case Key.W:
                    if (DirectoryTreeList.Visibility == Visibility.Visible &&
                        guiSelection - 1 >= 0)
                    {
                        guiSelection--;
                        repaintSelector();
                    }
                    break;

                // Previous image
                case Key.Left:
                case Key.A:
                    if (currentImageNum > 0 && !(setFocus && currentContentType == "text"))
                    {
                        currentImageNum--;
                        UpdateContent();
                    }
                    break;

                // Next image
                case Key.Right:
                case Key.D:
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
                ListBoxItem lb = (ListBoxItem)DirectoryTreeList.SelectedItem;
                if ((string)lb.Content != rootTitleText)
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