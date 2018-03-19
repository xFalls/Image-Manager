using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image_Manager.Properties;
using Microsoft.VisualBasic;

namespace Image_Manager
{
    partial class MainWindow
    {
        // Sets the contents of the infobar according to each 
        // displayed item's preferred representation
        private void UpdateInfobar()
        {
            CurrentFileInfoLabelLeft.Foreground = _defaultTextColor;

            if (_displayItems.Count == 0)
            {
                CurrentFileInfoLabelLeft.Content = "End of directory";
                CurrentFileInfoLabelRight.Content = "";
                return;
            }

            if (_currentItem.GetTypeOfFile() != "text")
                if (!File.Exists(_currentItem.GetFilePath()) && imageViewer.Source == null)
                {
                    CurrentFileInfoLabelLeft.Content = "Could not find content" + "   ";
                    CurrentFileInfoLabelRight.Content = "";
                    return;
                }


            // Colors the text according to preset preferences
            if (_currentItem.GetTypeOfFile() == "image")
            {
                bool isWebP = _currentItem.GetFileExtension() == ".webp";
                bool isOver = ((ImageItem)_displayItems[_displayedItemIndex]).GetSize() >= 1000;

                // Check if both
                if (_prefer1000Px && _preferWebP)
                    // Neither
                    if (!isWebP && !isOver)
                        CurrentFileInfoLabelLeft.Foreground = redWarning;
                    // Either
                    else if (!isWebP || !isOver)
                        CurrentFileInfoLabelLeft.Foreground = orangeWarning;
                    // Both
                    else
                        CurrentFileInfoLabelLeft.Foreground = _defaultTextColor;
                // Check for webP
                else if (!_prefer1000Px && _preferWebP)
                    CurrentFileInfoLabelLeft.Foreground = !isWebP ? orangeWarning : _defaultTextColor;
                // Check for 1000px
                else if (!_preferWebP && _prefer1000Px)
                    CurrentFileInfoLabelLeft.Foreground = !isOver ? orangeWarning : _defaultTextColor;
                // Default
                else
                    CurrentFileInfoLabelLeft.Foreground = _defaultTextColor;
            }


            // All content is added after its indexed number
            string preInfo = "(" + (_displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";
            CurrentFileInfoLabelLeft.Content = preInfo + _currentItem.GetInfobarContent() + "   ";
            CurrentFileInfoLabelRight.Content = _currentItem.GetInfobarContentExtra();
        }


        // Updates the title of the window
        public void UpdateTitle()
        {
            if (_displayItems.Count > 0)
            {
                string curItem = _currentItem.GetFilePath();
                Title = "(" + (_displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";

                // Shows what settings are active
                if (!_showSubDir)
                {
                    Title = Title + " -subdir ";
                }
                if (!_showSets)
                {
                    Title = Title + " -sets ";
                }
                if (!_showPrefix)
                {
                    Title = Title + " -prefix ";
                }
                if (!_allowOtherFiles)
                {
                    Title = Title + " -other ";
                }
                if (!_showSets || !_showSubDir || !_showPrefix || !_allowOtherFiles)
                {
                    Title = Title + "| ";
                }

                Title = Title + Path.GetFileName(curItem);
            }
            else
            {
                // What to show if nothing is loaded
                Title = "Image Manager";
                if (!_showSubDir)
                {
                    Title = Title + " -subdir";
                }
                if (!_showSets)
                {
                    Title = Title + " -sets";
                }
                if (!_showPrefix)
                {
                    Title = Title + " -prefix";
                }
                if (!_allowOtherFiles)
                {
                    Title = Title + " -other";
                }
            }
        }

        // Graphically draws the loaded folder structure
        private void CreateSortMenu()
        {
            DirectoryTreeList.Items.Clear();

            foreach (Folder foundFolder in _originFolder.GetAllShownFolders())
            {
                SolidColorBrush color = _defaultTextColor;

                // Color directories based on content
                _specialFolders.Where(c => foundFolder.GetFolderName().Contains(c.Key)).ToList()
                    .ForEach(cc => color = cc.Value);


                MenuItem newFolderButton = new MenuItem
                {
                    Foreground = new SolidColorBrush(Colors.Black),
                    Header = "New subfolder"
                };

                MenuItem renameFolderButton = new MenuItem
                {
                    Foreground = new SolidColorBrush(Colors.Black),
                    Header = "Rename folder"
                };

                MenuItem deleteFolderButton = new MenuItem
                {
                    Foreground = new SolidColorBrush(Colors.Black),
                    Header = "Delete folder",
                };


                MenuItem folderButton = new MenuItem
                {
                    Header = "",
                    Background = new SolidColorBrush(Colors.Transparent),
                    Height = 20,
                    Width = 20,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Visibility = Visibility.Hidden,
                    //Margin = new Thickness(0,0,80,0),
                    Icon = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/MenuIcon.png"))
                    },
                };

                folderButton.Items.Add(newFolderButton);
                folderButton.Items.Add(renameFolderButton);
                folderButton.Items.Add(deleteFolderButton);

                // How to display each item
                ListViewItem folderItem = new ListViewItem
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Cursor = Cursors.Hand,
                    Content = new Grid
                    { 
                        Margin = Margin = new Thickness(IndentDistance * foundFolder.GetFolderDepth(), 0, 0, 0),
                        Children = {
                            new TextBlock
                            {
                                Text = "(" + foundFolder.GetNumberOfFiles()[0] + "/" + foundFolder.GetNumberOfFiles()[7] +
                                       ") - " + Truncate(foundFolder.GetFolderName(), 40),
                                Foreground = color
                            },
                            folderButton
                        }
                    }
                };
                folderItem.MouseEnter += FolderEntry_MouseEnter;
                folderItem.MouseLeave += FolderEntry_MouseLeave;

                folderButton.PreviewMouseLeftButtonUp += ToggleMenu;
                newFolderButton.PreviewMouseLeftButtonUp += ClickOnFolderButton;
                renameFolderButton.PreviewMouseLeftButtonUp += ClickOnRenameButton;
                deleteFolderButton.PreviewMouseLeftButtonUp += ClickOnDeleteButton;

                DirectoryTreeList.Items.Add(folderItem);
            }
        }

        // Close folder menus when clicking anywhere
        // Stupid workaround? Absolutely
        private void ControlWindow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HideMenu();
        }

        public void HideMenu()
        {
            foreach (ListViewItem item in DirectoryTreeList.Items)
            {
                ((MenuItem) ((Grid) item.Content).Children[1]).IsSubmenuOpen = false;
            }
        }

        // Opens context menu for folders
        private void ToggleMenu(object sender, MouseButtonEventArgs e)
        {
            MenuItem item = (MenuItem)sender;

            // Moves the button to better align the appearing submenu
            // Please never do this
            item.Margin = new Thickness(0, 0, 80, 0);
            item.Visibility = Visibility.Hidden;
            item.IsSubmenuOpen = true;
            item.Margin = new Thickness(0, 0, 0, 0);
        }

        private void ClickOnFolderButton(object sender, MouseButtonEventArgs e)
        {
            HideMenu();

            DependencyObject parent = LogicalTreeHelper.GetParent((DependencyObject)sender);
            DependencyObject gParent = LogicalTreeHelper.GetParent(parent);
            DependencyObject ggParent = LogicalTreeHelper.GetParent(gParent);
            int index = DirectoryTreeList.Items.IndexOf(ggParent);
            Folder folder = _originFolder.GetAllShownFolders()[index];

            string newName = Interaction.InputBox("Enter name of the new folder", "Choose name", "Folder");

            Directory.CreateDirectory(folder.GetFolderPath() + "\\" + newName);
            RefreshAll();
        }

        private void ClickOnRenameButton(object sender, MouseButtonEventArgs e)
        {
            HideMenu();

            DependencyObject parent = LogicalTreeHelper.GetParent((DependencyObject)sender);
            DependencyObject gParent = LogicalTreeHelper.GetParent(parent);
            DependencyObject ggParent = LogicalTreeHelper.GetParent(gParent);
            int index = DirectoryTreeList.Items.IndexOf(ggParent);
            Folder folder = _originFolder.GetAllShownFolders()[index];

            if (folder.GetFolderPath() == _originFolder.GetFolderPath())
            {
                Interaction.MsgBox("Cannot rename root folder");
                return;
            }

            string newName = Interaction.InputBox("Enter new name of the folder", "Choose new name", folder.GetFolderName());

            string currentLocation = Directory.GetParent(folder.GetFolderPath()).ToString();

            try
            {
                Directory.Move(folder.GetFolderPath(), currentLocation + "\\" + newName);
            }
            catch
            {
                // Same name
            }

            RefreshAll();
        }

        private void ClickOnDeleteButton(object sender, MouseButtonEventArgs e)
        {
            HideMenu();

            DependencyObject parent = LogicalTreeHelper.GetParent((DependencyObject)sender);
            DependencyObject gParent = LogicalTreeHelper.GetParent(parent);
            DependencyObject ggParent = LogicalTreeHelper.GetParent(gParent);
            int index = DirectoryTreeList.Items.IndexOf(ggParent);
            Folder folder = _originFolder.GetAllShownFolders()[index];
            
            try
            {
                Directory.Delete(folder.GetFolderPath(), false);
            }
            catch
            {
                Interaction.MsgBox("Please empty folder first");
                    
            }

            RefreshAll();
        }



        private void FolderEntry_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                ListViewItem lvi = (ListViewItem) sender;
                ((Grid) lvi.Content).Children[1].Visibility = Visibility.Visible;

                // Gets the folder
                Folder folder =
                    _originFolder.GetAllShownFolders()[DirectoryTreeList.Items.IndexOf(sender)];
                List<int> data = folder.GetNumberOfFiles();

                CurrentFileInfoLabelLeft.Foreground = ((ListViewItem)(sender)).Foreground;

                string name = Truncate(folder.GetFolderName(), 40);
                string length = data[0] + " (" + data[7] + ") files";
                string images = data[1] + " images";
                string videos = data[2] + " videos";
                string gifs = data[3] + " gifs";
                string webp = data[4] + " webp";
                string text = data[5] + " text";
                string other = data[6] + " other";

                string size = folder.GetDirectorySize() + "";

                CurrentFileInfoLabelLeft.Content =
                    $"{name,-40}{size,-19}{length,-15}";
                CurrentFileInfoLabelRight.Content =
                    $"{images,-11}{webp,-9}{gifs,-9}{videos,-11}{text,-9}{other,-10}";
            }
            catch
            {
                RefreshAll();
            }
        }


        public static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 2) + ".. ";
        }

        // Revert infobar to previous text
        private void FolderEntry_MouseLeave(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = (ListViewItem)sender;
            ((Grid) lvi.Content).Children[1].Visibility = Visibility.Hidden;

            UpdateInfobar();
        }

        // Moves the folder selection up
        private void MoveUp()
        {
            if (FolderGrid.Opacity != 0)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex - 1 < 0
                    ? _originFolder.GetAllFolders().Count - 1
                    : DirectoryTreeList.SelectedIndex - 1;
        }

        // Moves the folder selection down
        private void MoveDown()
        {
            if (FolderGrid.Opacity != 0)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex + 1 ==
                    _originFolder.GetAllFolders().Count ?
                    0 : DirectoryTreeList.SelectedIndex + 1;
        }

        // Toggles the sort GUI
        private void ToggleViewMode()
        {
            Settings.Default.SortMode = !Settings.Default.SortMode;
            ShowSortMenuMenu.IsChecked = Settings.Default.SortMode;

            FolderGrid.Opacity = Settings.Default.SortMode ? 1 : 0;
        }

        private void ToggleSortField()
        {
            if (FolderGrid.Opacity == 0)
            {
                ToggleViewMode();
            }
            if (_isDrop || _displayItems.Count == 0) return;
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
}
