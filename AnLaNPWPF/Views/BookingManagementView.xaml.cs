using System.Windows.Controls;
using AnLaNPWPF.ViewModels;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for BookingManagementView.xaml
    /// </summary>
    public partial class BookingManagementView : UserControl
    {
        public BookingManagementView()
        {
            InitializeComponent();
            DataContext = new BookingManagementViewModel();
        }
    }
}
