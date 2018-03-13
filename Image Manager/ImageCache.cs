using System;
using System.Collections.Generic;

namespace Image_Manager
{
    partial class MainWindow
    {
        // Keeps track of whether an object has been cached
        List<bool> isInCache = new List<bool>();

        // Preloads all items within a preset range of the currently viewed one.
        private void AddToCache()
        {
            for (int scanRange = _displayedItemIndex - _preloadRange; scanRange < _displayedItemIndex + _preloadRange + 1; scanRange++)
            {
                // Don't load items out of bounds or already loaded ones.
                if (scanRange < 0 || scanRange >= isInCache.Count || isInCache[scanRange]) continue;

                isInCache[scanRange] = true;
                
                _displayItems[scanRange].PreloadContent();
                
            }

            DropFromCache();
        }

        // Drops all items outside the predefined range from the cache.
        private void DropFromCache()
        {
            for (var index = 0; index < isInCache.Count; index++)
            {
                if (index >= _displayedItemIndex - _preloadRange && index <= _displayedItemIndex + _preloadRange) continue;

                isInCache[index] = false;
                _displayItems[index].RemovePreloadedContent();
            }
            // Just in case files are large, throw them away 
            // immediately to prevent potential memory errors
            GC.Collect();
        }
    }
}