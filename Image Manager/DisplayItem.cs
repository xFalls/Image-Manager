using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using Image = System.Windows.Controls.Image;

namespace Image_Manager
{

    /// <summary>
    /// Base class inherited by all viewable types of files.
    /// Contains basic information that all files have.
    /// </summary>
    public abstract class DisplayItem
    {
        // TODO - Make some readonly
        protected string FilePath;
        protected string FileName;
        protected string FileNameExludingExtension;
        protected string FileExtension;
        protected string FileType;
        protected string FileLocation;


        protected DisplayItem(string name)
        {
            FilePath = name;

            FileName = Path.GetFileName(name);
            FileNameExludingExtension = Path.GetFileNameWithoutExtension(name);
            FileExtension = Path.GetExtension(name);
            FileLocation = Path.GetDirectoryName(name);
        }

        public string GetTypeOfFile()
        {
            return FileType;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ImageItem : DisplayItem
    {
        private readonly BitmapImage ImageSource;
        private readonly string ImageResolution;


        public ImageItem(string name) : base(name)
        {
            FileType = "image";
            ImageSource = LoadImage(name);
            
        }


        public BitmapImage GetImage()
        {
            return ImageSource;
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

        public override string ToString()
        {
            return FilePath;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class GifItem : DisplayItem
    {

        public GifItem(string name) : base(name)
        {
            FileType = "gif";
        }

        public BitmapImage GetGif(Image viewer)
        {
            return LoadGif(FilePath, viewer);
        }

        public BitmapImage LoadGif(string myGifFile, Image viewer)
        {
            BitmapImage image = new BitmapImage();
            using (FileStream stream = File.OpenRead(myGifFile))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                ImageBehavior.SetAnimatedSource(viewer, image);
                image.EndInit();
            }

            return image;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class VideoItem : DisplayItem
    {
        private BitmapImage thumbnailSource;
        private string videoResolution;
        private string videoLength;


        public VideoItem(string name) : base(name)
        {
            FileType = "video";
            thumbnailSource = LoadThumbnail(FilePath);
        }

        public BitmapImage GetThumbnail()
        {
            return thumbnailSource;
        }

        private BitmapImage LoadThumbnail(string myThumbnail)
        {
            const int THUMB_SIZE = 1024;
            Bitmap thumbnail = WindowsThumbnailProvider.GetThumbnail(
                myThumbnail, THUMB_SIZE, THUMB_SIZE, ThumbnailOptions.BiggerSizeOk);

            BitmapImage thumbnailImage = BitmapToImageSource(thumbnail);
            thumbnail.Dispose();

            return thumbnailImage;
        }

        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();

                bitmapimage.BeginInit();
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.StreamSource = memory;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class TextItem : DisplayItem
    {
        private string wordAmount;
        private string textContent;


        public TextItem(string name) : base(name)
        {
            FileType = "text";
            textContent = "\n\n" + File.ReadAllText(FilePath);
        }

        public string GetText()
        {
            return textContent;
        }
    }
}
