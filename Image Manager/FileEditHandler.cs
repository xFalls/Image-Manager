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

        private void MoveFileViaExplore()
        {
            if (currentMode != 1) return;       
            MoveFile(1);
        }

        private void MoveFileViaSort()
        {
            if (currentMode != 2) return;
            MoveFile(2);
        }



        //////////////////////////////////////
        
        private void MoveFile(int mode)
        {
            if (establishedRoot == false || filepaths.Count == 0)
            {
                return;
            }

            ListBoxItem selectedBoxItem;
            string currentFileName = Path.GetFileName(filepaths[displayedItemIndex]); ;
            string originalPath = filepaths[displayedItemIndex]; ;
            string newFileName;
            string ext = Path.GetExtension(currentFileName); ;
            

            // Explore
            if (mode == 1)
            {
                selectedBoxItem = (ListBoxItem)DirectoryTreeList.Items[guiSelection];
                newFileName = currentFolder + "\\" + selectedBoxItem.Content + "\\" + currentFileName;
                ext = Path.GetExtension(currentFileName);
                newFileName = newFileName.Replace(rootTitleText, "");
                newFileName = newFileName.Replace(prevDirTitleText, "");
            }
            // Sort
            if (mode == 2)
            {
                selectedBoxItem = (ListBoxItem) AllFolders.Items[sortGuiSelection];

                string folderPath = folderDict[selectedBoxItem.Content.ToString()];
                newFileName = folderPath + "\\" + currentFileName;
            }
            // Delete
            else
            {
                selectedBoxItem = (ListBoxItem)AllFolders.Items[sortGuiSelection];

                newFileName = deleteFolder + "\\" + currentFileName;

            }


            bool isTopDir = selectedBoxItem.Content.ToString() == rootTitleText ||
                            selectedBoxItem.Content.ToString() == prevDirTitleText;

            // Renames file if file with same name already exists
            // Also prevents the file from being moved into the same folder
            while (true)
            {
                if (File.Exists(newFileName))
                {
                    if (isTopDir == false)
                    {
                        string pathToCompare1 = currentFolder + "\\" + selectedBoxItem.Content;
                        string pathToCompare2 = originalPath.TrimEnd('\\').Replace(currentFileName, "");

                        // If current image is in the marked folder
                        if (pathToCompare1 == pathToCompare2) break;
                        newFileName = currentFolder + "\\" + selectedBoxItem.Content + "\\" +
                                      Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
                    }
                    else
                    {
                        // If current image is in the same folder
                        if (currentFolder.TrimEnd('\\') ==
                            originalPath.Replace(currentFileName, "").TrimEnd('\\')) break;
                        newFileName = currentFolder + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
                    }
                }
                else
                {
                    break;
                }
            }


            try
            {
                movedFiles.Insert(0, newFileName);
                movedFilesOldLocations.Insert(0, filepaths[displayedItemIndex]);

                File.Move(originalPath, newFileName);
                filepaths.RemoveAt(displayedItemIndex);
            }
            catch
            {
                Interaction.MsgBox("File is currently being used by another program");
                return;
            }

            // When last file has been moved
            if (filepaths.Count == 0)
            {
                textViewer.Visibility = Visibility.Hidden;
                imageViewer.Visibility = Visibility.Hidden;
                VideoPlayIcon.Visibility = Visibility.Hidden;
                gifViewer.Visibility = Visibility.Hidden;
                UpdateInfobar();
            }
            else if (displayedItemIndex == filepaths.Count)
            {
                displayedItemIndex--;
            }

            UpdateContent();
            UpdateTitle();
        }




        private void UndoMove()
        {
            if (movedFiles.Count == 0) return;
            string fileToUndo = movedFiles.ElementAt(0);
            string locationToMoveTo = movedFilesOldLocations.ElementAt(0);
            filepaths.Insert(displayedItemIndex, locationToMoveTo);

            File.Move(fileToUndo, locationToMoveTo);

            movedFiles.RemoveAt(0);
            movedFilesOldLocations.RemoveAt(0);
            UpdateContent();
        }

        private void RenameFile(string input)
        {
            if (input == "") return;
            string currentFileName;
            string currentFileExt;
            string currentLocation;

            try
            {
                currentFileName = Path.GetFileNameWithoutExtension(filepaths[displayedItemIndex]);
                currentFileExt = Path.GetExtension(filepaths[displayedItemIndex]);
                currentLocation = Path.GetFullPath(filepaths[displayedItemIndex]).Replace(currentFileName, "")
                    .Replace(currentFileExt, "");
            }
            catch
            {
                Interaction.MsgBox("Name can't be empty");
                return;
            }

            if (!File.Exists(currentLocation + "\\" + input + currentFileExt))
            {
                try
                {
                    File.Move(currentLocation + "\\" + currentFileName + currentFileExt,
                        currentLocation + "\\" + input + currentFileExt);
                    filepaths[displayedItemIndex] = currentLocation + "\\" + input + currentFileExt;
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
