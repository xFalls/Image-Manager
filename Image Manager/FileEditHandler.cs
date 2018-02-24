using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualBasic;

namespace Image_Manager
{
    partial class MainWindow
    {

        private void RemoveFile()
        {
            MoveFile(0);
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
                    workingPath = Path.GetDirectoryName(newPath) + "\\" + currentItem.GetFileNameExcludingExtension() + 
                                  " (" + iterNum + ")" + currentItem.GetFileExtension();
                }
                else
                {
                    return workingPath;
                }
            }
            
        }

        private void MoveFile(int mode)
        {
            if (_displayItems.Count == 0) return;
            
            try
            {
                string newPath = originFolder.GetAllShownFolders()[DirectoryTreeList.SelectedIndex].GetFolderPath() + "\\" +
                    currentItem.GetFileName();

                newPath = NewNameIfTaken(currentItem.GetFilePath(), newPath);

                File.Move(currentItem.GetFilePath(), newPath);

                _movedItems.Insert(0, currentItem);
                _displayItems.Remove(currentItem);
                currentItem.SetFilePath(newPath);
            }
            catch
            {
                Interaction.MsgBox("File is currently being used by another program or has been removed");
                return;
            }
            
            // When last file has been moved
            if (_displayItems.Count == 0)
                MakeTypeVisible("");
            else if (displayedItemIndex == _displayItems.Count)
                displayedItemIndex--;

            UpdateContent();
        }
        

        private void UndoMove()
        {
            if (_movedItems.Count == 0) return;
            try { 

            DisplayItem fileToUndo = _movedItems.ElementAt(0);

            File.Move(fileToUndo.GetFilePath(), fileToUndo.GetOldFilePath());
            fileToUndo.SetFilePath(fileToUndo.GetOldFilePath());

            _displayItems.Insert(displayedItemIndex, fileToUndo);
            _movedItems.RemoveAt(0);
            }
                catch
            {
                Interaction.MsgBox("File is currently being used by another program or has been removed");
            }
            UpdateContent();
        }

        private void RenameFile(string input)
        {
            if (input == "") return;
            string currentFileExt = currentItem.GetFileExtension();
            string currentLocation = currentItem.GetLocation();

            if (!File.Exists(currentLocation + "\\" + input + currentFileExt))
            {
                try
                {
                    File.Move(currentItem.GetFilePath(), currentLocation + "\\" + input + currentFileExt);
                    currentItem.SetFilePath(currentLocation + "\\" + input + currentFileExt);
                    UpdateContent();
                }
                catch
                {
                    Interaction.MsgBox("File is currently being used by another program");
                }
            }
            else
            {
                Interaction.MsgBox("File with name already exists");
            }

        }
    }
}
