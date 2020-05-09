using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
                    {
                        CurrentFileInfoLabelLeft.Foreground = redWarning;
                        CurrentFileInfoLabelRight.Foreground = redWarning;
                    }
                    // Either
                    else if (!isWebP || !isOver)
                    {
                        CurrentFileInfoLabelLeft.Foreground = orangeWarning;
                        CurrentFileInfoLabelRight.Foreground = orangeWarning;
                    }
                    // Both
                    else
                    {
                        CurrentFileInfoLabelLeft.Foreground = _defaultTextColor;
                        CurrentFileInfoLabelRight.Foreground = _defaultTextColor;
                    }
                // Check for webP
                else if (!_prefer1000Px && _preferWebP)
                {
                    CurrentFileInfoLabelLeft.Foreground = !isWebP ? orangeWarning : _defaultTextColor;
                    CurrentFileInfoLabelRight.Foreground = !isWebP ? orangeWarning : _defaultTextColor;
                }
                // Check for 1000px
                else if (!_preferWebP && _prefer1000Px && !_rescale)
                {
                    CurrentFileInfoLabelLeft.Foreground = !isOver ? orangeWarning : _defaultTextColor;
                    CurrentFileInfoLabelRight.Foreground = !isOver ? orangeWarning : _defaultTextColor;
                }
                // Default
                else
                {
                    CurrentFileInfoLabelLeft.Foreground = _defaultTextColor;
                    CurrentFileInfoLabelRight.Foreground = _defaultTextColor;
                }
            }


            // All content is added after its indexed number
            string preInfo = "(" + (_displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";

            // Replace + with ★ and add a space
            string firstStars = _currentItem.GetInfobarContent().Remove(5);
            var extractStars = Regex.Replace(firstStars, "[^+]", "");
            CurrentFileInfoLabelLeft.Content = preInfo +
                                               extractStars.Replace("+", "★") +
                                               " " +
                                               _currentItem.GetInfobarContent().Replace("+","") +
                                               "   ";


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
                if (_rescale)
                {
                    Title = Title + " -rescale ";
                }
                if (_renameShown)
                {
                    Title = Title + " -rename ";
                }
                if (!_showSets || !_showSubDir || !_showPrefix || !_allowOtherFiles || _rescale || _renameShown)
                {
                    Title = Title + "| ";
                }

                Title = Title + Path.GetFileName(curItem);
            }
            else
            {
                // What to show if nothing is loaded
                Title = "MediFiler";
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
                if (_rescale)
                {
                    Title = Title + " -rescale";
                }
                if (_renameShown)
                {
                    Title = Title + " -rename ";
                }
            }
        }

        // Graphically draws the loaded folder structure
        private void CreateSortMenu()
        {
            DirectoryTreeList.Items.Clear();
            int counter = 0;

            if (_originFolder == null) return;

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

                string shortcutkey = "";
                if (counter < 10)
                {
                    shortcutkey = counter + ". ";
                }

                var extractStars = Regex.Replace(foundFolder.GetFolderName(), "[^+]", "");
                extractStars = extractStars.Replace("+", "★");

                int depthSize = 0;
                // How to display each item
                if (foundFolder.GetFolderDepth() > 0)
                {
                    depthSize = IndentDistance * (foundFolder.GetFolderDepth() - 1) + 5;
                }

                ListViewItem folderItem = new ListViewItem
                {

                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Cursor = Cursors.Hand,
                    Content = new Grid
                    {
                        

                Margin = Margin = new Thickness(depthSize, 0, 0, 0),
                        Children = {
                            new TextBlock
                            {
                                Text = shortcutkey + "(" + foundFolder.GetNumberOfFiles()[0] + "/" + foundFolder.GetNumberOfFiles()[7] +
                                       ") " + extractStars + " - "
                                       + Truncate(foundFolder.GetFolderName().Replace("+", ""), 40),
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

                counter++;
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

            RenameFolder(folder);
        }

        public void RenameFolder(Folder folder, string dfltName = "")
        {
            if (folder.GetFolderPath() == _originFolder.GetFolderPath())
            {
                Interaction.MsgBox("Cannot rename root folder");
                return;
            }

            string newName = dfltName;

            if (dfltName == "")
            {
                newName = Interaction.InputBox("Enter new name of the folder", "Choose new name",
                    folder.GetFolderName());
            }

            string currentLocation = Directory.GetParent(folder.GetFolderPath()).ToString();

            try
            {
                // Batch rename
                if (newName.Contains("[Set]"))
                {
                    // Count number of stars
                    int starNum = newName.Count(f => f == '+');
                    string pre = "";
                    for (int i = 0; i < starNum; i++)
                    {
                        pre = pre + "+";
                    }

                    // Remove all current stars
                    DirectoryInfo d = new DirectoryInfo(folder.GetFolderPath());
                    FileInfo[] infos = d.GetFiles();
                    foreach (FileInfo f in infos)
                    {
                        string finalName = f.Name.Replace("+", "").Insert(0, pre);
                        File.Move(f.FullName, f.DirectoryName + "\\" + finalName);
                    }

                    // Add calculated number of stars
                }

                // Rename
                Directory.Move(folder.GetFolderPath(), currentLocation + "\\" + newName);
            }
            catch
            {
                // Same name
            }

            // Check if the changed folder is the currently opened one
            if (folder.GetFolderPath() == _currentItem.GetLocation())
            {
                Console.WriteLine("CURRENT");

                lastFolder = currentLocation + "\\" + newName;
                Console.WriteLine(currentLocation + "\\" + newName);
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

                /*string name = Truncate(folder.GetFolderName(), 40);
                string length = data[0] + " (" + data[7] + ") files";
                string images = data[1] + " images";
                string videos = data[2] + " videos";
                string gifs = data[3] + " gifs";
                string webp = data[4] + " webp";
                string text = data[5] + " text";
                string other = data[6] + " other";*/

                double per1 = Math.Round((double)data[1]*100 / data[0], 0);
                double per2 = Math.Round((double)data[2]*100 / data[0], 0);
                double per3 = Math.Round((double)data[3]*100 / data[0], 0);
                double per4 = Math.Round((double)data[4]*100 / data[0], 0);
                double per5 = Math.Round((double)data[5]*100 / data[0], 0);

                if (Double.IsNaN(per1))
                {
                    per1 = 0;
                }
                if (Double.IsNaN(per2))
                {
                    per2 = 0;
                }
                if (Double.IsNaN(per3))
                {
                    per3 = 0;
                }
                if (Double.IsNaN(per4))
                {
                    per4 = 0;
                }
                if (Double.IsNaN(per5))
                {
                    per5 = 0;
                }


                string name = Truncate(folder.GetFolderName(), 40);
                string length = data[0] + " (" + data[6] + ") files";

                /*string images = data[1] + " (" + per1 + "%) +++"
                 ;
                string videos = data[2] + " (" + per2 + "%) ++";
                string gifs = data[3] + " (" + per3 + "%) +";
                string webp = data[4] + " (" + per4 + "%) =";
                string text = data[5] + " (" + per5 + "%) new";*/

                /*string images = "5🟊 " + per1 + "% (" + data[1] + ")";
                string videos = " | 4🟊 " + per2 + "% (" + data[2] + ")";
                string gifs = " | 3🟊 " + per3 + "% (" + data[3] + ")";
                string webp = " | 2🟊 " + per4 + "% (" + data[4] + ")";
                string text = " | 1🟊 " + per5 + "% (" + data[5] + ")";
                string none = " | 0🟊 " + per5 + "% (" + data[6] + ")";
                */

                string images = "5🟊 " + data[1];
                string videos = "| 4🟊 " + data[2];
                string gifs =   "| 3🟊 " + data[3];
                string webp =   "| 2🟊 " + data[4];
                string text =   "| 1🟊 " + data[5];
                string none =   "| New " + data[6];


                string size = folder.GetDirectorySize() + "";

                CurrentFileInfoLabelLeft.Content =
                    $"{name,-40}{size,-19}{length,-15}";
                CurrentFileInfoLabelRight.Content =
                    $"{images,-10}{videos,-10}{gifs,-10}{webp,-10}{text,-10}{none,-10}";
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
