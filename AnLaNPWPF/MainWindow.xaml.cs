using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AnLaNPWPF.Views;
using AnLaNPWPF.Helpers;

namespace AnLaNPWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Check database connection on startup
            if (!StartupChecker.CheckDatabaseConnection())
            {
                this.Close();
                return;
            }
        }

        private void TestConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationTest.TestConfiguration();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Hide();
        }
    }
}
