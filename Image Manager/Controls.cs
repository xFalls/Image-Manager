using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualBasic;

namespace Image_Manager
{
    partial class MainWindow
    {

        // Various keyboard shortcuts
        private void ControlWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Not accessible whlie typing
            if (!_isTyping && _displayItems.Count != 0)
                switch (e.Key)
                {
                    // Toggle focus, enter selected directory
                    case Key.Space:
                        FocusContent();
                        break;

                    // Rename current file
                    case Key.F2:
                        string input = Interaction.InputBox("Rename", "Select a new name",
                            _currentItem.GetFileNameExcludingExtension());
                        RenameFile(input);
                        break;

                    // Adds a prefix to the current file
                    case Key.F3:
                        string hqFileName =
                            Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                        string hqInput = QuickPrefix + hqFileName;
                        RenameFile(hqInput);
                        break;

                    // Remove the prefix
                    case Key.F4:
                        string hQnoFileName =
                            Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                        string hQnoInput = hQnoFileName?.Replace(QuickPrefix, "");
                        RenameFile(hQnoInput);
                        break;

                    // Move file to selected directory
                    case Key.Enter:
                    case Key.R:
                        if (_sortMode)
                            MoveFile();
                        break;

                    // Removes the current file
                    case Key.Delete:
                        RemoveFile();
                        break;

                    // Zoom in
                    case Key.Add:
                        Zoom(ZoomAmountButton);
                        break;

                    // Zoom out
                    case Key.Subtract:
                        Zoom(-ZoomAmountButton);
                        break;

                    // Open directory in view mode
                    case Key.E:
                        if (!_isDrop && _sortMode)
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
                    case Key.S:
                            MoveDown();
                        break;

                    // Select directory above
                    case Key.Up:
                    case Key.W:
                            MoveUp();
                        break;

                    // Previous image
                    case Key.Left:
                    case Key.A:
                        if (_displayedItemIndex > 0 && !(_isActive && _currentItem.GetTypeOfFile() == "text"))
                        {
                            _displayedItemIndex--;
                            UpdateContent();
                        }
                        break;

                    // Next image
                    case Key.Right:
                    case Key.D:
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
                }

            // Toggle fullscreen
            if (e.Key == Key.F11)
            {
                switch (WindowState)
                {
                    // Make fullscreen
                    case (WindowState.Normal):
                        ResizeMode = ResizeMode.NoResize;
                        WindowStyle = WindowStyle.None;
                        WindowState = WindowState.Maximized;

                        ToggleShowingMenuStrip();
                        break;
                    // Make normal
                    case (WindowState.Maximized):
                        ResizeMode = ResizeMode.CanResize;
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowState = WindowState.Normal;

                        ToggleShowingMenuStrip();
                        break;
                }
            }

            // Toggle subdirectories in view mode
            else if (e.Key == Key.LeftShift)
            {
                IncludeSubMenu.IsChecked = _showSubDir;
                _showSubDir = !_showSubDir;
                UpdateTitle();
            }

            // Toggle special folders
            else if (e.Key == Key.X)
            {
                IncludeSpecialMenu.IsChecked = _showSets;
                _showSets = !_showSets;
                UpdateTitle();
            }

            // Toggle prefixed files
            else if (e.Key == Key.C)
            {
                IncludePrefixMenu.IsChecked = _showPrefix;
                _showPrefix = !_showPrefix;
                UpdateTitle();
            }

            // Toggle prefixed files
            else if (e.Key == Key.V)
            {
                IncludeOtherFilesMenu.IsChecked = _allowOtherFiles;
                _allowOtherFiles = !_allowOtherFiles;
                UpdateTitle();
            }

            // Undo last move
            else if (e.Key == Key.Z)
                UndoMove();

            // Start typing mode
            else if (e.Key == Key.LeftCtrl)
            {
                if (!_sortMode || _isDrop || _displayItems.Count == 0) return;
                if (_isTyping)
                {
                    SortTypeBox.Visibility = Visibility.Hidden;
                    _isTyping = false;
                }
                else
                {
                    SortTypeBox.Text = "";
                    SortTypeBox.Visibility = Visibility.Visible;
                    SortTypeBox.Focus();
                    _isTyping = true;
                }
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
                    if (_sortMode)
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

                if (_originFolder.GetAllShownFolders().Count == 0) return;
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
            otherword = otherword.ToLower();

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
        private void DirectoryTreeList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
            {
                DirectoryTreeList.SelectedItem = item;
                MoveFile();

            }
            
        }

        // A left click opens the selected directory in the gallery        
        private void DirectoryTreeList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
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
            var vis = Visibility;

            if (WindowState == WindowState.Normal || mouseOver)
            {
                margin.Top = 18;
                ImageBorder.Margin = margin;
                vis = Visibility.Visible;
                var bc = new BrushConverter();
                MenuStrip.Background = (Brush)bc.ConvertFrom("#FF171717");
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
    }
}
