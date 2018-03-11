using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml;
using Image_Manager.Properties;
using Microsoft.VisualBasic;
using Control = System.Windows.Controls.Control;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Image_Manager
{
    /// <summary>
    /// The main window where content is loaded
    /// </summary>
    public partial class MainWindow
    {
        // Sets what type of folders to show
        private bool _showSubDir = true;
        private bool _showSets = true;
        private bool _showPrefix = true;

        // List of all stored objects
        private Folder _originFolder;
        private readonly List<DisplayItem> _displayItems = new List<DisplayItem>();
        private readonly List<DisplayItem> _movedItems = new List<DisplayItem>();
        private List<Image> _previewContainer = new List<Image>();

        // Keeps track of changes in the folder structure
        public static List<string> NewFiles = new List<string>();
        public static List<string> MovedFiles = new List<string>();
        private readonly string _deleteFolder;

        // The index of the displayed item in _displayItems
        private static int _displayedItemIndex;
        private DisplayItem _currentItem;

        // Other toggles
        private bool _isActive;
        private bool _isDrop;
        private bool _sortMode;
        private bool _isTyping;
        private bool _isEndless;

        // Image manipulation
        private Point _start;
        private Point _origin;
        private double _currentZoom = 1;
        private readonly TransformGroup _imageTransformGroup = new TransformGroup();
        private readonly TranslateTransform _tt = new TranslateTransform();
        private readonly ScaleTransform _st = new ScaleTransform();
        private readonly BlurEffect _videoBlur = new BlurEffect();


        public MainWindow()
        {
            // Loads all elements into view
            InitializeComponent();

            // Initializes variables used for zooming and panning
            _imageTransformGroup.Children.Add(_st);
            _imageTransformGroup.Children.Add(_tt);
            imageViewer.RenderTransform = _imageTransformGroup;

            // Sets default values
            _videoBlur.Radius = _defaultBlurRadius;
            DisplayItem.ShortLength = FileNameSize;
            UpdateSettingsChanged();
            UpdatePreviewLength();

            // Default view
            PreviewField.Visibility = Settings.Default.IsPreviewOpen ? Visibility.Visible : Visibility.Hidden;
            ShowSortPreview.IsChecked = PreviewField.Visibility == Visibility.Visible;

            // Adds the folder "Deleted Files" used for moving files to when deleted
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Deleted Files"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Deleted Files");
            _deleteFolder = AppDomain.CurrentDomain.BaseDirectory + "Deleted Files";

            // Remove harmless error messages from output
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;

        }


        public void UpdatePreviewLength()
        {
            PreviewContainer.Children.RemoveRange(0, PreviewContainer.Children.Count);
            _previewContainer.Clear();

            // Sets how many preview images to create in one direction
            for (int i = 0; i < _previewSteps * 2 + 1; i++)
            {
                string borderXAML = XamlWriter.Save(PreviewImage);
                StringReader stringReader = new StringReader(borderXAML);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                Border previewImage = (Border)XamlReader.Load(xmlReader);

                previewImage.Visibility = Visibility.Visible;
                PreviewContainer.Children.Add(previewImage);
            }

            // Gets all preview image containers
            foreach (var item in PreviewContainer.Children)
            {
                _previewContainer.Add((Image)((Border)item).Child);
            }
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

            // Gif file
            if (temp.EndsWith(".gif"))
                return "gif";

            // Text file
            if (temp.EndsWith(".txt"))
                return "text";

            // Video file
            if (temp.EndsWith(".mp4") || temp.EndsWith(".mkv") || temp.EndsWith(".webm")
                 || temp.EndsWith(".wmv") || temp.EndsWith(".flv") || temp.EndsWith(".avi"))
                return "video";

            return "file";
        }

        // Changes the visibility of all UI elements not related to the current displayed item
        private void MakeTypeVisible(string fileType)
        {
            imageViewer.Visibility = gifViewer.Visibility = gifViewer.Visibility =
                textViewer.Visibility = VideoPlayIcon.Visibility = iconViewer.Visibility =
                Visibility.Hidden;
            imageViewer.Effect = null;
            gifViewer.Source = null;
            GifItem._gifImage = null;

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
                    imageViewer.Effect = _videoBlur;
                    break;
                case "text":
                    textViewer.Visibility = Visibility.Visible;
                    break;
                case "file":
                    iconViewer.Visibility = Visibility.Visible;
                    break;
            }
        }





        // Changes the currently displayed content
        public void UpdateContent()
        {
            // Show specific view when content is empty
            if (_displayItems.Count == 0)
            {
                PreviewField.Visibility = Visibility.Hidden;
                UpdateSettingsChanged();
                return;
            }

            _currentItem = _displayItems[_displayedItemIndex];

            // Makes all irrelevant elements invisible
            MakeTypeVisible(_currentItem.GetTypeOfFile());
            string currentFileType = _currentItem.GetTypeOfFile();

            try
            {
                // Preloads images ahead of time
                AddToCache();

                // Gets and show the content
                if (currentFileType == "image")
                    imageViewer.Source = ((ImageItem)_currentItem).GetImage();
                else if (currentFileType == "gif")
                    gifViewer.Source = ((GifItem)_currentItem).GetGif(gifViewer);
                else if (currentFileType == "video")
                    imageViewer.Source = ((VideoItem)_currentItem).GetThumbnail();
                else if (currentFileType == "text")
                    textViewer.Text = ((TextItem)_currentItem).GetText();
                else if (currentFileType == "file")
                    iconViewer.Source = (_currentItem).GetThumbnail();
            }
            catch
            {
                // If content can't get loaded, show a blank black screen
                MakeTypeVisible("");
            }

            _loadedOffset = 0;

            PreviewContent();

            ResetView();
            UpdateSettingsChanged();
        }

        // Preview content in front or back
        private void PreviewContent()
        {
            int firstPreview = -((_previewContainer.Count - 1) / 2);

            for (var index = 0; index < _previewContainer.Count; index++)
            {
                Image item = _previewContainer[index];

                // Don't display images out of bounds
                if (_displayedItemIndex + index + firstPreview > _displayItems.Count - 1)
                {
                    item.Source = null;
                    continue;
                }
                if (_displayedItemIndex + index + firstPreview < 0)
                {
                    item.Source = null;
                    continue;
                }

                var offsetItem = _displayItems[_displayedItemIndex + index + firstPreview];

                // Give the center image a border
                if (index + firstPreview == 0)
                {
                    Border parBorder = (Border)item.Parent;

                    parBorder.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#b01e76")); ;
                    parBorder.BorderThickness = new Thickness(2, 2, 2, 2);
                }

                // Sets the image to view
                item.Source = offsetItem.GetTypeOfFile() == "image"
                    ? ((ImageItem)offsetItem).GetImage()
                    : offsetItem.GetThumbnail();
            }
        }

        /// <summary>
        /// Zooms in or out of the current viewed image
        /// </summary>
        /// <param name="zoomAmount">The amount to zoom, where 0.5 is 50% and 2 is 200% zoomed in</param>
        public void Zoom(double zoomAmount)
        {
            // Only allow zooming in an image
            if (_currentItem.GetTypeOfFile() != "image") return;

            // Disallow zooming beyond the specified limits
            if (zoomAmount > 0 && _currentZoom + zoomAmount >= MaxZoom ||
                zoomAmount < 0 && _currentZoom + zoomAmount <= MinZoom) return;

            // Changes the current zoom level and updates the image
            _currentZoom += zoomAmount;
            _st.ScaleX = _currentZoom;
            _st.ScaleY = _currentZoom;
            imageViewer.RenderTransform = _imageTransformGroup;
        }



        // Adds all valid new files to a list
        private void ProcessNewFiles()
        {
            foreach (var item in NewFiles)
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
                    case "file":
                        if (!_allowOtherFiles ||
                            File.GetAttributes(item).HasFlag(FileAttributes.Hidden)) continue;
                        _displayItems.Add(new FileItem(item));
                        isInCache.Add(false);
                        break;
                }
            }
            NewFiles.Clear();
        }

        // Recursively finds all files and subfolders in a folder
        private void FindFilesInSubfolders(string[] folder)
        {
            foreach (var s in folder)
            {
                if (Directory.Exists(s))
                {
                    if (_isDrop) InitializeDrop(s);

                    SearchOption scanFolderStructure =
                        _showSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    // Files to add
                    foreach (string foundFile in Directory.GetFiles(s, "*.*", scanFolderStructure))
                    {
                        // Exlude folders started with an underscore
                        if (Path.GetDirectoryName(foundFile).Contains("_")) continue;
                        // Exclude special folders when set to do so
                        if (!_showSets &&
                            _specialFolders.Any(o => Path.GetDirectoryName(foundFile).Contains(o.Key))) continue;

                        // If set, exclude showing files with the set prefix
                        if (!_showPrefix && foundFile.Contains(QuickPrefix)) continue;

                        NewFiles.Add(foundFile);
                    }
                }
                else if (File.Exists(s))
                {
                    if (Path.GetDirectoryName(s).Contains("_")) continue;
                    if (_isDrop) InitializeDrop(Path.GetDirectoryName(s));

                    // Add filepath
                    NewFiles.Add(s);
                }
            }
        }

        // Resets specific settings when content is loaded from a drop as opposed to
        // loading an already defined subfolder
        private void InitializeDrop(string s)
        {
            // The folder highest in the tree
            _originFolder = new Folder(s);
            DisplayItem.RootFolder = Directory.GetParent(_originFolder.GetFolderPath()).ToString();

            _isDrop = false;
        }

        // Handler for drag-dropping files
        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            AddNewFolder(folder);
        }

        // Initializes a new folder, removing the previous state
        private void AddNewFolder(string[] folder)
        {
            try
            {
                _displayedItemIndex = 0;
                DirectoryTreeList.Items.Clear();

                _originFolder?.GetAllFolders()?.Clear();
                _originFolder?.GetAllShownFolders()?.Clear();

                RemoveOldContext();
                _isDrop = true;
                CreateNewContext(folder);
                CreateSortMenu();
            }
            catch
            {
                RemoveOldContext();
                Interaction.MsgBox("Couldn't load files");
            }
        }

        // Content-specific actions
        private void FocusContent()
        {
            // Sets focus on image and text files
            if (_currentItem.GetTypeOfFile() == "text" || _currentItem.GetTypeOfFile() == "image")
            {
                _isActive = !_isActive;
                textViewer.IsEnabled = _isActive;

                if (_isActive == false)
                {
                    ResetView();
                }
            }

            // Start video in default player
            else if (_currentItem.GetTypeOfFile() == "video" || _currentItem.GetTypeOfFile() == "file")
            {
                Process.Start(_displayItems[_displayedItemIndex].GetFilePath());
            }
        }

        /// <summary>
        /// Resets the current image's zoom level and panning to default
        /// </summary>
        public void ResetView()
        {
            _tt.Y = 0.5;
            _tt.X = 0.5;
            _st.ScaleX = 1;
            _st.ScaleY = 1;
            _currentZoom = 1;
            imageViewer.RenderTransform = _imageTransformGroup;
        }


        // Loads a new folder
        private void CreateNewContext(string[] folder)
        {
            _displayedItemIndex = 0;
            RemoveOldContext();
            FindFilesInSubfolders(folder);
            ProcessNewFiles();

            UpdateContent();

            foreach (Control item in ViewMenu.Items)
            {
                if (!(item is MenuItem)) continue;
                item.IsEnabled = true;
            }

            EditMenu.IsEnabled = true;
            OpenMenu.IsEnabled = true;

            if (Settings.Default.IsPreviewOpen)
            {
                PreviewField.Visibility = Visibility.Visible;
            }
        }

        // Removes an old folder
        private void RemoveOldContext()
        {
            _isActive = false;
            _isTyping = false;
            SortTypeBox.Visibility = Visibility.Hidden;

            imageViewer.Source = null;

            UpdateTitle();
            ResetView();

            _displayItems.Clear();
            _movedItems.Clear();
            isInCache.Clear();

            PreviewField.Visibility = Visibility.Hidden;

            if (_isEndless)
            {
                OpenEndlessView();
            }

            UpdateTitle();
            UpdateInfobar();

            foreach (Control item in ViewMenu.Items)
            {
                if (!(item is MenuItem) || item.Name == "FullscreenMenu" || item.Name == "IncludeSpecialMenu" ||
                    item.Name == "IncludeOtherFilesMenu" || item.Name == "IncludeSubMenu" ||
                    item.Name == "IncludePrefixMenu") continue;
                item.IsEnabled = false;
            }

            EditMenu.IsEnabled = false;
            OpenMenu.IsEnabled = false;

            GC.Collect();
        }
    }
}