﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Image_Manager.Properties;
using Microsoft.VisualBasic;
using Shell32;
using Folder = Image_Manager.Folder;

namespace Image_Manager
{
    partial class MainWindow
    {
        // Undo
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UndoMove();
        }

        // Toggle sort view
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ToggleViewMode();
        }

        // Rename
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            string input = Interaction.InputBox("Rename", "Select a new name",
                _currentItem.GetFileNameExcludingExtension());
            RenameFile(input);
        }

        // Add prefix
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            string hqFileName =
                Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
            string hqInput = QuickPrefix + hqFileName;
            RenameFile(hqInput);
        }

        // Remove prefix
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            string hQnoFileName =
                Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
            string hQnoInput = hQnoFileName?.Replace(QuickPrefix, "");
            RenameFile(hQnoInput);
        }

        // Open folder
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowDialog();
                Console.WriteLine(dialog.SelectedPath);
                AddNewFolder(new[] { dialog.SelectedPath });
            }
        }

        // Clear view
        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            RemoveOldContext();
            DirectoryTreeList.Items.Clear();

            _originFolder?.GetAllFolders()?.Clear();
            _originFolder?.GetAllShownFolders()?.Clear();
            MakeTypeVisible("");
            RecentItems.Visibility = Visibility.Visible;
        }

        // Toggle subfolders
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            _showSubDir = !_showSubDir;
            UpdateTitle();
        }

        // Toggle special folders
        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            _showSets = !_showSets;
            UpdateTitle();
        }

        // Toggle prefixed content
        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            _showPrefix = !_showPrefix;
            UpdateTitle();
        }

        // Toggle prefixed content
        private void MenuItem_Click_12(object sender, RoutedEventArgs e)
        {
            _allowOtherFiles = !_allowOtherFiles;
            UpdateTitle();
        }

        // Toggle endless mode
        private void MenuItem_Click_18(object sender, RoutedEventArgs e)
        {
            OpenEndlessView();
        }

        // Settings Window
        private void MenuItem_Click_13(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).Show();
        }

        // Open README
        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            Process.Start("README.md");
        }

        // Open externally
        private void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            try
            {
                Process.Start("rundll32.exe", "shell32.dll, OpenAs_RunDLL " + _currentItem.GetFilePath());
                //Process.Start(_currentItem.GetFilePath());
            }
            catch
            {
                Interaction.MsgBox("Failed to open file");
            }
        }

        // Toggles preview bar
        private void MenuItem_Click_14(object sender, RoutedEventArgs e)
        {
            PreviewField.Visibility = (PreviewField.Visibility == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
            Settings.Default.IsPreviewOpen = PreviewField.Visibility == Visibility.Visible;
            ShowSortPreview.IsChecked = PreviewField.Visibility == Visibility.Visible;
        }

        // Remove current file
        private void MenuItem_Click_15(object sender, RoutedEventArgs e)
        {
            RemoveFile();
        }

        // Convert to WebP file format
        private void MenuItem_Click_23(object sender, RoutedEventArgs e)
        {
            WebPConverter();
        }

        // Open current folder
        private void MenuItem_Click_17(object sender, RoutedEventArgs e)
        {
            Process.Start(_currentItem.GetLocation());
        }

        // Toggle fullscreen
        private void MenuItem_Click_21(object sender, RoutedEventArgs e)
        {
            MakeFullscreen();
        }
        
        // Upscale
        private void MenuItem_Click_22(object sender, RoutedEventArgs e)
        {
            UpscaleFile(true);
        }
        
        // Denoise
        private void MenuItem_Click_26(object sender, RoutedEventArgs e)
        {
            UpscaleFile(false);
        }

        // Filter
        private void MenuItem_Click_24(object sender, RoutedEventArgs e)
        {
            ToggleSortField();
        }

        // Refresh
        private void MenuItem_Click_25(object sender, RoutedEventArgs e)
        {
            RefreshAll();
        }

        // Zoom in
        private void MenuItem_Click_19(object sender, RoutedEventArgs e)
        {
            if (_isEndless)
            {
                if (InfiScroll.Width < InfiMaxZoom)
                {
                    InfiScroll.Width += InfiZoomAmount;
                }
            }
            else
            {
                Zoom(ZoomAmountButton);
            }
        }

        // Zoom out
        private void MenuItem_Click_20(object sender, RoutedEventArgs e)
        {
            if (_isEndless)
            {
                if (InfiScroll.Width > InfiMinZoom)
                {
                    InfiScroll.Width -= InfiZoomAmount;
                }
            }
            else
            {
                Zoom(-ZoomAmountButton);
            }
        }

        // Recent folders
        private void MenuItem_Click_27(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menu = (System.Windows.Controls.MenuItem)sender;

            string[] fol = { (string) menu.Header };
            AddNewFolder(fol);
        }

        // Recent menu
        private void MenuItem_Click_28(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ListViewItem menu = (System.Windows.Controls.ListViewItem)sender;

            string[] fol = { (string)menu.Content };
            AddNewFolder(fol);
        }
    }
}