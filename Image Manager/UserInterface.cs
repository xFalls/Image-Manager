using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Image_Manager
{
    partial class MainWindow
    {
        // Sets the contents of the infobar according to each 
        // displayed item's preferred representation
        private void UpdateInfobar()
        {
            CurrentFileInfoLabel.Foreground = _defaultTextColor;

            if (_displayItems.Count == 0)
            {
                CurrentFileInfoLabel.Content = "End of directory";
                return;
            }

            if (_currentItem.GetTypeOfFile() != "text")
                if (!File.Exists(_currentItem.GetFilePath()) && imageViewer.Source == null)
                {
                    CurrentFileInfoLabel.Content = "Could not find content" + "   ";
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
                        CurrentFileInfoLabel.Foreground = redWarning;
                    // Either
                    else if (!isWebP || !isOver)
                        CurrentFileInfoLabel.Foreground = orangeWarning;
                    // Both
                    else
                        CurrentFileInfoLabel.Foreground = _defaultTextColor;
                // Check for webP
                else if (!_prefer1000Px && _preferWebP)
                    CurrentFileInfoLabel.Foreground = !isWebP ? orangeWarning : _defaultTextColor;
                // Check for 1000px
                else if (!_preferWebP && _prefer1000Px)
                    CurrentFileInfoLabel.Foreground = !isOver ? orangeWarning : _defaultTextColor;
                // Default
                else
                    CurrentFileInfoLabel.Foreground = _defaultTextColor;
            }


            // All content is added after its indexed number
            string preInfo = "(" + (_displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";
            CurrentFileInfoLabel.Content = preInfo + _currentItem.GetInfobarContent() + "   ";
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

                // How to display each item
                ListViewItem folderItem = new ListViewItem
                {
                    Content = new TextBlock
                    { Text = "(" + foundFolder.GetNumberOfFiles()[0] + "/" + foundFolder.GetNumberOfFiles()[7] +
                        ") - " + Truncate(foundFolder.GetFolderName(), 40) },
                    //{ Text = $"{foundFolder.GetFolderName(),-50}{foundFolder.GetNumberOfFiles()[0],20}" },
                    Foreground = color,
                    Margin = new Thickness(IndentDistance * foundFolder.GetFolderDepth(), 0, 0, 0)
                };

                folderItem.MouseEnter += FolderEntry_MouseEnter;
                folderItem.MouseLeave += FolderEntry_MouseLeave;

                DirectoryTreeList.Items.Add(folderItem);
            }
        }


        private void FolderEntry_MouseEnter(object sender, MouseEventArgs e)
        {
            // Gets the folder
            Folder folder =
                _originFolder.GetAllShownFolders()[DirectoryTreeList.Items.IndexOf(sender)];

            CurrentFileInfoLabel.Foreground = ((ListViewItem)sender).Foreground;

            string name = Truncate(folder.GetFolderName(), 40);
            string length = folder.GetNumberOfFiles()[0] + " (" + folder.GetNumberOfFiles()[7] + ") files";
            string images = folder.GetNumberOfFiles()[1] + " images";
            string videos = folder.GetNumberOfFiles()[2] + " videos";
            string gifs = folder.GetNumberOfFiles()[3] +   " gifs";
            string webp = folder.GetNumberOfFiles()[4] +   " webp";
            string text = folder.GetNumberOfFiles()[5] +   " text";
            string other = folder.GetNumberOfFiles()[6] +  " other";

            string size = folder.GetDirectorySize() + "";

            CurrentFileInfoLabel.Content = 
                $"{name, -40}{size, 19}{length, 15}  |  {images, -11}{webp,-9}{gifs,-9}{videos, -11}{text, -9}{other, -10}";
        }


        public static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "..";
        }

        // Revert infobar to previous text
        private void FolderEntry_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateInfobar();
        }

        // Moves the folder selection up
        private void MoveUp()
        {
            if (_sortMode)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex - 1 < 0
                    ? _originFolder.GetAllFolders().Count - 1
                    : DirectoryTreeList.SelectedIndex - 1;
        }

        // Moves the folder selection down
        private void MoveDown()
        {
            if (_sortMode)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex + 1 ==
                    _originFolder.GetAllFolders().Count ?
                    0 : DirectoryTreeList.SelectedIndex + 1;
        }

        // Toggles the sort GUI
        private void ToggleViewMode()
        {
            _sortMode = !_sortMode;
            DirectoryTreeList.Visibility = _sortMode ? Visibility.Visible : Visibility.Hidden;
            ShowSortMenuMenu.IsChecked = _sortMode;
        }
    }
}
