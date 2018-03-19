using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic;
using Shell32;

namespace Image_Manager
{
    partial class MainWindow
    {
        public static Shell shell = new Shell();
        public static Shell32.Folder RecyclingBin = shell.NameSpace(10);

        /// <summary>
        /// Moves the currently displayed file
        /// </summary>
        /// <param name="mode">Optionally set as "remove" to remove the file.</param>
        public void MoveFile(string mode = "")
        {
            if (_displayItems.Count == 0) return;
            if (mode != "remove")
                if (DirectoryTreeList.SelectedIndex == -1) return;

            _changed = true;
            
            try
            {
                if (mode == "remove")
                {
                    _currentItem.hasBeenDeleted = true;
                    RecyclingBin.MoveHere(_currentItem.GetFilePath());
                    
                    //newPath = _deleteFolder + "\\" + _currentItem.GetFileName();
                }
                else
                {
                    var newPath = _originFolder.GetAllShownFolders()[DirectoryTreeList.SelectedIndex].GetFolderPath() + 
                                     "\\" + _currentItem.GetFileName();

                    newPath = NewNameIfTaken(_currentItem.GetFilePath(), newPath);

                    File.Move(_currentItem.GetFilePath(), newPath);
                    _currentItem.SetFilePath(newPath);
                }

                

                // Update internal representation to reflect changes
                _movedItems.Insert(0, _currentItem);
                UndoMenu.IsEnabled = true;
                _displayItems.RemoveAt(_displayedItemIndex);
                isInCache.RemoveAt(_displayedItemIndex);
                
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
            _changed = true;

            try { 
                DisplayItem fileToUndo = _movedItems.ElementAt(0);

                if (fileToUndo.hasBeenDeleted)
                {
                    // Based on
                    // https://stackoverflow.com/questions/6025311/how-to-restore-files-from-recycle-bin?lq=1

                    string Item = fileToUndo.GetFilePath().Replace(@"\\", @"\");   // restore is sensitive to double backslashes
                    Shell Shl = new Shell();
                    Shell32.Folder Recycler = Shl.NameSpace(10);
                    foreach (FolderItem FI in Recycler.Items())
                    {
                        string FileName = Recycler.GetDetailsOf(FI, 0);
                        if (Path.GetExtension(FileName) == "") FileName += Path.GetExtension(FI.Path);
                        //Necessary for systems with hidden file extensions.
                        string FilePath = Recycler.GetDetailsOf(FI, 1);
                        if (Item == Path.Combine(FilePath, FileName))
                        {
                            File.Move(FI.Path, fileToUndo.GetFilePath());
                            fileToUndo.hasBeenDeleted = false;
                            break;
                        }
                    }

                }
                else
                {

                    File.Move(fileToUndo.GetFilePath(), fileToUndo.GetOldFilePath());
                    fileToUndo.SetFilePath(fileToUndo.GetOldFilePath());
                }

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

        private void WebPConverter()
        {
            if (_currentItem.GetTypeOfFile() != "image" ||
                _currentItem.GetFileExtension() == ".webp")
            {
                Interaction.MsgBox("Unsupported filetype");
                return;
            }

            string convertedFile = _currentItem.GetLocation() + "\\" +
                                  _currentItem.GetFileNameExcludingExtension() + "[P].webp";

            if (File.Exists(convertedFile))
            {
                Interaction.MsgBox("File already exists");
                return;
            }

            Bitmap image = new Bitmap(_currentItem.GetFilePath());
            FileStream to = File.Create(convertedFile);

            new Imazen.WebP.SimpleEncoder().Encode(image, to, 85);

            ImageItem newWebP = new ImageItem(convertedFile);
            newWebP.wasConverted = true;

            _displayItems.Insert(_displayedItemIndex + 1, newWebP);
            isInCache.Insert(_displayedItemIndex + 1, true);

            _displayedItemIndex++;

            UpdateContent();
        }

        private void UpscaleFile()
        {
            string waifu2x = AppDomain.CurrentDomain.BaseDirectory + "\\waifu2x-caffe\\";

            if (!File.Exists(waifu2x + "waifu2x-caffe-cui.exe")) return;

            string upscaledFile = _currentItem.GetLocation() + "\\" +
                                  _currentItem.GetFileNameExcludingExtension() + "[U]" +
                                  _currentItem.GetFileExtension();

            if (File.Exists(upscaledFile))
            {
                Interaction.MsgBox("File already exists");
                return;
            }

            Process upscaler = new Process();
            upscaler.StartInfo.FileName = waifu2x + "waifu2x-caffe-cui.exe";
            upscaler.StartInfo.Arguments = "-i " + "\"" + _currentItem.GetFilePath() + "\"" +
                                           " -s 2.0 -p gpu -n 2 -o " + 
                                           "\"" + upscaledFile + "\"";
            upscaler.Start();
            upscaler.WaitForExit();


            

            if (!File.Exists(upscaledFile))
            {
                upscaler = new Process();
                upscaler.StartInfo.FileName = waifu2x + "waifu2x-caffe-cui.exe";
                upscaler.StartInfo.Arguments = "-i " + "\"" + _currentItem.GetFilePath() + "\"" +
                                               " -s 2.0 -p cpu -n 2 -o " +
                                               "\"" + upscaledFile + "\"";
                upscaler.Start();
                upscaler.WaitForExit();
            }

            if (!File.Exists(upscaledFile))
            {
                Interaction.MsgBox("Unsupported filetype");
                return;
            }

            _displayItems.Insert(_displayedItemIndex + 1, new ImageItem(upscaledFile));
            isInCache.Insert(_displayedItemIndex + 1, false);

            _displayedItemIndex++;
            UpdateContent();
        }
    }
}
