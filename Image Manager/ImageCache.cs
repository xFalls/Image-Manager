using System;
using System.Collections.Generic;

namespace Image_Manager
{
    partial class MainWindow
    {
        // Keeps track of whether an object has been cached
        List<bool> isInCache = new List<bool>();

        private void AddToCache()
        {
            for (int scanRange = _displayedItemIndex - _preloadRange; scanRange < _displayedItemIndex + _preloadRange; scanRange++)
            {
                if (scanRange < 0 || scanRange >= isInCache.Count || isInCache[scanRange]) continue;

                isInCache[scanRange] = true;
                _displayItems[scanRange].PreloadContent();
            }

            DropFromCache();
        }

        private void DropFromCache()
        {
            for (var index = 0; index < isInCache.Count; index++)
            {
                if (index >= _displayedItemIndex - _preloadRange && index <= _displayedItemIndex + _preloadRange) continue;

                isInCache[index] = false;
                _displayItems[index].RemovePreloadedContent();
            }
            GC.Collect();
        }
    }
}