using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using Point = System.Windows.Point;

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

        private SolidColorBrush defaultTextColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush warningTextColor = new SolidColorBrush(Colors.Red);

        private SolidColorBrush setTextColor = new SolidColorBrush(Colors.Orange);
        private SolidColorBrush artistTextColor = new SolidColorBrush(Colors.Yellow);
        private SolidColorBrush mangaTextColor = new SolidColorBrush(Colors.MediumPurple);
        private SolidColorBrush collectionTextColor = new SolidColorBrush(Colors.CornflowerBlue);
        private SolidColorBrush selectionColor = new SolidColorBrush(Colors.Blue);

        // Variables
        public static List<string> filepaths = new List<string>();
        public static List<string> newFiles = new List<string>();
        public static List<string> movedFiles = new List<string>();
        public static List<string> movedFilesOldLocations = new List<string>();
        public static List<string> folderPaths = new List<string>();
        private readonly Dictionary<string, string> folderDict = new Dictionary<string, string>();

        public static Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();

        private static int currentImageNum;
        private string currentContentType = "null";

        private bool setFocus;
        private bool allowSubDir = true;
        private bool showSets = true;

        private bool establishedRoot;
        private string rootFolder;
        private string currentFolder;
        private string deleteFolder;

        private bool isTyping;

        public static string curFileName = "";
        private string curFolderPath = "";

        // Image manipulation
        private Point start;
        private Point origin;
        private double currentZoom = 1;
        TransformGroup imageTransformGroup = new TransformGroup();
        TranslateTransform tt = new TranslateTransform();
        ScaleTransform st = new ScaleTransform();

        BlurEffect videoBlur = new BlurEffect();

        private int guiSelection;
        private int sortGuiSelection;
        private int currentMode;
        CacheHandler cacheHandler = new CacheHandler();



        public MainWindow()
        {
            InitializeComponent();

            imageTransformGroup.Children.Add(st);
            imageTransformGroup.Children.Add(tt);
            imageViewer.RenderTransform = imageTransformGroup;
            videoBlur.Radius = 20;

            // Adds the folder "Deleted Files" used for moving files to when deleted
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Deleted Files"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Deleted Files");
            }

            deleteFolder = AppDomain.CurrentDomain.BaseDirectory + "Deleted Files";

            // Remove harmless error messages from output
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        }



        public static int ReturnCurrentImageNum()
        {
            return currentImageNum;
        }

        /// <summary>
        /// Accepts a file and then returns its type as a string
        /// </summary>
        /// <param name="inputFile">The name of the given file</param>
        /// <returns>A string indicating the type of file given</returns>
        public static string FileType(string inputFile)
        {
            string temp = inputFile.ToLower();

            // Image file
            if (temp.EndsWith(".jpg") || temp.EndsWith(".jpeg") || temp.EndsWith(".tif") ||
                    temp.EndsWith(".tiff") || temp.EndsWith(".png") || temp.EndsWith(".gif") ||
                    temp.EndsWith(".bmp") || temp.EndsWith(".ico") || temp.EndsWith(".wmf") ||
                    temp.EndsWith(".emf") || temp.EndsWith(".webp"))
            {
                return "image";
            }

            // Text file
            if (temp.EndsWith(".txt"))
            {
                return "text";
            }

            // Video file
            if (temp.EndsWith(".mp4") || temp.EndsWith(".mkv") || temp.EndsWith(".webm")
                 || temp.EndsWith(".wmv") || temp.EndsWith(".flv") || temp.EndsWith(".avi"))
            {
                return "video";
            }

            return "invalidType";
        }

        

        // Changes the currently displayed content
        private void UpdateContent()
        {
            try
            {
                cacheHandler.UpdateCache();
            }
            catch
            {
                // ignored
            }

            cacheHandler.lastPos = currentImageNum;

            // Don't display an empty directory
            if (filepaths.Count == 0)
            {
                return;
            }

            ResetView();

            // Update widely used variables
            curFileName = Path.GetFileName(filepaths[currentImageNum]);
            curFolderPath = Path.GetFullPath(filepaths[currentImageNum]);

            string curItem = filepaths[currentImageNum];
            currentContentType = FileType(curItem);

            UpdateTitle();

            if ((currentContentType == "image" || currentContentType == "video") && cache.ContainsKey(curItem))
            {
                try
                {
                    imageViewer.Source = cache[curItem];
                }
                catch
                {
                    imageViewer.Source = null;
                    ResetView();
                    textViewer.Visibility = Visibility.Hidden;
                    imageViewer.Visibility = Visibility.Hidden;
                    VideoPlayIcon.Visibility = Visibility.Hidden;
                    gifViewer.Visibility = Visibility.Hidden;
                    UpdateTitle();
                    UpdateInfobar();
                    return;
                }

                if (curFileName.ToLower().EndsWith(".gif"))
                {
                    gifViewer.Visibility = Visibility.Visible;
                    BitmapImage image = new BitmapImage();
                    using (FileStream stream = File.OpenRead(curItem))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        ImageBehavior.SetAnimatedSource(gifViewer, image);
                        image.EndInit();
                    }
                }
                else
                {
                    gifViewer.Visibility = Visibility.Hidden;
                }

                imageViewer.Effect = null;

                imageViewer.Visibility = Visibility.Visible;
                textViewer.Visibility = Visibility.Hidden;
                VideoPlayIcon.Visibility = Visibility.Hidden;

                if (currentContentType == "video")
                {
                    VideoPlayIcon.Visibility = Visibility.Visible;
                    gifViewer.Visibility = Visibility.Hidden;

                    imageViewer.Effect = videoBlur;
                }
            }
            else if (currentContentType == "text")
            {
                try
                {
                    textViewer.Text = "\n\n" + File.ReadAllText(curItem);

                    imageViewer.Visibility = Visibility.Hidden;
                    textViewer.Visibility = Visibility.Visible;
                    VideoPlayIcon.Visibility = Visibility.Hidden;
                    gifViewer.Visibility = Visibility.Hidden;
                }
                catch
                {
                    // ignored
                }
            }

            UpdateInfobar();
        }

        /// <summary>
        /// Zooms in or out of the current viewed image
        /// </summary>
        /// <param name="zoomAmount">The amount to zoom, where 0.5 is 50% and 2 is 200% zoomed</param>
        private void Zoom(double zoomAmount)
        {
            // Only allow zooming the image
            if (currentContentType != "image") return;
            
            // Disallow zooming beyond the specified limits
            if ((zoomAmount > 0 && currentZoom + zoomAmount >= MaxZoom) || 
                (zoomAmount < 0 && currentZoom + zoomAmount <= MinZoom)) return;

            // Changes the current zoom level and updates the image
            currentZoom += zoomAmount;
            st.ScaleX = currentZoom;
            st.ScaleY = currentZoom;
            imageViewer.RenderTransform = imageTransformGroup;
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

                        folderDict.Clear();

                        rootFolder = currentFolder = s;

                        folderDict.Add(Path.GetFileName(rootFolder), rootFolder);
                        foreach (string foundFolder in Directory.GetDirectories(s, "*.*", SearchOption.AllDirectories))
                            if (!foundFolder.Contains("_"))
                            {
                                folderDict.Add(Path.GetFullPath(foundFolder).TrimEnd('\\').Replace(rootFolder, "").TrimStart('\\'), foundFolder);
                            }
                    }
                    // Files
                    if (allowSubDir)
                    {
                        foreach (string foundFile in Directory.GetFiles(s, "*.*", SearchOption.AllDirectories))
                        {
                            if (filepaths.Contains(foundFile)) continue;
                            if (Path.GetDirectoryName(foundFile).Contains("_")) continue;
                            if (showSets)
                                newFiles.Add(foundFile);
                            else
                            {
                                if ((!Path.GetDirectoryName(foundFile).Contains("[Set]") && !Path.GetDirectoryName(foundFile).Contains("[Manga]") && !Path.GetDirectoryName(foundFile).Contains("[Artist]") && !Path.GetDirectoryName(foundFile).Contains("[Collection]")))
                                    newFiles.Add(foundFile);
                            }
                        }
                        // Folders
                        foreach (string foundFolder in Directory.GetDirectories(s, "*", SearchOption.AllDirectories))
                        {
                            if (folderPaths.Contains(foundFolder)) continue;
                            if (!foundFolder.Contains("_"))
                            {
                                folderPaths.Add(foundFolder);
                            }
                        }
                    }
                    else
                    {
                        foreach (string foundFile in Directory.GetFiles(s, "*.*", SearchOption.TopDirectoryOnly))
                        {
                            if (filepaths.Contains(foundFile)) continue;
                            if (Path.GetDirectoryName(foundFile).Contains("_")) continue;
                            if (showSets)
                                newFiles.Add(foundFile);
                            else
                            {
                                if ((!Path.GetDirectoryName(foundFile).Contains("[Set]") &&
                                     !Path.GetDirectoryName(foundFile).Contains("[Manga]") &&
                                     !Path.GetDirectoryName(foundFile).Contains("[Artist]") &&
                                     !Path.GetDirectoryName(foundFile).Contains("[Collection]")))
                                    newFiles.Add(foundFile);
                            }
                        }
                        // Folders
                        foreach (string foundFolder in Directory.GetDirectories(s, "*", SearchOption.TopDirectoryOnly))
                        {
                            if (!folderPaths.Contains(foundFolder))
                            {
                                if (!foundFolder.Contains("_"))
                                {
                                    folderPaths.Add(foundFolder);
                                }
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

                        folderDict.Clear();

                        rootFolder = currentFolder = Directory.GetParent(s).ToString();

                        folderDict.Add(Path.GetFileName(rootFolder), rootFolder);
                        foreach (string foundFolder in Directory.GetDirectories(rootFolder, "*.*", SearchOption.AllDirectories))
                            if (!foundFolder.Contains("_"))
                                folderDict.Add(
                                    Path.GetFullPath(foundFolder).TrimEnd('\\').Replace(rootFolder, "").TrimStart('\\'),
                                    foundFolder);

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
            if (currentContentType == "text" || currentContentType == "image")
            {
                setFocus = !setFocus;
                textViewer.IsEnabled = setFocus;

                if (setFocus == false)
                {
                    ResetView();
                }
            }

            // Start video in default player
            else if (currentContentType == "video")
            {
                Process.Start(filepaths[currentImageNum]);
            }
        }

        // Resets the zoom level and panning
        private void ResetView()
        {
            tt.Y = 0.5;
            tt.X = 0.5;
            st.ScaleX = 1;
            st.ScaleY = 1;
            currentZoom = 1;
            imageViewer.RenderTransform = imageTransformGroup;
        }

        // Handler for drag-dropping files
        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            // Finds all filepaths of a dropped object
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            RemoveOldContext();
            establishedRoot = false;
            CreateNewContext(folder);
            CompleteFolderTree(rootFolder);
        }

        // Resets the program and starts over with new files
        private void CreateNewContext(string[] folder)
        {
            RemoveOldContext();

            FindFilesInSubfolders(folder);

            InitializeValidFiles();

            UpdateContent();

            MakeArchiveTree(currentFolder);
            CompleteFolderTree(rootFolder);
        }

        // Resets the program
        private void RemoveOldContext()
        {
            setFocus = false;
            imageViewer.Source = null;
            textViewer.Visibility = Visibility.Hidden;
            currentContentType = "";

            currentImageNum = 0;
            guiSelection = 0;
            sortGuiSelection = 0;
            UpdateTitle();
            ResetView();

            DirectoryTreeList.Items.Clear();
            AllFolders.Items.Clear();

            cache.Clear();

            //folderDict.Clear();
            folderPaths.Clear();
            filepaths.Clear();
            newFiles.Clear();
            movedFiles.Clear();
            movedFilesOldLocations.Clear();

            GC.Collect();
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }





        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();

                bitmapimage.BeginInit();
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.StreamSource = memory;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }
}