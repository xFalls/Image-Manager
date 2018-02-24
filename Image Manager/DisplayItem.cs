using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using DirectShowLib;
using DirectShowLib.DES;
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
        protected string PermFilePath;

        protected string FileName;
        protected string FileNameExludingExtension;
        protected string FileExtension;
        protected string FileType;
        protected string FileLocation;
        protected string LocalLocation;
        protected string InfoBarDefaultContent;

        public static string RootFolder;


        protected DisplayItem(string name)
        {
            FilePath = PermFilePath = name;

            FileName = Path.GetFileName(name);
            FileNameExludingExtension = Path.GetFileNameWithoutExtension(name);
            FileExtension = Path.GetExtension(name).ToLower();
            FileLocation = Path.GetDirectoryName(name);

            LocalLocation = FileLocation.Replace(RootFolder, "").TrimStart('\\');
            InfoBarDefaultContent = FileName + "    -    " + LocalLocation;
        }

        public string GetTypeOfFile()
        {
            return FileType;
        }

        public string GetFileNameExcludingExtension()
        {
            return FileNameExludingExtension;
        }

        public string GetFileName()
        {
            return FileName;
        }

        public string GetFilePath()
        {
            return FilePath;
        }

        public string GetOldFilePath()
        {
            return PermFilePath;
        }

        public void SetFilePath(string newPath)
        {
            FilePath = newPath;
            FileLocation = Path.GetDirectoryName(newPath);

            FileName = Path.GetFileName(newPath);
            FileNameExludingExtension = Path.GetFileNameWithoutExtension(newPath);
            FileExtension = Path.GetExtension(newPath).ToLower();
            FileLocation = Path.GetDirectoryName(newPath);
            InfoBarDefaultContent = FileName + "    -    " + LocalLocation;
        }

        public string GetFileExtension()
        {
            return FileExtension;
        }

        public string GetLocation()
        {
            return FileLocation;
        }

        public virtual void PreloadContent()
        {
        }

        public virtual void RemovePreloadedContent()
        {
        }

        public virtual string GetInfobarContent()
        {
            return InfoBarDefaultContent;
        }

        public override string ToString()
        {
            return FilePath;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ImageItem : DisplayItem
    {
        private BitmapImage ImageSource;
        public int ImageHeight;
        private int ImageWidth;


        public ImageItem(string name) : base(name)
        {
            FileType = "image";
        }


        public override string GetInfobarContent()
        {
            return InfoBarDefaultContent + "    -    ( " + ImageWidth + " x " + ImageHeight + " )";
        }

        public override void PreloadContent()
        {
            ImageSource = LoadImage(FilePath);
        }

        public override void RemovePreloadedContent()
        {
            ImageSource = null;
        }

        public int GetSize()
        {
            return ImageHeight;
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

                if (ImageHeight != 0 || ImageWidth != 0) return image;
                ImageHeight = image.PixelHeight;
                ImageWidth = image.PixelWidth;
            }
            return image;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class GifItem : DisplayItem
    {
        private BitmapImage gifImage;

        public GifItem(string name) : base(name)
        {
            FileType = "gif";
        }


        public BitmapImage GetGif(Image viewer)
        {
            return LoadGif(FilePath, viewer);
        }

        public override void RemovePreloadedContent()
        {
            gifImage = null;
        }

        public BitmapImage LoadGif(string myGifFile, Image viewer)
        {
            gifImage = new BitmapImage();
            using (FileStream stream = File.OpenRead(myGifFile))
            {
                gifImage.BeginInit();
                gifImage.CacheOption = BitmapCacheOption.OnLoad;
                gifImage.StreamSource = stream;
                ImageBehavior.SetAnimatedSource(viewer, gifImage);
                gifImage.EndInit();
            }

            return gifImage;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class VideoItem : DisplayItem
    {
        private BitmapImage thumbnailSource;
        private int videoResolutionWidth;
        private int videoResolutionHeight;
        private string videoLength;


        public VideoItem(string name) : base(name)
        {
            FileType = "video";
        }


        public override string GetInfobarContent()
        {
            return InfoBarDefaultContent + "    -    ( " + videoResolutionWidth + " x " + videoResolutionHeight + " )" +
                   "    -    ( " + videoLength + " )";
        }

        public override void PreloadContent()
        {
            // If the values returned are nonsensical, retry
            while (videoResolutionHeight == 0 ||
                   videoResolutionWidth < -100000 || videoResolutionHeight < -100000 ||
                   videoResolutionWidth > 100000 || videoResolutionHeight > 100000)
                GetMetaData();

            thumbnailSource = LoadThumbnail(FilePath);
        }

        public override void RemovePreloadedContent()
        {
            thumbnailSource = null;
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

        public void GetMetaData()
        {
            var mediaDet = (IMediaDet) new MediaDet();
            DsError.ThrowExceptionForHR(mediaDet.put_Filename(FilePath));

            // retrieve some measurements from the video

            var mediaType = new AMMediaType();
            mediaDet.get_StreamMediaType(mediaType);
            var videoInfo = (VideoInfoHeader) Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
            DsUtils.FreeAMMediaType(mediaType);

            videoResolutionWidth = videoInfo.BmiHeader.Width;
            videoResolutionHeight = videoInfo.BmiHeader.Height;

            mediaDet.get_StreamLength(out double mediaLength);

            // Convert time into readable format
            var parts = new List<string>();

            void Add(int val, string unit)
            {
                if (val > 0) parts.Add(val + unit);
            }

            var t = TimeSpan.FromSeconds((int) mediaLength);

            Add(t.Days, "d");
            Add(t.Hours, "h");
            Add(t.Minutes, "m");
            Add(t.Seconds, "s");

            videoLength = string.Join(" ", parts);

            mediaDet.put_Filename(null);
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


        public override string GetInfobarContent()
        {
            if (wordAmount == null) CountWords();
            return InfoBarDefaultContent + "    -    ( " + wordAmount + " words )";
        }

        public void CountWords()
        {
            StreamReader sr = new StreamReader(FilePath);

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

            wordAmount = counter.ToString();
        }

        public string GetText()
        {
            return textContent;
        }
    }
}
