using System;
using System.Collections.Generic;
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
            if (!isTyping)
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
                        string input = Interaction.InputBox("Rename", "Select a new name", currentItem.GetFileNameExcludingExtension());
                        RenameFile(input);
                        break;

                    // Adds +HQ modifier
                    case Key.F3:
                        string hqFileName = Path.GetFileNameWithoutExtension(currentItem.GetFileNameExcludingExtension());
                        string hqInput = quickPrefix + hqFileName;
                        RenameFile(hqInput);
                        break;

                    // Remove +HQ modifier
                    case Key.F4:
                        string hQnoFileName = Path.GetFileNameWithoutExtension(currentItem.GetFileNameExcludingExtension());
                        string hQnoInput = hQnoFileName?.Replace(quickPrefix, "");
                        RenameFile(hQnoInput);
                        break;

                    // Move file to selected directory
                    case Key.Enter:
                    case Key.R:
                        if (sortMode)
                            MoveFile(0);
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
                        if (!isDrop && sortMode)
                        {
                            string[] folder = new string[1];

                            folder[0] = originFolder.GetAllShownFolders()[DirectoryTreeList.SelectedIndex].GetFolderPath();
                            CreateNewContext(folder);
                        }
                        UpdateTitle();
                        break;

                    // Toggle subdirectories in view mode
                    case Key.LeftShift:
                        showSubDir = !showSubDir;
                        UpdateTitle();
                        break;

                    // Toggle special folders
                    case Key.X:
                        showSets = !showSets;
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
                        if (displayedItemIndex > 0 && !(isActive && currentContentType == "text"))
                        {
                            displayedItemIndex--;
                            UpdateContent();
                        }
                        break;

                    // Next image
                    case Key.Right:
                    case Key.D:
                        if (displayedItemIndex + 1 < _displayItems.Count && !(isActive && currentContentType == "text"))
                        {
                            displayedItemIndex++;
                            UpdateContent();
                        }
                        break;

                    // First image
                    case Key.Home:
                        if (!(isActive && currentContentType == "text"))
                        {
                            displayedItemIndex = 0;
                            UpdateContent();
                        }
                        break;

                    // Last image
                    case Key.End:
                        if (!(isActive && currentContentType == "text"))
                        {
                            displayedItemIndex = _displayItems.Count - 1;
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
                if (sortMode && !isDrop)
                {
                    if (isTyping)
                    {
                        SortTypeBox.Visibility = Visibility.Hidden;
                        isTyping = false;
                        //RepaintSortSelector();
                    }
                    else
                    {
                        SortTypeBox.Text = "";
                        SortTypeBox.Visibility = Visibility.Visible;
                        SortTypeBox.Focus();
                        isTyping = true;
                    }
                }
            }
        }

        // Occurs while typing
        private void SortTypeBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isTyping) return;
            if (e.Key == Key.Left)
            {
                if (displayedItemIndex <= 0 || isActive && currentContentType == "text") return;
                displayedItemIndex--;
                UpdateContent();
            }
            else if (e.Key == Key.Right)
            {
                if (displayedItemIndex + 1 >= _displayItems.Count || isActive && currentContentType == "text") return;
                displayedItemIndex++;
                UpdateContent();
            }
            else if (e.Key == Key.Enter)
            {
                if (sortMode)
                {
                    MoveFile(0);
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
            originFolder.RemoveAllShownFolders();
            if (SortTypeBox.Text != "" && isTyping)
            {
                // Filter out all items that don't contain the input string in alphabetical order
                // E.g. RiN shows Rain but not rni

                foreach (Folder item in originFolder.GetAllFolders())
                {
                    if (ContainsWord(SortTypeBox.Text, item.GetFolderName()))
                    {
                        originFolder.GetAllShownFolders().Add(item);
                    }
                    else
                    {
                        //originFolder.GetAllShownFolders().Add(item);
                    }
                }

                if (originFolder.GetAllShownFolders().Count == 0) return;
            }
            else
            {
                originFolder.GetAllShownFolders().AddRange(originFolder.GetAllFolders());
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
            if (isActive && currentContentType == "text")
            {
                return;
            }
            if (isActive)
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

            if (e.Delta > 0 && displayedItemIndex > 0)
            {
                displayedItemIndex--;
                UpdateContent();
            }
            else if (e.Delta < 0 && displayedItemIndex + 1 < _displayItems.Count)
            {
                displayedItemIndex++;
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

        // Explores the selected gallery
        private void DirectoryTreeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentMode != 1) return;
            if (DirectoryTreeList.SelectedIndex != 0)
            {
                //currentFolder = currentFolder + "\\" + DirectoryTreeList.SelectedItem;
                //MakeArchiveTree(currentFolder);
            }
            else
            {
                ListBoxItem lb = (ListBoxItem)DirectoryTreeList.SelectedItem;
                if ((string)lb.Content != rootTitleText)
                {
                    //currentFolder = Path.GetFullPath(Path.Combine(currentFolder, "..\\"));
                    //MakeArchiveTree(currentFolder);
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
            if (isActive == false) return;
            imageViewer.CaptureMouse();
            imageViewer.RenderTransform = imageTransformGroup;

            start = e.GetPosition(ImageBorder);
            origin = new Point(tt.X, tt.Y);
        }

        private void imageViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            imageViewer.ReleaseMouseCapture();
        }

        private void imageViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (!imageViewer.IsMouseCaptured) return;
            Vector v = start - e.GetPosition(ImageBorder);
            tt.X = origin.X - v.X;
            tt.Y = origin.Y - v.Y;

            imageViewer.RenderTransform = imageTransformGroup;
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
