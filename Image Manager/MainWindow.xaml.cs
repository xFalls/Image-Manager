using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using DirectShowLib;
using DirectShowLib.DES;
using System.Runtime.InteropServices;
using WpfAnimatedGif;

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
        SolidColorBrush warningTextColor = new SolidColorBrush(Colors.Red);

        SolidColorBrush setTextColor = new SolidColorBrush(Colors.Orange);
        SolidColorBrush artistTextColor = new SolidColorBrush(Colors.Yellow);
        SolidColorBrush mangaTextColor = new SolidColorBrush(Colors.MediumPurple);
        SolidColorBrush selectionColor = new SolidColorBrush(Colors.Blue);

        // Variables
        public static List<string> filepaths = new List<string>();
        public static List<string> newFiles = new List<string>();
        public static List<string> folderPaths = new List<string>();
        private Dictionary<string, string> folderDict = new Dictionary<string, string>();

        public static Dictionary<string, BitmapImage> cache = new Dictionary<string, BitmapImage>();

        private static int currentImageNum = 0;
        private string currentContentType = "null";

        private bool setFocus = false;
        private bool allowSubDir = true;

        private bool establishedRoot = false;
        private string rootFolder;
        private string currentFolder;

        public static string curFileName = "";
        private string curFolderPath = "";

        // Image manipulation
        private System.Windows.Point start;
        private System.Windows.Point origin;
        double initialZoom = 1;
        TransformGroup imageTransformGroup = new TransformGroup();
        TranslateTransform tt = new TranslateTransform();
        ScaleTransform st = new ScaleTransform();

        BlurEffect videoBlur = new BlurEffect();

        private int guiSelection = 0;
        private int sortGuiSelection = 0;
        private int currentMode = 0;
        CacheHandler cacheHandler = new CacheHandler();



        public MainWindow()
        {
            InitializeComponent();

            imageTransformGroup.Children.Add(st);
            imageTransformGroup.Children.Add(tt);
            imageViewer.RenderTransform = imageTransformGroup;
            videoBlur.Radius = 20;

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
                if (foundFolder.Contains('_')) continue;

                string shortDir = Path.GetFileName(foundFolder);
                SolidColorBrush color = new SolidColorBrush(Colors.White);

                // Color directories based on content
                if (shortDir.Contains("[Artist]"))
                {
                    color = artistTextColor;
                }
                else if (shortDir.Contains("[Set]"))
                {
                    color = setTextColor;
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

            RepaintSelector();
            DirectoryTreeList.Items.Refresh();
        }

        private void CompleteFolderTree(string folder)
        {
            AllFolders.Items.Clear();

            sortGuiSelection = 0;

            foreach (KeyValuePair<string, string> storedFolder in folderDict)
            {
                string dirName = storedFolder.Key;
                SolidColorBrush color = new SolidColorBrush(Colors.White);

                // Color directories based on content
                if (dirName.Contains("[Set]"))
                {
                    color = setTextColor;
                }
                else if (dirName.Contains("[Artist]"))
                {
                    color = artistTextColor;
                }
                else if (dirName.Contains("[Manga]"))
                {
                    color = mangaTextColor;
                }

                AllFolders.Items.Add(new ListBoxItem
                {
                    Content = dirName,
                    Foreground = color
                });
            }

            RepaintSortSelector();
            AllFolders.Items.Refresh();
        }

        private void RepaintSelector()
        {
            foreach (ListBoxItem item in DirectoryTreeList.Items)
            {
                item.Background = new SolidColorBrush(Colors.Transparent);
            }

            ListBoxItem selectedBox = (ListBoxItem) DirectoryTreeList.Items[guiSelection];
            selectedBox.Background = selectionColor;
        }

        private void RepaintSortSelector()
        {
            foreach (ListBoxItem item in AllFolders.Items)
            {
                item.Background = new SolidColorBrush(Colors.Transparent);
            }
            ListBoxItem selectedBox = (ListBoxItem) AllFolders.Items[sortGuiSelection];
            selectedBox.Background = selectionColor;
        }




        public static int ReturnCurrentImageNum()
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

        private void UpdateInfobar()
        {
            if (currentContentType == "image")
            {
                if (curFileName.ToLower().EndsWith(".gif"))
                {
                    CurrentFileInfoLabel.Foreground = defaultTextColor;
                    CurrentFileInfoLabel.Content = curFileName + "    -    " + Path.GetFileName(rootFolder) + curFolderPath.Replace(rootFolder, "").Replace(curFileName, "").TrimEnd('\\') + "   ";
                } else
                {
                    CurrentFileInfoLabel.Foreground = imageViewer.Source.Height >= 1000 ? defaultTextColor : warningTextColor;
                    CurrentFileInfoLabel.Content = curFileName + "    -    " + Path.GetFileName(rootFolder) + curFolderPath.Replace(rootFolder, "").Replace(curFileName, "").TrimEnd('\\') +
                        "    -    ( " + (int)imageViewer.Source.Width + " x " + (int)imageViewer.Source.Height + " )   ";
                }
            }
            else if (currentContentType == "text")
            {
                StreamReader sr = new StreamReader(filepaths[currentImageNum]);

                int counter = 0;
                string delim = " ,.!?";
                string[] fields = null;
                string line = null;

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    line.Trim();
                    fields = line.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    counter += fields.Length;
                }

                CurrentFileInfoLabel.Foreground = defaultTextColor;
                CurrentFileInfoLabel.Content = curFileName + "    -    " + Path.GetFileName(rootFolder) + curFolderPath.Replace(rootFolder, "").Replace(curFileName, "").TrimEnd('\\') +
                    "    -    " + counter + " words   ";

            } else if (currentContentType == "video")
            {
                
                var mediaDet = (IMediaDet)new MediaDet();
                DsError.ThrowExceptionForHR(mediaDet.put_Filename(filepaths[currentImageNum]));

                /* find the video stream in the file
                int index;
                var type = Guid.Empty;
                for (index = 0; index < 1000 && type != MediaType.Video; index++)
                {
                    mediaDet.put_CurrentStream(index);
                    mediaDet.get_StreamType(out type);
                }*/

                // retrieve some measurements from the video

                double frameRate;
                mediaDet.get_FrameRate(out frameRate);

                
                var mediaType = new AMMediaType();
                mediaDet.get_StreamMediaType(mediaType);
                var videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
                DsUtils.FreeAMMediaType(mediaType);
                var width = videoInfo.BmiHeader.Width;
                var height = videoInfo.BmiHeader.Height;

                double mediaLength;
                mediaDet.get_StreamLength(out mediaLength);
                var frameCount = (int)(frameRate * mediaLength);
                var duration = frameCount / frameRate;

                // Convert time into readable format
                var parts = new List<string>();
                Action<int, string> add = (val, unit) => { if (val > 0) parts.Add(val + unit); };
                var t = TimeSpan.FromSeconds((int) mediaLength);

                add(t.Days, "d");
                add(t.Hours, "h");
                add(t.Minutes, "m");
                add(t.Seconds, "s");

                string formattedTime = string.Join(" ", parts);

                string textInfo = curFileName + "    -    " + Path.GetFileName(rootFolder) + 
                    curFolderPath.Replace(rootFolder, "").Replace(curFileName, "").TrimEnd('\\') +
                    "    -    ( " + width + " x " + height + " )" +
                    "    -    ( " + formattedTime + " )   ";

                CurrentFileInfoLabel.Foreground = defaultTextColor;
                CurrentFileInfoLabel.Content = textInfo;


                mediaDet.put_Filename(null);

            }
        }

        // Changes the currently displayed content
        private void UpdateContent()
        {
            CompleteFolderTree(rootFolder);

            cacheHandler.UpdateCache();
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
                imageViewer.Source = cache[curItem];

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
                textViewer.Text = "\n\n" + File.ReadAllText(curItem);

                imageViewer.Visibility = Visibility.Hidden;
                textViewer.Visibility = Visibility.Visible;
                VideoPlayIcon.Visibility = Visibility.Hidden;
                gifViewer.Visibility = Visibility.Hidden;
            }

            UpdateInfobar();
        }

        private void ZoomIn(double zoomAmount)
        {
            if (currentContentType == "video") return;
            if (initialZoom - zoomAmount >= 3) return;
            initialZoom += zoomAmount;

            st.ScaleX = initialZoom;
            st.ScaleY = initialZoom;

            imageViewer.RenderTransform = imageTransformGroup;
        }

        private void ZoomOut(double zoomAmount)
        {
            if (currentContentType == "video") return;
            if (initialZoom - zoomAmount < 0.5) return;
            initialZoom -= zoomAmount;

            st.ScaleX = initialZoom;
            st.ScaleY = initialZoom;

            imageViewer.RenderTransform = imageTransformGroup;
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
                            if (!filepaths.Contains(foundFile))
                            {
                                if (!Path.GetDirectoryName(foundFile).Contains("_"))
                                {
                                    newFiles.Add(foundFile);
                                }
                            }
                        }
                        // Folders
                        foreach (string foundFolder in Directory.GetDirectories(s, "*", SearchOption.AllDirectories))
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
                    else
                    {
                        foreach (string foundFile in Directory.GetFiles(s, "*.*", SearchOption.TopDirectoryOnly))
                        {
                            if (!filepaths.Contains(foundFile))
                            {
                                if (!Path.GetDirectoryName(foundFile).Contains("_"))
                                {
                                    newFiles.Add(foundFile);
                                }
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
                            folderDict.Add(Path.GetFullPath(foundFolder).TrimEnd('\\').Replace(rootFolder, "").TrimStart('\\'), foundFolder);

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
            initialZoom = 1;
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
        }

        // Resets the program and starts over with new files
        private void CreateNewContext(string[] folder)
        {
            RemoveOldContext();

            FindFilesInSubfolders(folder);

            InitializeValidFiles();

            UpdateContent();

            MakeArchiveTree(currentFolder);
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

            System.GC.Collect();
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        private void MoveFile()
        {
            if (currentMode != 1) return;

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
                        string pathToCompare1 = NormalizePath(currentFolder + "\\" + selectedBoxItem.Content.ToString());
                        string pathToCompare2 = NormalizePath(originalPath.TrimEnd('\\').Replace(currentFileName, "").ToString());

                        // If current image is in the marked folder
                        if (pathToCompare1 == pathToCompare2) break;
                        newFileName = currentFolder + "\\" + selectedBoxItem.Content.ToString() + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
                    }
                    else
                    {
                        // If current image is in the same folder
                        if (currentFolder.ToString().TrimEnd('\\') ==
                            originalPath.Replace(currentFileName, "").ToString().TrimEnd('\\')) break;
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

        private void MoveFileViaSort()
        {
            if (currentMode != 2) return;

            if (establishedRoot == false || filepaths.Count == 0)
            {
                return;
            }

            ListBoxItem selectedBoxItem = (ListBoxItem)AllFolders.Items[sortGuiSelection];
            string currentFileName = Path.GetFileName(filepaths[currentImageNum]);
            string originalPath = filepaths[currentImageNum];
            string folderPath = folderDict[selectedBoxItem.Content.ToString()];

            string newFileName = folderPath + "\\" + currentFileName;
            string ext = Path.GetExtension(currentFileName);

            

            // Renames file if file with same name already exists
            // Also prevents the file from being moved into the same folder
            while (true)
            {
                if (File.Exists(newFileName))
                {
                    // If current image is in the marked folder
                    if (originalPath == newFileName) break;
                    newFileName = folderPath + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
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
            }
            else if (currentImageNum == filepaths.Count)
            {
                currentImageNum--;
            }

            UpdateContent();
            UpdateTitle();

            cache.Remove(originalPath);
        }

        private void RenameFile(string input)
        {
            string currentFileName = Path.GetFileNameWithoutExtension(filepaths[currentImageNum]);
            string currentFileExt = Path.GetExtension(filepaths[currentImageNum]);
            string currentLocation = Path.GetFullPath(filepaths[currentImageNum]).Replace(currentFileName, "").Replace(currentFileExt, "");

            if (!File.Exists(currentLocation + "\\" + input + currentFileExt))
            {
                File.Move(currentLocation + "\\" + currentFileName + currentFileExt, currentLocation + "\\" + input + currentFileExt);
                filepaths[currentImageNum] = currentLocation + "\\" + input + currentFileExt;
                UpdateContent();
            } else
            {
                Microsoft.VisualBasic.Interaction.MsgBox("File with name already exists");
            }
        }

        private void ToggleViewMode()
        {
            // View mode
            if (currentMode == 0)
            {
                DirectoryTreeList.Visibility = Visibility.Visible;
                AllFolders.Visibility = Visibility.Hidden;
                currentMode = 1;
            }

            // Explore mode
            else if (currentMode == 1)
            {
                DirectoryTreeList.Visibility = Visibility.Hidden;
                AllFolders.Visibility = Visibility.Visible;
                currentMode = 2;
            }

            // Sort mode
            else if (currentMode == 2)
            {
                DirectoryTreeList.Visibility = Visibility.Hidden;
                AllFolders.Visibility = Visibility.Hidden;
                currentMode = 0;
            }
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();

                bitmapimage.BeginInit();
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.StreamSource = memory;
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

                // Enter directory
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

                // Rename current file
                case Key.F2:
                    string currentFileName = Path.GetFileNameWithoutExtension(filepaths[currentImageNum]);
                    string input = Microsoft.VisualBasic.Interaction.InputBox("Rename", "Select a new name", currentFileName, -1, -1);
                    RenameFile(input);
                    break;

                // Adds +HQ modifier
                case Key.F3:
                    string HQFileName = Path.GetFileNameWithoutExtension(filepaths[currentImageNum]);
                    string HQInput = "+HQ " + HQFileName;
                    RenameFile(HQInput);
                    break;

                // Remove +HQ modifier
                case Key.F4:
                    string HQnoFileName = Path.GetFileNameWithoutExtension(filepaths[currentImageNum]);
                    string HQnoInput = HQnoFileName.Replace("+HQ ", "");
                    RenameFile(HQnoInput);
                    break;

                // Move file to selected directory
                case Key.R:
                    if (currentMode == 1)
                    {
                        MoveFile();
                    }
                    else if (currentMode == 2)
                    {
                        MoveFileViaSort();
                    }
                    break;

                // Zoom in
                case Key.Add:
                    ZoomIn(0.2);
                    break;

                // Zoom out
                case Key.Subtract:
                    ZoomOut(0.2);
                    break;

                // Go up one directory
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

                // Open directory in view mode
                case Key.F:
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

                // Toggle subdirectories in view mode
                case Key.LeftShift:
                    allowSubDir = !allowSubDir;
                    UpdateTitle();
                    break;

                // Toggle directory list
                case Key.Tab:
                    ToggleViewMode();
                    break;

                // Select directory below
                case Key.S:
                    if (DirectoryTreeList.Visibility == Visibility.Visible)
                    {
                        guiSelection++;

                        if (guiSelection == DirectoryTreeList.Items.Count)
                        {
                            guiSelection = 0;
                        }

                        RepaintSelector();
                    }
                    else if (currentMode == 2)
                    {
                        sortGuiSelection++;

                        if (sortGuiSelection == AllFolders.Items.Count)
                        {
                            sortGuiSelection = 0;
                        }

                        RepaintSortSelector();
                    }
                    break;

                // Select directory above
                case Key.W:
                    if (DirectoryTreeList.Visibility == Visibility.Visible)
                    {
                        guiSelection--;

                        if (guiSelection < 0)
                        {
                            guiSelection = DirectoryTreeList.Items.Count - 1;
                        }

                        RepaintSelector();
                    }
                    else if (currentMode == 2)
                    {
                        sortGuiSelection--;

                        if (sortGuiSelection < 0)
                        {
                            sortGuiSelection = AllFolders.Items.Count - 1;
                        }

                        RepaintSortSelector();
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

                // First image
                case Key.Home:
                    if (!(setFocus && currentContentType == "text"))
                    {
                        currentImageNum = 0;
                        cache.Clear();
                        System.GC.Collect();
                        UpdateContent();
                    }
                    break;

                // Last image
                case Key.End:
                    if (!(setFocus && currentContentType == "text"))
                    {
                        currentImageNum = filepaths.Count - 1;
                        cache.Clear();
                        System.GC.Collect();
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
            if (setFocus)
            {
                double zoom = e.Delta > 0 ? .2 : -.2;
                if (zoom > 0)
                {
                    ZoomIn(0.1);
                }
                else if (zoom < 0)
                {
                    ZoomOut(0.1);
                }
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
                ResetView();
            }
        }

        // A right click opens the selected directory in the gallery
        private void DirectoryTreeList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentMode != 1) return;
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
            if (currentMode != 1) return;
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

        // Toggles the directory box with a mouse wheel click
        private void ControlWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                ToggleViewMode();
            }
        }

        
        // Drag support
        private void imageViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentContentType == "video") return;
            if (setFocus == false) return;
            imageViewer.CaptureMouse();
            imageViewer.RenderTransform = imageTransformGroup;

            start = e.GetPosition(ImageBorder);
            origin = new System.Windows.Point(tt.X, tt.Y);
        }

        private void imageViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imageViewer.ReleaseMouseCapture();
        }

        private void imageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (imageViewer.IsMouseCaptured)
            {
                Vector v = start - e.GetPosition(ImageBorder);
                tt.X = origin.X - v.X;
                tt.Y = origin.Y - v.Y;

                imageViewer.RenderTransform = imageTransformGroup;
            }
        }

        private void ControlWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleFocus();
        }
    }
}