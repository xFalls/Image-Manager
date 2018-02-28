using System.IO;
using System.Linq;
using Microsoft.VisualBasic;

namespace Image_Manager
{
    partial class MainWindow
    {
        /// <summary>
        /// Moves the currently displayed file
        /// </summary>
        /// <param name="mode">Optionally set as "remove" to remove the file.</param>
        public void MoveFile(string mode = "")
        {
            if (_displayItems.Count == 0) return;
            if (mode != "remove")
                if (DirectoryTreeList.SelectedIndex == -1) return;
            
            try
            {
                string newPath;
                if (mode == "remove")
                {
                    newPath = _deleteFolder + "\\" + _currentItem.GetFileName();
                }
                else
                {
                    newPath = _originFolder.GetAllShownFolders()[DirectoryTreeList.SelectedIndex].GetFolderPath() + 
                        "\\" + _currentItem.GetFileName();
                }

                newPath = NewNameIfTaken(_currentItem.GetFilePath(), newPath);

                File.Move(_currentItem.GetFilePath(), newPath);

                // Update internal representation to reflect changes
                _movedItems.Insert(0, _currentItem);
                _displayItems.RemoveAt(_displayedItemIndex);
                isInCache.RemoveAt(_displayedItemIndex);
                _currentItem.SetFilePath(newPath);
            }
            catch
            {
                Interaction.MsgBox("File is currently being used by another program or has been removed");
                return;
            }
            
            // When last file has been moved
            if (_displayItems.Count == 0)
                MakeTypeVisible("");
            else if (_displayedItemIndex == _displayItems.Count)
                _displayedItemIndex--;

            UpdateContent();
        }
        
        // Undos the previous moves and deletions
        private void UndoMove()
        {
            if (_movedItems.Count == 0) return;

            try { 
                DisplayItem fileToUndo = _movedItems.ElementAt(0);

                File.Move(fileToUndo.GetFilePath(), fileToUndo.GetOldFilePath());
                fileToUndo.SetFilePath(fileToUndo.GetOldFilePath());

                _displayItems.Insert(_displayedItemIndex, fileToUndo);
                isInCache.Insert(_displayedItemIndex, false);
                _movedItems.RemoveAt(0);
            }
                catch
            {
                Interaction.MsgBox("File is currently being used by another program or has been removed");
            }

            UpdateContent();
        }

        /// <summary>
        /// Gives the current file a new name.
        /// </summary>
        /// <param name="input">The new name.</param>
        private void RenameFile(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            string currentFileExt = _currentItem.GetFileExtension();
            string currentLocation = _currentItem.GetLocation();

            if (!File.Exists(currentLocation + "\\" + input + currentFileExt))
            {
                try
                {
                    File.Move(_currentItem.GetFilePath(), currentLocation + "\\" + input + currentFileExt);
                    _currentItem.SetFilePath(currentLocation + "\\" + input + currentFileExt);
                    UpdateContent();
                }
                catch
                {
                    Interaction.MsgBox("File is currently being used by another program or has been removed");
                }
            }
            else
            {
                if (_currentItem.GetFilePath() == currentLocation + "\\" + input + currentFileExt)
                {
                    return;
                }
                Interaction.MsgBox("File with name already exists");
            }
        }

        // Removing a file simply moves it to a preset directory.
        private void RemoveFile()
        {
            MoveFile("remove");
        }

        // Renames the file if an file with the same name already exists in the directory
        private string NewNameIfTaken(string curPath, string newPath)
        {
            string workingPath = newPath;
            int iterNum = 0;

            while (true)
            {
                iterNum++;
                if (File.Exists(workingPath))
                {
                    // If current image is in the same folder
                    if (curPath == newPath) return workingPath;

                    // Tries with the same path, but appends a (#) number at the end
                    workingPath = Path.GetDirectoryName(newPath) + "\\" + _currentItem.GetFileNameExcludingExtension() +
                                  " (" + iterNum + ")" + _currentItem.GetFileExtension();
                }
                else
                {
                    return workingPath;
                }
            }

        }
    }
}
