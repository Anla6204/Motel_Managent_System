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
using AnLaNPWPF.ViewModels;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for ResidentManagementView.xaml
    /// </summary>
    public partial class ResidentManagementView : Page
    {
        public ResidentManagementView()
        {
            InitializeComponent();
            this.DataContext = new ResidentManagementViewModel();
        }
    }
}
