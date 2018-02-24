using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Image_Manager
{
    partial class MainWindow
    {
        private void UpdateInfobar()
        {

            if (_displayItems.Count == 0) 
            {
                CurrentFileInfoLabel.Content = "End of directory";
                return;
            }

            if (_currentItem.GetTypeOfFile() == "image" && _currentItem.GetFileExtension() != ".webp" && _preferWebP)
                CurrentFileInfoLabel.Foreground =
                    ((ImageItem) _displayItems[_displayedItemIndex]).GetSize() < 1000 && _prefer1000Px
                    ? _notOver1000PxWarningTextColor
                    : _notWebPWarningTextColor;
            else 
                CurrentFileInfoLabel.Foreground = _defaultTextColor;

            string preInfo = "(" + (_displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";
            CurrentFileInfoLabel.Content = preInfo + _currentItem.GetInfobarContent() + "   ";
        }

        private void UpdateTitle()
        {
            if (_displayItems.Count > 0)
            {
                string curItem = _currentItem.GetFilePath();
                Title = "(" + (_displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";

                if (!_showSubDir)
                {
                    Title = Title + " -subdir ";
                }
                if (!_showSets)
                {
                    Title = Title + " -sets ";
                }
                if (!_showSets || !_showSubDir)
                {
                    Title = Title + "| ";
                }

                Title = Title + Path.GetFileName(curItem);
            }
            else
            {
                Title = "Image Manager";
                if (!_showSubDir)
                {
                    Title = Title + " -subdir";
                }
                if (!_showSets)
                {
                    Title = Title + " -sets";
                }
            }
        }

        private void CreateSortMenu()
        {
            DirectoryTreeList.Items.Clear();

            foreach (Folder foundFolder in _originFolder.GetAllShownFolders())
            {
                SolidColorBrush color = _defaultTextColor;

                // Color directories based on content
                _specialFolders.Where(c => foundFolder.GetFolderName().Contains(c.Key)).ToList()
                    .ForEach(cc => color = cc.Value);

                DirectoryTreeList.Items.Add(new ListBoxItem
                {
                    Content = foundFolder.GetFolderName(),
                    Foreground = color,
                    Margin = new Thickness(IndentDistance * foundFolder.GetFolderDepth(),0,0,0)
                });
            }
        }

        private void MoveUp()
        {
            if (_sortMode)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex - 1 < 0
                    ? _originFolder.GetAllFolders().Count - 1
                    : DirectoryTreeList.SelectedIndex - 1;
        }

        private void MoveDown()
        {
            if (_sortMode)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex + 1 == 
                    _originFolder.GetAllFolders().Count ? 
                    0 : DirectoryTreeList.SelectedIndex + 1;
        }
        
        private void ToggleViewMode()
        {
            _sortMode = !_sortMode;
            DirectoryTreeList.Visibility = _sortMode ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
