using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            if (_currentItem.GetTypeOfFile() == "image" && _currentItem.GetFileExtension() != ".webp" && _preferWebP)
                CurrentFileInfoLabel.Foreground =
                    ((ImageItem) _displayItems[_displayedItemIndex]).GetSize() < 1000 && _prefer1000Px
                    ? _notOver1000PxWarningTextColor
                    : _notWebPWarningTextColor;

            // All content is contains its indexed number
            string preInfo = "(" + (_displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";
            CurrentFileInfoLabel.Content = preInfo + _currentItem.GetInfobarContent() + "   ";
        }

        // Updates the title of the window
        private void UpdateTitle()
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
                DirectoryTreeList.Items.Add(new ListBoxItem
                {
                    Content = foundFolder.GetFolderName(),
                    Foreground = color,
                    Margin = new Thickness(IndentDistance * foundFolder.GetFolderDepth(),0,0,0)
                });
            }
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
        }
    }
}
