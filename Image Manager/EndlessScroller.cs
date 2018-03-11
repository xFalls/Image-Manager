using System;
using System.Windows;
using System.Windows.Controls;

namespace Image_Manager
{
    partial class MainWindow
    {
        private readonly int PreloadedScrollImages = 5;
        private int _preloadedScrollAhead;
        private int _loadedOffset;
        private int startingImage;

        // Creates a new row for images
        public void AddNewRow(bool insertLast)
        {
            Image uc = new Image { VerticalAlignment = VerticalAlignment.Top };

            if (insertLast)
            {
                InfiScroll.Children.Add(uc);
            }
            /*else
            {
                InfiScroll.Children.Insert(0, uc);
            }*/
        }

        // Dynamically loads new images as you scroll down
        private void InfiScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (InfiScrollViewer.VerticalOffset / InfiScrollViewer.ScrollableHeight > 0.95 || Math.Abs(InfiScrollViewer.VerticalOffset - InfiScrollViewer.ScrollableHeight) < 5)
            {
                if (_displayedItemIndex + _loadedOffset + _preloadedScrollAhead >= _displayItems.Count - 1) return;

                double lastPos = InfiScrollViewer.VerticalOffset;
                double originalSize = InfiScrollViewer.ScrollableHeight;

                //InfiScroll.Children.RemoveAt(0);
                InfiScroll.UpdateLayout();

                double lostSize = originalSize - InfiScrollViewer.ScrollableHeight;
                double scrollTo = lastPos - lostSize;

                InfiScrollViewer.ScrollToVerticalOffset(scrollTo);

                AddNewRow(true);

                int selection = _displayedItemIndex + _loadedOffset + _preloadedScrollAhead + 1;

                // All scrolled to content will remain in memory
                _displayItems[selection].PreloadContent();


                ((Image)InfiScroll.Children[InfiScroll.Children.Count - 1]).Source =
                    _displayItems[selection].GetTypeOfFile() == "image"
                        ? ((ImageItem)_displayItems[selection]).GetImage()
                        : _displayItems[selection].GetThumbnail();

                _loadedOffset++;
            }

            // Buggy implementation of support for scrolling up

            /*else if (InfiScrollViewer.VerticalOffset / InfiScrollViewer.ScrollableHeight < 0.05)
            {
                if (_displayedItemIndex + loadedOffset - preloadedScrollAhead <= 0) return;

                double lastPos = InfiScrollViewer.VerticalOffset;
                double originalSize = InfiScrollViewer.ScrollableHeight;

                InfiScroll.Children.RemoveAt(InfiScroll.Children.Count - 1);
                InfiScroll.UpdateLayout();

                double lostSize = originalSize - InfiScrollViewer.ScrollableHeight;
                double scrollTo = lastPos + lostSize;

                InfiScrollViewer.ScrollToVerticalOffset(scrollTo);

                AddNewRow(false);

                int selection = _displayedItemIndex + loadedOffset - preloadedScrollAhead - 1;

                ((Image)InfiScroll.Children[0]).Source =
                    _displayItems[selection].GetTypeOfFile() == "image"
                        ? ((ImageItem)_displayItems[selection]).GetImage()
                        : _displayItems[selection].GetThumbnail();

                loadedOffset--;
            }*/
        }

        // Adds the initial images to scroller
        private void InitializeInfiniteScroller()
        {
            _preloadedScrollAhead = (PreloadedScrollImages - 1) / 2;

            for (int i = 0, iteration = 0; i <= _preloadedScrollAhead; i++, iteration++)
            {
                AddNewRow(true);

                // Don't create rows if outside the limits
                if (_displayedItemIndex + i < 0 || _displayedItemIndex + i >= _displayItems.Count)
                {
                    iteration--;
                    continue;
                }

                ((Image)InfiScroll.Children[iteration]).Source =
                    _displayItems[_displayedItemIndex + i].GetTypeOfFile() == "image"
                        ? ((ImageItem)_displayItems[_displayedItemIndex + i]).GetImage()
                        : _displayItems[_displayedItemIndex + i].GetThumbnail();
            }
        }

        // Opens the endless viewing mode and initializes all settings
        private void OpenEndlessView()
        {
            if (!_isEndless)
            {
                startingImage = _displayedItemIndex;

                InfiScrollViewer.Visibility = Visibility.Visible;
                InitializeInfiniteScroller();
                InfiScrollViewer.ScrollToVerticalOffset(0);

                foreach (Control item in ViewMenu.Items)
                {
                    if (item is MenuItem && item.Name != "ShowSortEndless" && item.Name != "ZoomInMenu" &&
                        item.Name != "ZoomOutMenu")
                    {
                        item.IsEnabled = false;
                    }
                }

                ShowSortEndless.IsChecked = true;
                EditMenu.IsEnabled = false;
                OpenMenu.IsEnabled = false;

                MakeTypeVisible("");
            }
            else
            {
                _displayedItemIndex = startingImage;
                startingImage = 0;

                InfiScrollViewer.Visibility = Visibility.Hidden;
                InfiScroll.Children.RemoveRange(0, InfiScroll.Children.Count);
                InfiScrollViewer.ScrollToVerticalOffset(400);

                foreach (Control item in ViewMenu.Items)
                {
                    if (item is MenuItem)
                    {
                        item.IsEnabled = true;
                    }
                }

                ShowSortEndless.IsChecked = false;
                EditMenu.IsEnabled = true;
                OpenMenu.IsEnabled = true;

                UpdateContent();
            }
            _isEndless = !_isEndless;
        }
    }
}