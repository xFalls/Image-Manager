using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Image_Manager
{
    internal partial class CacheHandler
    {
        private const int NUM_OF_CACHED_IMAGES = 15;
        public int lastPos = 0;

        public void UpdateCache()
        {
            bool isGoingRight = true;
            int currentImageNum = Image_Manager.MainWindow.ReturnCurrentImageNum();
            
            // Find direction moved in gallery
            if (currentImageNum - lastPos < 0)
            {
                isGoingRight = false;
            }

            // Load images NUM_OF_CACHED_IMAGES steps
            if (isGoingRight)
            {
                for (int i = currentImageNum; i < currentImageNum + NUM_OF_CACHED_IMAGES && i < Image_Manager.MainWindow.filepaths.Count; i++)
                {
                    AddCache(i);
                }
            }
            else
            {
                for (int i = currentImageNum - NUM_OF_CACHED_IMAGES + 1; i <= currentImageNum && i >= 0; i++)
                {
                    AddCache(i);
                }
            }

            DropCache();
        }

        public void AddCache(int i)
        {
            if (Image_Manager.MainWindow.cache.ContainsKey(Image_Manager.MainWindow.filepaths[i])) return;
            if (Image_Manager.MainWindow.FileType(Image_Manager.MainWindow.filepaths[i]) == "image")
            {
                BitmapImage imageToCache = LoadImage(Image_Manager.MainWindow.filepaths[i]);
                Image_Manager.MainWindow.cache.Add(Image_Manager.MainWindow.filepaths[i], imageToCache);
            }
            else if (Image_Manager.MainWindow.FileType(Image_Manager.MainWindow.filepaths[i]) == "video")
            {
                // Grab thumbnail from video and cache it
                int THUMB_SIZE = 1024;
                Bitmap thumbnail = WindowsThumbnailProvider.GetThumbnail(
                    Image_Manager.MainWindow.filepaths[i], THUMB_SIZE, THUMB_SIZE, ThumbnailOptions.BiggerSizeOk);

                BitmapImage thumbnailImage = Image_Manager.MainWindow.BitmapToImageSource(thumbnail);
                thumbnail.Dispose();

                Image_Manager.MainWindow.cache.Add(Image_Manager.MainWindow.filepaths[i], thumbnailImage);

            }
        }

        private BitmapImage LoadImage(string myImageFile)
        {
            BitmapImage image = new BitmapImage();
            using (FileStream stream = File.OpenRead(myImageFile))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            BitmapImage retImage = image;
            return retImage;
        }

        public void DropCache()
        {
            int currentImageNum = Image_Manager.MainWindow.ReturnCurrentImageNum();

            // Remove image N steps back
            if (currentImageNum - NUM_OF_CACHED_IMAGES >= 0 &&
                Image_Manager.MainWindow.cache.ContainsKey(Image_Manager.MainWindow.filepaths[currentImageNum - NUM_OF_CACHED_IMAGES]))
            {
                Image_Manager.MainWindow.cache.Remove(Image_Manager.MainWindow.filepaths[currentImageNum - NUM_OF_CACHED_IMAGES]);
            }

            // Remove image N steps forward
            if (currentImageNum + NUM_OF_CACHED_IMAGES + 1 < Image_Manager.MainWindow.filepaths.Count && 
                Image_Manager.MainWindow.cache.ContainsKey(Image_Manager.MainWindow.filepaths[currentImageNum + NUM_OF_CACHED_IMAGES + 1]))
            {
                Image_Manager.MainWindow.cache.Remove(Image_Manager.MainWindow.filepaths[currentImageNum + NUM_OF_CACHED_IMAGES + 1]);
            }
        }

    }
}
