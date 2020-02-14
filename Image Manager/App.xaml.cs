using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Image_Manager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();

            
            if (e.Args.Length == 1)
            {
                string[] folder = {e.Args[0]};

                wnd.AddNewFolder(folder);
                //MessageBox.Show("Now opening file: \n\n" + e.Args[0]);
                //wnd.Show();
            }
        }
    }


}
