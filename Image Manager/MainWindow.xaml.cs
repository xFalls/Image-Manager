using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Image_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> filepaths = new List<string>();
        private int currentImageNum = 0;
        BitmapImage loadedImage;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            // Finds all filepaths of a dropped object
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            FindFilesInSubfolders(e, folder);

            UpdateImage();
        }

        private void UpdateImage()
        {
            loadedImage = new BitmapImage(new Uri(filepaths[currentImageNum], UriKind.RelativeOrAbsolute));
            imageViewer.Source = loadedImage;
        }

        // Recursively finds all files and subfolders in a folder
        private void FindFilesInSubfolders(DragEventArgs e, string[] folder)
        {
            foreach (var s in folder)
            {
                if (Directory.Exists(s))
                {
                    // Add files and subfolders from folder
                    filepaths.AddRange(Directory.GetFiles(s));
                    filepaths.AddRange(Directory.GetDirectories(s));

                    string[] subfolder = Directory.GetDirectories(s);
                    FindFilesInSubfolders(e, subfolder);
                }
                else
                {
                    // Add filepath
                    filepaths.Add(s);
                }
            }
        }

        private void ControlWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (currentImageNum > 0)
                    {
                        currentImageNum--;
                        UpdateImage();
                    }
                    break;
                case Key.Right:
                    if (currentImageNum + 1 < filepaths.Count)
                    {
                        currentImageNum++;
                        UpdateImage();
                    }
                    break;
            }
        }
    }
}
