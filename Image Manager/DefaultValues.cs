using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly Dictionary<string, SolidColorBrush> specialFolders = new Dictionary<string, SolidColorBrush>()
        {
            { "[Artist]", new SolidColorBrush(Colors.Yellow)},
            { "[Collection]", new SolidColorBrush(Colors.CornflowerBlue)},
            { "[Manga]", new SolidColorBrush(Colors.MediumPurple)},
            { "[Set]", new SolidColorBrush(Colors.Orange)}
        };

        // UI colors
        private SolidColorBrush defaultTextColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush warningTextColor = new SolidColorBrush(Colors.Red);
        private SolidColorBrush selectionColor = new SolidColorBrush(Colors.Blue);
        private SolidColorBrush unSelectedColor = new SolidColorBrush(Colors.Transparent);

        private const int IndentDistance = 20;
    }
}
