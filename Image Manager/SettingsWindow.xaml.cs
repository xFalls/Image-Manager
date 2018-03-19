using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Image_Manager.Properties;

namespace Image_Manager
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private MainWindow main;

        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();

            this.main = main;

            UpdateExperimental();
        }

        // Toggles showing experimental features
        private void UpdateExperimental()
        {
            bool afterExperimental = false;
            bool setEnabled = Settings.Default.Experimental;

            foreach (Control item in SettingValues.Children)
            {
                if (item.Name == "ExperimentalCheckbox")
                {
                    afterExperimental = true;
                    continue;
                }

                if (!afterExperimental) continue;

                item.IsEnabled = setEnabled;

                if (!setEnabled)
                {
                    ((CheckBox) item).IsChecked = false;
                }
            }
        }

        // Sets textfield settings
        private void Prefix_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || !(sender is TextBox)) return;

            // Saves the new default text to settings
            var mediaElement = (TextBox)sender;
            string text = mediaElement.Text;

            // Disallow empty characters
            if (string.IsNullOrWhiteSpace(text))
            {
                AnimateBackground(mediaElement, Colors.OrangeRed);
                return;
            }

            switch (((Control) sender).Name)
            {
                case "Prefix":
                    Settings.Default.PrefixName = text;
                    Keyboard.ClearFocus();
                    break;
                case "Steps":
                    if (int.TryParse(text, out int v) && v >= 0 && v <= 100)
                    {
                        Settings.Default.PreviewSteps = v;
                    }
                    else
                    {
                        AnimateBackground(mediaElement, Colors.OrangeRed);
                        return;
                    }

                    break;
            }

            AnimateBackground(mediaElement, Colors.GreenYellow);
        }

        // Displays a short animation to indicate that changes have been saved
        public static void AnimateBackground(Control elem, Color color)
        {
            ColorAnimation ca = new ColorAnimation(Colors.White, new Duration(TimeSpan.FromSeconds(0.5)));
            elem.Background = new SolidColorBrush(color);
            elem.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        // Updates all changed values in the main window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            main.UpdateSettingsChanged();
            main.UpdatePreviewLength();
            main.UpdateContent();
        }

        
        private void ExperimentalCheckbox_Click(object sender, RoutedEventArgs e)
        {
            UpdateExperimental();
        }
    }
}
