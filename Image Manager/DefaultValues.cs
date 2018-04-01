using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
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
        private const double MaxZoom = 30.0;
        private const double MinZoom = 0.5;

        // Infinite scroller zoom
        private const double InfiZoomAmount = 100;
        private const double InfiMaxZoom = 1200;
        private const double InfiMinZoom = 200;

        // Special folders
        private Dictionary<string, SolidColorBrush> _specialFolders = new Dictionary<string, SolidColorBrush>()
        {
            { "[Artist]", new SolidColorBrush(Colors.Yellow)},
            { "[Collection]", new SolidColorBrush(Colors.CornflowerBlue)},
            { "[Comic]", new SolidColorBrush(Colors.MediumPurple)},
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
        private int _preloadRange;
        private int _previewSteps;

        // Blur effect on videos
        private readonly int _defaultBlurRadius = 20;


        // Apply settings
        public void UpdateSettingsChanged(bool refreshMenu = false)
        {
            QuickPrefix = Settings.Default.PrefixName + " ";
            _prefer1000Px = Settings.Default.Prefer1000px;
            _preferWebP = Settings.Default.PreferWebP;
            _previewSteps = Settings.Default.PreviewSteps;
            _preloadRange = _previewSteps;

            

            _specialFolders.Clear();
            ConvertDictionary d = new ConvertDictionary();
            Dictionary<string, SolidColorBrush> newD = d.Read(Settings.Default.FolderColors);
            _specialFolders = newD;
            if (refreshMenu)
            {
                CreateSortMenu();
            }

            

            OpenInWaifu.Visibility = Settings.Default.Experimental ? Visibility.Visible : Visibility.Collapsed;
            WebPConvertMenu.Visibility = Settings.Default.Experimental ? Visibility.Visible : Visibility.Collapsed;
            OpenInWaifu2.Visibility = Settings.Default.Experimental ? Visibility.Visible : Visibility.Collapsed;

            UpdateTitle();
            UpdateInfobar();
        }
    }
}



/// <summary>
/// Code found and modified from
/// https://www.dotnetperls.com/convert-dictionary-string
/// </summary>
class ConvertDictionary
{
    /// <summary>
    /// Creates a dictionary from its inputted string
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public Dictionary<string, SolidColorBrush> Read(string s)
    {
        return GetDict(s);
    }


    string GetLine(Dictionary<string, SolidColorBrush> d)
    {
        // Build up each line one-by-one and then trim the end
        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<string, SolidColorBrush> pair in d)
        {
            builder.Append(pair.Key).Append(":").Append(pair.Value).Append(',');
        }
        string result = builder.ToString();
        // Remove the final delimiter
        result = result.TrimEnd(',');
        return result;
    }

    Dictionary<string, SolidColorBrush> GetDict(string s)
    {
        Dictionary<string, SolidColorBrush> d = new Dictionary<string, SolidColorBrush>();

        // Divide all pairs (remove empty strings)
        string[] tokens = s.Split(new char[] { ':', ',' },
            StringSplitOptions.RemoveEmptyEntries);

        // Walk through each item
        for (int i = 0; i < tokens.Length; i += 2)
        {
            string name = tokens[i];
            string freq = tokens[i + 1];
            SolidColorBrush brush = (SolidColorBrush)new BrushConverter().ConvertFromString(freq);

            d.Add(name, brush);
        }
        return d;
    }
}