using System.Collections.Generic;
using System.Windows.Media;

namespace Image_Manager
{
    partial class MainWindow
    {

        // Zoom
        private const double ZoomAmountButton = 0.2;
        private const double ZoomAmountWheel = 0.1;
        private const double MaxZoom = 3.0;
        private const double MinZoom = 0.5;

        // Special folders
        private readonly Dictionary<string, SolidColorBrush> _specialFolders = new Dictionary<string, SolidColorBrush>()
        {
            { "[Artist]", new SolidColorBrush(Colors.Yellow)},
            { "[Collection]", new SolidColorBrush(Colors.CornflowerBlue)},
            { "[Manga]", new SolidColorBrush(Colors.MediumPurple)},
            { "[Set]", new SolidColorBrush(Colors.Orange)}
        };

        // UI colors
        private readonly SolidColorBrush _defaultTextColor = new SolidColorBrush(Colors.White);
        private readonly bool _preferWebP = true;
        private readonly bool _prefer1000Px = true;
        private readonly SolidColorBrush _notOver1000PxWarningTextColor = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush _notWebPWarningTextColor = new SolidColorBrush(Colors.DarkOrange);

        private const int IndentDistance = 20;

        private const string QuickPrefix = "+HQ ";

        // Caching
        private readonly int _preloadRange = 15;
    }
}
