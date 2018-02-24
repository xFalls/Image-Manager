using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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

        // Variables
        // public static List<string> filepaths = new List<string>();
        public static List<string> newFiles = new List<string>();
        public static List<string> movedFiles = new List<string>();
        public static List<string> movedFilesOldLocations = new List<string>();
        //public static List<string> folderPaths = new List<string>();
        //private readonly Dictionary<string, string> folderDict = new Dictionary<string, string>();

       
        private string currentContentType = "null";

        
        private bool showSubDir = true;
        private bool showSets = true;

        private bool establishedRoot;
        //private string rootFolder;
        //private string currentFolder;
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


        //////////////

        // List of all stored objects
        private readonly List<DisplayItem> _displayItems = new List<DisplayItem>();
        private readonly List<DisplayItem> _movedItems = new List<DisplayItem>();

        // The index of the displayed item in _displayItems
        private static int displayedItemIndex;
        private DisplayItem currentItem;

        private Folder originFolder;

        private bool isActive;
        private bool isDrop;
        private bool sortMode = false;

        private int cursor = 0;

        //////////////







        public MainWindow()
        {
            InitializeComponent();

            imageTransformGroup.Children.Add(st);
            imageTransformGroup.Children.Add(tt);
            imageViewer.RenderTransform = imageTransformGroup;
            videoBlur.Radius = 20;

            // Adds the folder "Deleted Files" used for moving files to when deleted
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Deleted Files"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Deleted Files");
            deleteFolder = AppDomain.CurrentDomain.BaseDirectory + "Deleted Files";

            // Remove harmless error messages from output
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
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
                    temp.EndsWith(".tiff") || temp.EndsWith(".png") || temp.EndsWith(".bmp") || 
                    temp.EndsWith(".ico") || temp.EndsWith(".wmf") || temp.EndsWith(".emf") || 
                    temp.EndsWith(".webp"))
                return "image";

            if (temp.EndsWith(".gif"))
                return "gif";

            // Text file
            if (temp.EndsWith(".txt"))
                return "text";

            // Video file
            if (temp.EndsWith(".mp4") || temp.EndsWith(".mkv") || temp.EndsWith(".webm")
                 || temp.EndsWith(".wmv") || temp.EndsWith(".flv") || temp.EndsWith(".avi"))
                return "video";

            return "invalidType";
        }

        // Changes the visibility of all UI elements not related to the current displayed item
        private void MakeTypeVisible(string fileType)
        {
            imageViewer.Visibility = gifViewer.Visibility = gifViewer.Visibility =
                    textViewer.Visibility = VideoPlayIcon.Visibility = Visibility.Hidden;
            imageViewer.Effect = null;

            switch (fileType)
            {
                case "image":
                    imageViewer.Visibility = Visibility.Visible;
                    break;
                case "gif":
                    gifViewer.Visibility = Visibility.Visible;
                    break;
                case "video":
                    VideoPlayIcon.Visibility = imageViewer.Visibility = Visibility.Visible;
                    imageViewer.Effect = videoBlur;
                    break;
                case "text":
                    textViewer.Visibility = Visibility.Visible;
                    break;
            }
        }

        // Changes the currently displayed content
        private void UpdateContent()
        {
            if (_displayItems.Count == 0) return;
            
            currentItem = _displayItems[displayedItemIndex];

            ResetView();
            UpdateTitle();
            UpdateInfobar();

            // Makes all irrelevant elements inivisible
            MakeTypeVisible(currentItem.GetTypeOfFile());

            // Preloads images ahead of time
            AddToCache();

            // Gets and show the content
            if (currentItem.GetTypeOfFile() == "image")
                imageViewer.Source = ((ImageItem) currentItem)?.GetImage();
            else if (currentItem.GetTypeOfFile() == "gif")
                gifViewer.Source = ((GifItem) currentItem).GetGif(gifViewer);
            else if (currentItem.GetTypeOfFile() == "video")
                imageViewer.Source = ((VideoItem) currentItem).GetThumbnail();
            else if (currentItem.GetTypeOfFile() == "text")
                textViewer.Text = ((TextItem) currentItem).GetText();
        }

        /// <summary>
        /// Zooms in or out of the current viewed image
        /// </summary>
        /// <param name="zoomAmount">The amount to zoom, where 0.5 is 50% and 2 is 200% zoomed</param>
        public void Zoom(double zoomAmount)
        {
            // Only allow zooming in an image
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

        

        // Adds all valid new files to a list
        private void ProcessNewFiles()
        {
            foreach (var item in newFiles)
            {
                string fileType = FileType(item);
                switch (fileType)
                {
                    case "image":
                        _displayItems.Add(new ImageItem(item));
                        isInCache.Add(false);
                        break;
                    case "gif":
                        _displayItems.Add(new GifItem(item));
                        isInCache.Add(false);
                        break;
                    case "video":
                        _displayItems.Add(new VideoItem(item));
                        isInCache.Add(false);
                        break;
                    case "text":
                        _displayItems.Add(new TextItem(item));
                        isInCache.Add(false);
                        break;
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
                    if (isDrop) InitializeDrop(s);

                    SearchOption scanFolderStructure =
                        showSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    // Files to add
                    foreach (string foundFile in Directory.GetFiles(s, "*.*", scanFolderStructure))
                    {
                        // Exlude folders started with an underscore
                        if (Path.GetDirectoryName(foundFile).Contains("_")) continue;
                        // Exclude special folders when set to do so
                        if (!showSets && specialFolders.Any(o => Path.GetDirectoryName(foundFile).Contains(o.Key))) continue;

                        newFiles.Add(foundFile);
                    }
                }
                else if (File.Exists(s))
                {
                    if (isDrop) InitializeDrop(Path.GetDirectoryName(s));

                    // Add filepath
                    newFiles.Add(s);
                }
            }
        }

        private void InitializeDrop(string s)
        {
            // The folder highest in the tree
            originFolder = new Folder(s);

            isDrop = false;
        }

        // Handler for drag-dropping files
        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            // Finds all filepaths of a dropped object
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            displayedItemIndex = 0;
            DirectoryTreeList.Items.Clear();
            originFolder?.GetAllFolders()?.Clear();
            originFolder?.GetAllShownFolders()?.Clear();
            RemoveOldContext();
            isDrop = true;
            CreateNewContext(folder);
            CreateSortMenu();
        }

        private void ToggleAction()
        {
            if (currentContentType == "text" || currentContentType == "image")
            {
                isActive = !isActive;
                textViewer.IsEnabled = isActive;

                if (isActive == false)
                {
                    ResetView();
                }
            }

            // Start video in default player
            else if (currentContentType == "video")
            {
                Process.Start(_displayItems[displayedItemIndex].GetFilePath());
            }
        }

        /// <summary>
        /// Resets the current image's zoom level and panning to default
        /// </summary>
        public void ResetView()
        {
            tt.Y = 0.5;
            tt.X = 0.5;
            st.ScaleX = 1;
            st.ScaleY = 1;
            currentZoom = 1;
            imageViewer.RenderTransform = imageTransformGroup;
        }


        // Resets the program and starts over with new files
        private void CreateNewContext(string[] folder)
        {
            displayedItemIndex = 0;
            RemoveOldContext();
            FindFilesInSubfolders(folder);
            ProcessNewFiles();
            UpdateContent();
        }

        // Resets the program
        private void RemoveOldContext()
        {
            isActive = false;
            imageViewer.Source = null;

            UpdateTitle();
            ResetView();

            _displayItems.Clear();
            _movedItems.Clear();
            isInCache.Clear();

            GC.Collect();
        }
    }
}