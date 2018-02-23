using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DirectShowLib;
using DirectShowLib.DES;

namespace Image_Manager
{
    partial class MainWindow
    {
        private void UpdateInfobar()
        {
            if (_displayItems.Count == 0 || establishedRoot == false || imageViewer.Source == null)
            {
                CurrentFileInfoLabel.Content = "End of directory";
                return;
            }

            switch (currentContentType)
            {
                case "image":
                    if (curFileName.ToLower().EndsWith(".gif"))
                    {
                        CurrentFileInfoLabel.Foreground = defaultTextColor;
                        CurrentFileInfoLabel.Content = "(" + (displayedItemIndex + 1) + "/" + _displayItems.Count + ") " +
                                                       curFileName + "    -    " + Path.GetFileName(originFolder.GetFolderPath()).Replace(curFileName, "").TrimEnd('\\') + "   ";
                    }
                    else
                    {
                        if (((BitmapImage)imageViewer.Source).PixelHeight < 1000)
                        {
                            CurrentFileInfoLabel.Foreground = warningTextColor;
                        }
                        else if (currentItem.GetFileExtension() != ".webp")
                        {
                            CurrentFileInfoLabel.Foreground = warningTextColor;
                        }
                        else
                        {
                            CurrentFileInfoLabel.Foreground = defaultTextColor;
                        }

                        CurrentFileInfoLabel.Content = "(" + (displayedItemIndex + 1) + "/" + _displayItems.Count + ") " +
                                                       curFileName + "    -    " + Path.GetFileName(originFolder.GetFolderPath()).Replace(curFileName, "").TrimEnd('\\') +
                                                       "    -    ( " + ((BitmapImage)imageViewer.Source).PixelWidth + " x " + ((BitmapImage)imageViewer.Source).PixelHeight + " )   ";
                    }

                    break;
                case "text":
                    StreamReader sr = new StreamReader(currentItem.GetFilePath());

                    int counter = 0;
                    const string delim = " ,.!?";

                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        line?.Trim();
                        string[] fields = line.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        counter += fields.Length;
                    }

                    sr.Close();

                    CurrentFileInfoLabel.Foreground = defaultTextColor;
                    CurrentFileInfoLabel.Content = "(" + (displayedItemIndex + 1) + "/" + _displayItems.Count + ") " +
                                                   curFileName + "    -    " + Path.GetFileName(originFolder.GetFolderPath()).Replace(curFileName, "").TrimEnd('\\') +
                                                   "    -    " + counter + " words   ";
                    break;
                case "video":
                    var mediaDet = (IMediaDet)new MediaDet();
                    DsError.ThrowExceptionForHR(mediaDet.put_Filename(currentItem.GetFilePath()));

                    /* find the video stream in the file
                int index;
                var type = Guid.Empty;
                for (index = 0; index < 1000 && type != MediaType.Video; index++)
                {
                    mediaDet.put_CurrentStream(index);
                    mediaDet.get_StreamType(out type);
                }*/

                    // retrieve some measurements from the video

                    mediaDet.get_FrameRate(out double frameRate);


                    var mediaType = new AMMediaType();
                    mediaDet.get_StreamMediaType(mediaType);
                    var videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
                    DsUtils.FreeAMMediaType(mediaType);
                    var width = videoInfo.BmiHeader.Width;
                    var height = videoInfo.BmiHeader.Height;

                    mediaDet.get_StreamLength(out double mediaLength);
                    var frameCount = (int)(frameRate * mediaLength);
                    var duration = frameCount / frameRate;

                    // Convert time into readable format
                    var parts = new List<string>();
                    void Add(int val, string unit)
                    { if (val > 0) parts.Add(val + unit); }
                    var t = TimeSpan.FromSeconds((int)mediaLength);

                    Add(t.Days, "d");
                    Add(t.Hours, "h");
                    Add(t.Minutes, "m");
                    Add(t.Seconds, "s");

                    string formattedTime = string.Join(" ", parts);

                    string textInfo = "(" + (displayedItemIndex + 1) + "/" + _displayItems.Count + ") " + curFileName + "    -    " +
                                      Path.GetFileName(originFolder.GetFolderPath()).Replace(curFileName, "").TrimEnd('\\') +
                                      "    -    ( " + width + " x " + height + " )" +
                                      "    -    ( " + formattedTime + " )   ";

                    CurrentFileInfoLabel.Foreground = defaultTextColor;
                    CurrentFileInfoLabel.Content = textInfo;


                    mediaDet.put_Filename(null);
                    break;
            }
        }






        private void UpdateTitle()
        {
            if (_displayItems.Count > 0)
            {
                string curItem = currentItem.GetFilePath();
                Title = "(" + (displayedItemIndex + 1) + "/" + _displayItems.Count + ") ";

                if (!showSubDir)
                {
                    Title = Title + " -subdir ";
                }
                if (!showSets)
                {
                    Title = Title + " -sets ";
                }
                if (!showSets || !showSubDir)
                {
                    Title = Title + "| ";
                }

                Title = Title + Path.GetFileName(curItem);
            }
            else
            {
                Title = "Image Manager";
                if (!showSubDir)
                {
                    Title = Title + " -subdir";
                }
                if (!showSets)
                {
                    Title = Title + " -sets";
                }
            }
        }

        private void CreateSortMenu()
        {
            DirectoryTreeList.Items.Clear();

            foreach (Folder foundFolder in originFolder.GetAllFolders())
            {
                SolidColorBrush color = defaultTextColor;

                // Color directories based on content
                specialFolders.Where(c => foundFolder.GetFolderName().Contains(c.Key)).ToList().ForEach(cc => color = cc.Value);

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
            if (sortMode)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex - 1 < 0
                    ? originFolder.GetAllFolders().Count - 1
                    : DirectoryTreeList.SelectedIndex - 1;
        }

        private void MoveDown()
        {
            if (sortMode)
                DirectoryTreeList.SelectedIndex = DirectoryTreeList.SelectedIndex + 1 == 
                    originFolder.GetAllFolders().Count ? 
                    0 : DirectoryTreeList.SelectedIndex + 1;
        }
        
        private void ToggleViewMode()
        {
            sortMode = !sortMode;
            DirectoryTreeList.Visibility = sortMode ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
