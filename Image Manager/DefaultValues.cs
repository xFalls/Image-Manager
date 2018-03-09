using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Image_Manager.Properties;

namespace Image_Manager
{
    /// <summary>
    /// Default values that are set and used throughout the program
    /// </summary>
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
        private bool _preferWebP = Settings.Default.PreferWebP;
        private bool _prefer1000Px = Settings.Default.Prefer1000px;
        private readonly SolidColorBrush redWarning = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush orangeWarning = new SolidColorBrush(Colors.DarkOrange);

        // Indentation distance for each subfolder level
        private const int IndentDistance = 20;

        // Naming
        private string QuickPrefix = Settings.Default.PrefixName;
        private readonly int FileNameSize = 30;

        // Inclusion
        public bool _allowOtherFiles = true;

        // Caching
        private readonly int _preloadRange = 7;
        private readonly int previewSteps = 2;

        // Blur effect on videos
        private readonly int _defaultBlurRadius = 20;


        // Apply settings
        public void UpdateSettingsChanged()
        {
            QuickPrefix = Settings.Default.PrefixName + " ";
            _prefer1000Px = Settings.Default.Prefer1000px;
            _preferWebP = Settings.Default.PreferWebP;

            UpdateTitle();
            UpdateInfobar();
        }
    }
}
