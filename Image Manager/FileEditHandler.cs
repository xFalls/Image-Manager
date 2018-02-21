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
            if (establishedRoot == false || filepaths.Count == 0)
            {
                return;
            }

            ListBoxItem selectedBoxItem = (ListBoxItem)AllFolders.Items[sortGuiSelection];
            string currentFileName = Path.GetFileName(filepaths[currentImageNum]);
            string originalPath = filepaths[currentImageNum];
            string folderPath = folderDict[selectedBoxItem.Content.ToString()];

            string newFileName = deleteFolder + "\\" + currentFileName;
            string ext = Path.GetExtension(currentFileName);



            // Renames file if file with same name already exists
            // Also prevents the file from being moved into the same folder
            while (true)
            {
                if (File.Exists(newFileName))
                {
                    // If current image is in the marked folder
                    if (originalPath == newFileName) break;
                    newFileName = folderPath + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
                }
                else
                {
                    break;
                }
            }



            try
            {
                movedFiles.Insert(0, newFileName);
                movedFilesOldLocations.Insert(0, filepaths[currentImageNum]);
                File.Move(originalPath, newFileName);
            }
            catch
            {
                Interaction.MsgBox("File is currently being used by another program");
                return;
            }


            filepaths.RemoveAt(currentImageNum);


            // When last file has been moved
            if (filepaths.Count == 0)
            {
                textViewer.Visibility = Visibility.Hidden;
                imageViewer.Visibility = Visibility.Hidden;
                VideoPlayIcon.Visibility = Visibility.Hidden;
                gifViewer.Visibility = Visibility.Hidden;
                UpdateInfobar();
                /*string[] refreshFolder = new string[1];
                refreshFolder[0] = rootFolder;
                RemoveOldContext();
                currentFolder = rootFolder;
                CreateNewContext(refreshFolder);*/
            }
            else if (currentImageNum == filepaths.Count)
            {
                currentImageNum--;
            }

            UpdateContent();
            UpdateTitle();

            cache.Remove(originalPath);
        }

        private void MoveFile()
        {
            if (currentMode != 1) return;

            if (establishedRoot == false || filepaths.Count == 0)
            {
                return;
            }

            ListBoxItem selectedBoxItem = (ListBoxItem)DirectoryTreeList.Items[guiSelection];
            string currentFileName = Path.GetFileName(filepaths[currentImageNum]);
            string originalPath = filepaths[currentImageNum];
            string newFileName = currentFolder + "\\" + selectedBoxItem.Content + "\\" + currentFileName;
            string ext = Path.GetExtension(currentFileName);
            newFileName = newFileName.Replace(rootTitleText, "");
            newFileName = newFileName.Replace(prevDirTitleText, "");

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
                        string pathToCompare1 = NormalizePath(currentFolder + "\\" + selectedBoxItem.Content);
                        string pathToCompare2 = NormalizePath(originalPath.TrimEnd('\\').Replace(currentFileName, ""));

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
                movedFilesOldLocations.Insert(0, filepaths[currentImageNum]);

                File.Move(originalPath, newFileName);
                filepaths.RemoveAt(currentImageNum);
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
                /*string[] refreshFolder = new string[1];
                refreshFolder[0] = rootFolder;
                RemoveOldContext();
                currentFolder = rootFolder;
                CreateNewContext(refreshFolder);*/
            }
            else if (currentImageNum == filepaths.Count)
            {
                currentImageNum--;
            }

            UpdateContent();
            UpdateTitle();

            cache.Remove(originalPath);
        }

        private void MoveFileViaSort()
        {
            if (currentMode != 2) return;

            if (establishedRoot == false || filepaths.Count == 0)
            {
                return;
            }

            ListBoxItem selectedBoxItem = (ListBoxItem)AllFolders.Items[sortGuiSelection];
            string currentFileName = Path.GetFileName(filepaths[currentImageNum]);
            string originalPath = filepaths[currentImageNum];
            string folderPath = folderDict[selectedBoxItem.Content.ToString()];

            string newFileName = folderPath + "\\" + currentFileName;
            string ext = Path.GetExtension(currentFileName);



            // Renames file if file with same name already exists
            // Also prevents the file from being moved into the same folder
            while (true)
            {
                if (File.Exists(newFileName))
                {
                    // If current image is in the marked folder
                    if (originalPath == newFileName) break;
                    newFileName = folderPath + "\\" + Path.GetFileNameWithoutExtension(newFileName) + "-" + ext;
                }
                else
                {
                    break;
                }
            }


            try
            {
                movedFiles.Insert(0, newFileName);
                movedFilesOldLocations.Insert(0, filepaths[currentImageNum]);

                File.Move(originalPath, newFileName);
                filepaths.RemoveAt(currentImageNum);
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
                /*string[] refreshFolder = new string[1];
                refreshFolder[0] = rootFolder;
                RemoveOldContext();
                currentFolder = rootFolder;
                CreateNewContext(refreshFolder);*/
            }
            else if (currentImageNum == filepaths.Count)
            {
                currentImageNum--;
            }

            UpdateContent();
            UpdateTitle();

            cache.Remove(originalPath);
        }

        private void UndoMove()
        {
            if (movedFiles.Count == 0) return;
            string fileToUndo = movedFiles.ElementAt(0);
            string locationToMoveTo = movedFilesOldLocations.ElementAt(0);
            filepaths.Insert(currentImageNum, locationToMoveTo);

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
                currentFileName = Path.GetFileNameWithoutExtension(filepaths[currentImageNum]);
                currentFileExt = Path.GetExtension(filepaths[currentImageNum]);
                currentLocation = Path.GetFullPath(filepaths[currentImageNum]).Replace(currentFileName, "")
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
                    filepaths[currentImageNum] = currentLocation + "\\" + input + currentFileExt;
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
