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
            if (!_isTyping)
                switch (e.Key)
                {
                    // Toggle focus, enter selected directory
                    case Key.Space:

                        ToggleAction();

                        break;

                    // Enter directory
                    case Key.E:
                        //CreateSortMenu(openFolder.GetChildFolders()[openFolder.GetSelected()]);
                        break;

                    // Rename current file
                    case Key.F2:
                        string input = Interaction.InputBox("Rename", "Select a new name", _currentItem.GetFileNameExcludingExtension());
                        RenameFile(input);
                        break;

                    // Adds +HQ modifier
                    case Key.F3:
                        string hqFileName = Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                        string hqInput = QuickPrefix + hqFileName;
                        RenameFile(hqInput);
                        break;

                    // Remove +HQ modifier
                    case Key.F4:
                        string hQnoFileName = Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                        string hQnoInput = hQnoFileName?.Replace(QuickPrefix, "");
                        RenameFile(hQnoInput);
                        break;

                    // Move file to selected directory
                    case Key.Enter:
                    case Key.R:
                        if (_sortMode)
                            MoveFile();
                        break;

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
                    case Key.F:
                        if (!_isDrop && _sortMode)
                        {
                            string[] folder = new string[1];

                            folder[0] = _originFolder.GetAllShownFolders()[DirectoryTreeList.SelectedIndex].GetFolderPath();
                            CreateNewContext(folder);
                        }
                        UpdateTitle();
                        break;

                    // Toggle subdirectories in view mode
                    case Key.LeftShift:
                        _showSubDir = !_showSubDir;
                        UpdateTitle();
                        break;

                    // Toggle special folders
                    case Key.X:
                        _showSets = !_showSets;
                        UpdateTitle();
                        break;

                    // Toggle directory list
                    case Key.Tab:
                        ToggleViewMode();
                        break;

                    // Undo last move
                    case Key.Z:
                        UndoMove();
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

                        MakeMenuStripInvisible();
                        break;
                    // Make normal
                    case (WindowState.Maximized):
                        ResizeMode = ResizeMode.CanResize;
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        WindowState = WindowState.Normal;

                        MakeMenuStripVisible();
                        break;
                }
            }
            // Start typing mode
            else if (e.Key == Key.LeftCtrl)
            {
                if (_sortMode && !_isDrop)
                {
                    if (_isTyping)
                    {
                        SortTypeBox.Visibility = Visibility.Hidden;
                        _isTyping = false;
                        //RepaintSortSelector();
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
        }

        // Occurs while typing
        private void SortTypeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_isTyping) return;
            if (e.Key == Key.Left)
            {
                if (_displayedItemIndex <= 0 || _isActive && _currentItem.GetTypeOfFile() == "text") return;
                _displayedItemIndex--;
                UpdateContent();
            }
            else if (e.Key == Key.Right)
            {
                if (_displayedItemIndex + 1 >= _displayItems.Count || _isActive && _currentItem.GetTypeOfFile() == "text") return;
                _displayedItemIndex++;
                UpdateContent();
            }
            else if (e.Key == Key.Enter)
            {
                if (_sortMode)
                {
                    MoveFile();
                }
            }
            else if (e.Key == Key.Up)
            {
                


            }
            else if (e.Key == Key.Down)
            {
                


            }
            else
            {
                FilterSort();

            }
        }
        
        private void FilterSort()
        {
            _originFolder.RemoveAllShownFolders();
            if (SortTypeBox.Text != "" && _isTyping)
            {
                // Filter out all items that don't contain the input string in alphabetical order
                // E.g. RiN shows Rain but not rni

                foreach (Folder item in _originFolder.GetAllFolders())
                {
                    if (ContainsWord(SortTypeBox.Text, item.GetFolderName()))
                    {
                        _originFolder.GetAllShownFolders().Add(item);
                    }
                    else
                    {
                        //originFolder.GetAllShownFolders().Add(item);
                    }
                }

                if (_originFolder.GetAllShownFolders().Count == 0) return;
            }
            else
            {
                _originFolder.GetAllShownFolders().AddRange(_originFolder.GetAllFolders());
            }
            CreateSortMenu();
        }


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

        private void ControlWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Disable switching image when on a focused text item
            if (_isActive && _currentItem.GetTypeOfFile() == "text")
            {
                return;
            }
            if (_isActive)
            {
                double zoom = e.Delta > 0 ? .2 : -.2;
                if (zoom > 0)
                {
                    Zoom(ZoomAmountWheel);
                }
                else if (zoom < 0)
                {
                    Zoom(-ZoomAmountWheel);
                }
                return;
            }

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

        // Double click to reset view
        private void ControlWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ResetView();
            }
        }

        // A right click opens the selected directory in the gallery
        
        private void DirectoryTreeList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {/*
            if (currentMode != 1) return;
            if (ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) is ListBoxItem item)
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
            //e.Handled = true; */
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
            if (_currentItem.GetTypeOfFile() == "video") return;
            if (_isActive == false) return;
            imageViewer.CaptureMouse();
            imageViewer.RenderTransform = _imageTransformGroup;

            _start = e.GetPosition(ImageBorder);
            _origin = new Point(_tt.X, _tt.Y);
        }

        private void imageViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imageViewer.ReleaseMouseCapture();
        }

        private void imageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!imageViewer.IsMouseCaptured) return;
            Vector v = _start - e.GetPosition(ImageBorder);
            _tt.X = _origin.X - v.X;
            _tt.Y = _origin.Y - v.Y;

            imageViewer.RenderTransform = _imageTransformGroup;
        }

        private void ControlWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ToggleAction();
        }

        private void MenuStrip_MouseEnter(object sender, MouseEventArgs e)
        {
            MakeMenuStripVisible();
        }

        private void MenuStrip_MouseLeave(object sender, MouseEventArgs e)
        {
            MakeMenuStripInvisible();
        }

        private void MakeMenuStripInvisible()
        {
            if (WindowState == WindowState.Normal)
            {
                return;
            }

            foreach (MenuItem item in MenuStrip.Items)
            {
                item.Visibility = Visibility.Hidden;
            }

            MenuStrip.Background = new SolidColorBrush(Colors.Transparent);
            MenuStrip.Visibility = Visibility.Visible;

            var margin = Margin;
            margin.Top = 0;

            imageViewer.Margin = margin;
            gifViewer.Margin = margin;
            textViewer.Margin = margin;
        }

        private void MakeMenuStripVisible()
        {
            if (WindowState == WindowState.Normal)
            {
                return;
            }

            foreach (MenuItem item in MenuStrip.Items)
            {
                item.Visibility = Visibility.Visible;
            }

            var bc = new BrushConverter();
            MenuStrip.Background = (Brush)bc.ConvertFrom("#FF171717");
            MenuStrip.Visibility = Visibility.Visible;

            var margin = Margin;
            margin.Top = 18;

            ImageBorder.Margin = margin;
            imageViewer.Margin = margin;
            gifViewer.Margin = margin;
            textViewer.Margin = margin;
        }
    }
}
