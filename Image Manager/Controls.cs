using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Image_Manager.Properties;
using Microsoft.VisualBasic;
using Console = System.Console;

namespace Image_Manager
{ 
    partial class MainWindow
    {

        // Various keyboard shortcuts
        private void ControlWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Not accessible whlie typing
            if (!_isTyping && _displayItems.Count != 0 && !_isEndless)
                switch (e.Key)
                {
                    // Toggle focus, enter selected directory
                    case Key.Space:
                        FocusContent();
                        break;

                    // Rename current file
                    case Key.F2:

                        // Batch rename Sets
                        // Check if loaded folder is a set
                        if (_currentItem.GetLocation().Split('\\').Last().Contains("[Set]"))
                        {
                            // Find Folder by searching for current item's location
                            int loop = 0;
                            foreach (Folder folder in _originFolder.GetAllShownFolders())
                            {
                                if (folder.GetFolderPath() == _currentItem.GetLocation())
                                {
                                    break;
                                }
                                loop++;
                            }

                            RenameFolder(_originFolder.GetAllShownFolders()[loop]);
                            break;
                        }


                        // Rename
                        string input = Interaction.InputBox("Rename", "Select a new name",
                            _currentItem.GetFileNameExcludingExtension());
                        RenameFile(input);
                        break;

                    // Adds a prefix to the current file
                    case Key.F3:

                        // Batch rename Sets
                        // Check if loaded folder is a set
                        if (_currentItem.GetLocation().Split('\\').Last().Contains("[Set]"))
                        {
                            // Find Folder by searching for current item's location
                            int loop = 0;
                            foreach (Folder folder in _originFolder.GetAllShownFolders())
                            {
                                if (folder.GetFolderPath() == _currentItem.GetLocation())
                                {
                                    break;
                                }

                                loop++;
                            }

                            RenameFolder(_originFolder.GetAllShownFolders()[loop], 
                                "+" + _currentItem.GetLocation().Split('\\').Last());
                            break;
                        }

                        string hqFileName =
                            Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                        if (_currentItem.GetFileName().StartsWith("="))
                        {
                            hqFileName = hqFileName?.Replace("=", "");
                        }
                        string hqInput = QuickPrefix + hqFileName;
                        RenameFile(hqInput);
                        break;

                    // Remove the prefix
                    case Key.F4:
                        // Batch rename Sets
                        // Check if loaded folder is a set
                        if (_currentItem.GetLocation().Split('\\').Last().Contains("+[Set]"))
                        {
                            // Find Folder by searching for current item's location
                            int loop = 0;
                            foreach (Folder folder in _originFolder.GetAllShownFolders())
                            {
                                if (folder.GetFolderPath() == _currentItem.GetLocation())
                                {
                                    break;
                                }

                                loop++;
                            }

                            RenameFolder(_originFolder.GetAllShownFolders()[loop],
                                _currentItem.GetLocation().Split('\\').Last().Replace("+[Set]","[Set]"));
                            break;
                        }


                        string hQnoFileName =
                            Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                        string hQnoInput = hQnoFileName?.Replace(QuickPrefix, "");
                        string hQnoInput2 = hQnoFileName?.Replace(QuickPrefix+hQnoInput, hQnoInput);
                        RenameFile(hQnoInput2);
                        break;


                    // Add Action
                    case Key.A:
                       string hqFileName2 =
                            Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                        if (_currentItem.GetFileName().Contains("[A]")) {
                            break;
                        }

                        string hqInput2 = "[A]" + hqFileName2;
                        RenameFile(hqInput2);
                        break;


                    // Move file to selected directory
                    case Key.Enter:
                    case Key.R:
                        if (Settings.Default.SortMode)
                            MoveFile();
                        break;

                    // Removes the current file
                    case Key.Delete:
                        RemoveFile();
                        break;

                    // Zoom in
                    case Key.Add:
                        Zoom(ZoomAmountButton);
                        InfiScroll.Width += 50; 
                        break;

                    // Upscales the currently shown file
                    case Key.U:
                        if (Settings.Default.Experimental)
                            UpscaleFile(true);
                        break;

                    // Upscales the currently shown file
                    case Key.N:
                        if (Settings.Default.Experimental)
                            UpscaleFile(false);
                        break;

                    // Converts image to WebP
                    case Key.P:
                        if (Settings.Default.Experimental)
                            WebPConverter();
                        break;

                    // Zoom out
                    case Key.Subtract:
                        Zoom(-ZoomAmountButton);
                        InfiScroll.Width -= 50;
                        break;

                    // Open directory in view mode
                    case Key.E:
                        if (!_isDrop && Settings.Default.SortMode)
                        {
                            string[] folder =
                                {_originFolder.GetAllShownFolders()[DirectoryTreeList.SelectedIndex].GetFolderPath()};
                            CreateNewContext(folder);
                        }
                        break;

                    // Toggle directory list
                    case Key.Tab:
                        ToggleViewMode();
                        break;

                    // Select directory below
                    case Key.Down:
                        MoveDown();
                        break;

                    // Select directory above
                    case Key.Up:
                        MoveUp();
                        break;

                    // Previous image
                    case Key.Left:
                        if (_displayedItemIndex > 0 && !(_isActive && _currentItem.GetTypeOfFile() == "text"))
                        {
                            _displayedItemIndex--;
                            UpdateContent();
                        }
                        break;

                    // Next image
                    case Key.Right:
                        if (_displayedItemIndex + 1 < _displayItems.Count && !(_isActive && _currentItem.GetTypeOfFile() == "text"))
                        {
                            _displayedItemIndex++;
                            UpdateContent();
                        }
                        break;

                    // First image
                    case Key.Home:
                        if (!(_isActive && _currentItem.GetTypeOfFile() == "text"))
                        {
                            _displayedItemIndex = 0;
                            UpdateContent();
                        }
                        break;

                    // Last image
                    case Key.End:
                        if (!(_isActive && _currentItem.GetTypeOfFile() == "text"))
                        {
                            _displayedItemIndex = _displayItems.Count - 1;
                            UpdateContent();
                        }
                        break;

                    // Toggles showing thumbnail preview bar
                    case Key.F:
                        PreviewField.Visibility = (PreviewField.Visibility == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
                        Settings.Default.IsPreviewOpen = PreviewField.Visibility == Visibility.Visible;
                        ShowSortPreview.IsChecked = PreviewField.Visibility == Visibility.Visible;
                        break;

                    // Converts all image files to WebP and removes the original (ESC + F9)
                    case Key.F9:
                        if (Keyboard.IsKeyDown(Key.Escape))
                            ConvertAllToWebP();
                        break;

                    case Key.NumPad0:
                        DirectoryTreeList.SelectedIndex = 0;
                        MoveFile();
                        break;

                    case Key.NumPad1:
                        DirectoryTreeList.SelectedIndex = 1;
                        MoveFile();
                        break;

                    case Key.NumPad2:
                        DirectoryTreeList.SelectedIndex = 2;
                        MoveFile();
                        break;

                    case Key.NumPad3:
                        DirectoryTreeList.SelectedIndex = 3;
                        MoveFile();
                        break;

                    case Key.NumPad4:
                        DirectoryTreeList.SelectedIndex = 4;
                        MoveFile();
                        break;

                    case Key.NumPad5:
                        DirectoryTreeList.SelectedIndex = 5;
                        MoveFile();
                        break;

                    case Key.NumPad6:
                        DirectoryTreeList.SelectedIndex = 6;
                        MoveFile();
                        break;

                    case Key.NumPad7:
                        DirectoryTreeList.SelectedIndex = 7;
                        MoveFile();
                        break;

                    case Key.NumPad8:
                        DirectoryTreeList.SelectedIndex = 8;
                        MoveFile();
                        break;

                    case Key.NumPad9:
                        DirectoryTreeList.SelectedIndex = 9;
                        MoveFile();
                        break;
                }

            // Toggle fullscreen
            if (e.Key == Key.F11)
            {
                MakeFullscreen();

            }
            // Toggles prefixing viewed files
            else if (e.Key == Key.F7)
            {
                _renameShown = !_renameShown;
                Settings.Default.Rename = _renameShown;
                UpdateTitle();
            }

            // Opens README file
            else if (e.Key == Key.F1)
            {
                Process.Start("README.md");
            }
            // Opens settings window
            else if (e.Key == Key.F12)
            {
                new SettingsWindow(this).Show();
            }
            // Refresh
            else if (e.Key == Key.F5)
            {
                RefreshAll();
            }

            else if (_isEndless && e.Key == Key.Add)
            {
                if (InfiScroll.Width < InfiMaxZoom) {
                    InfiScroll.Width += InfiZoomAmount;
                }
            }
            else if (_isEndless && e.Key == Key.Subtract)
            {
                if (InfiScroll.Width > InfiMinZoom)
                {
                    InfiScroll.Width -= InfiZoomAmount;
                }
            }

            // Toggle subdirectories in view mode
            if (!_isTyping)
            {
                switch (e.Key)
                {
                    case Key.LeftShift:
                    case Key.RightShift:
                        IncludeSubMenu.IsChecked = _showSubDir;
                        _showSubDir = !_showSubDir;
                        UpdateTitle();
                        break;
                    case Key.X:
                        IncludeSpecialMenu.IsChecked = _showSets;
                        _showSets = !_showSets;
                        UpdateTitle();
                        CreateSortMenu();
                        break;
                    case Key.C:
                        IncludePrefixMenu.IsChecked = _showPrefix;
                        _showPrefix = !_showPrefix;
                        UpdateTitle();
                        break;
                    case Key.V:
                        IncludeOtherFilesMenu.IsChecked = _allowOtherFiles;
                        _allowOtherFiles = !_allowOtherFiles;
                        UpdateTitle();
                        break;
                    case Key.T:
                        //IncludeOtherFilesMenu.IsChecked = _allowOtherFiles;
                        _rescale = !_rescale;
                        //UpdateTitle();
                        UpdateContent();
                        break;
                    case Key.Z:
                        UndoMove();
                        break;
                    case Key.M:
                        OpenEndlessView();
                        break;
                }
            }

            // Start typing mode
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ToggleSortField();
            }

        }

        // Makes window go fullscreen
        private void MakeFullscreen()
        {
            switch (WindowStyle)
            {
                // Make fullscreen
                case (WindowStyle.SingleBorderWindow):
                    //ResizeMode = ResizeMode.NoResize;
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;

                    FullscreenMenu.IsChecked = true;
                    ToggleShowingMenuStrip();
                    break;
                // Make normal
                case (WindowStyle.None):
                    //ResizeMode = ResizeMode.CanResize;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;

                    FullscreenMenu.IsChecked = false;
                    ToggleShowingMenuStrip();
                    break;
            }
        }

        // Occurs while typing
        private void SortTypeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_isTyping || _displayItems.Count == 0) return;

            // Keys that work while in typing mode
            switch (e.Key)
            {
                // Previous image
                case Key.Left:
                    if (_displayedItemIndex <= 0 || _isActive && _currentItem.GetTypeOfFile() == "text") return;
                    _displayedItemIndex--;
                    UpdateContent();
                    break;

                // Next image
                case Key.Right:
                    if (_displayedItemIndex + 1 >= _displayItems.Count || _isActive && _currentItem.GetTypeOfFile() == "text") return;
                    _displayedItemIndex++;
                    UpdateContent();
                    break;

                // Adds current item to the selected folder
                case Key.Enter:
                    if (FolderGrid.Opacity == 0)
                        MoveFile();
                    break;

                // Selects directory above
                case Key.Up:
                    MoveUp();
                    break;

                // Selects directory below
                case Key.Down:
                    MoveDown();
                    break;

                // Send all other key inputs to the textbox
                default:
                    FilterSort();
                    break;
            }
        }

        // Filters the list of all shown folders according to what's typed.
        private void FilterSort()
        {
            List<Folder> tempList = new List<Folder>();
            tempList.AddRange(_originFolder.GetAllShownFolders());
           _originFolder.RemoveAllShownFolders();

            if (SortTypeBox.Text != "" && _isTyping)
            {
                // Filter out all items that don't contain the input string in alphabetical order
                // E.g. RiN shows Rain but not rni
                foreach (Folder item in _originFolder.GetAllFolders())
                {
                    // Matches all items that exist in the local path 
                    // (working up from the root folder)
                    if (ContainsWord(SortTypeBox.Text.Replace(" ", ""), item.GetLocalPath()))
                    {
                        _originFolder.GetAllShownFolders().Add(item);
                    }
                }

                // If nothing matches, keep previous shown state
                if (_originFolder.GetAllShownFolders().Count == 0)
                {
                    _originFolder.GetAllShownFolders().AddRange(tempList);
                };
            }
            else
            {
                _originFolder.GetAllShownFolders().AddRange(_originFolder.GetAllFolders());
            }
            // Recreates the sort menu based on the typed criteria
            CreateSortMenu();
        }

        /// <summary>
        /// Checks if a word contains parts of another.
        /// </summary>
        /// <param name="word">The word to compare with.</param>
        /// <param name="otherword">The word to compare to.</param>
        /// <returns>Returns whether the match is a success.</returns>
        public static bool ContainsWord(string word, string otherword)
        {
            word = word.ToLower();
            otherword = otherword?.ToLower();

            int lastPos = -1;
            foreach (char c in word)
            {
                lastPos++;
                while (lastPos < otherword.Length && otherword[lastPos] != c)
                    lastPos++;
                if (lastPos == otherword.Length)
                    return false;
            }
            return true;
        }

        // Sets what scrolling the mousewheel should do
        private void ControlWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Disable switching image when on a focused text item
            if (_isActive && _currentItem.GetTypeOfFile() == "text")
                return;

            // Allows for zooming on a focused image
            if (_isActive)
            {
                if (e.Delta > 0)
                {
                    Zoom(ZoomAmountWheel);
                }
                else if (e.Delta < 0)
                {
                    Zoom(-ZoomAmountWheel);
                }
                return;
            }

            // Go to next/previous image
            if (e.Delta > 0 && _displayedItemIndex > 0)
            {
                _displayedItemIndex--;
                UpdateContent();
            }
            else if (e.Delta < 0 && _displayedItemIndex + 1 < _displayItems.Count)
            {
                _displayedItemIndex++;
                UpdateContent();
            }
        }

        // Right clicking a folder moves the item to that folder
        private void DirectoryTreeList_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
            {
                DirectoryTreeList.SelectedItem = item;
                MoveFile();
            }
        }

        // A left click opens the selected directory in the gallery    
        private void DirectoryTreeList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(sender as ListView, e.OriginalSource as DependencyObject) is
                ListViewItem item)
            {
                DirectoryTreeList.SelectedItem = item;
                string[] folder =
                    {_originFolder.GetAllShownFolders()[DirectoryTreeList.SelectedIndex].GetFolderPath()};
                CreateNewContext(folder);
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
            // Only works with images
            if (_currentItem.GetTypeOfFile() != "image" || _isActive == false) return;
            imageViewer.CaptureMouse();
            imageViewer.RenderTransform = _imageTransformGroup;

            _start = e.GetPosition(ImageBorder);
            _origin = new Point(_tt.X, _tt.Y);
        }

        // Stops the dragging action
        private void imageViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imageViewer.ReleaseMouseCapture();
        }

        // Drags the image with the mouse movement
        private void imageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!imageViewer.IsMouseCaptured) return;
            Vector v = _start - e.GetPosition(ImageBorder);
            _tt.X = _origin.X - v.X;
            _tt.Y = _origin.Y - v.Y;

            imageViewer.RenderTransform = _imageTransformGroup;
        }

        // Triggers an action on the displayed content
        private void ControlWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_displayItems.Count != 0)
                FocusContent();
        }

        // Show the menu strip on hover in fullscreen
        private void MenuStrip_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleShowingMenuStrip(true);
        }

        // Hide the menu strip on leaving it in fullscreen
        private void MenuStrip_MouseLeave(object sender, MouseEventArgs e)
        {
            ToggleShowingMenuStrip();
        }

        // Hides/shows the menu strip and moves elements around to accomodate the new layout
        private void ToggleShowingMenuStrip(bool mouseOver = false)
        {
            var margin = Margin;
            margin.Left = 0;
            margin.Right = 0;
            Visibility vis;

            if (WindowStyle == WindowStyle.SingleBorderWindow || mouseOver)
            {
                margin.Top = 18;
                ImageBorder.Margin = margin;
                vis = Visibility.Visible;
                //var bc = new BrushConverter();
                MenuStrip.Background = currentBrush;
            }
            else
            {
                margin.Top = 0;
                vis = Visibility.Hidden;
                MenuStrip.Background = new SolidColorBrush(Colors.Transparent);
            }

            foreach (MenuItem item in MenuStrip.Items)
                item.Visibility = vis;

            imageViewer.Margin = margin;
            gifViewer.Margin = margin;
            textViewer.Margin = margin;

        }

        // Allows jumping between items by clicking their preview images
        private void PreviewField_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(e.Source is FrameworkElement mouseWasDownOn)) return;

            int index = 0 - _previewSteps;
            foreach (Image item in _previewContainer)
            {
                if (item == mouseWasDownOn)
                {
                    _displayedItemIndex += index;
                    UpdateContent();
                    break;
                }
                index++;
            }
        }
    }
}
