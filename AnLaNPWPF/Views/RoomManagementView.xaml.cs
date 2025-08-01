using System;
using System.Windows.Controls;
using AnLaNPWPF.ViewModels;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for RoomManagementView.xaml
    /// </summary>
    public partial class RoomManagementView : Page
    {
        public RoomManagementView()
        {
            InitializeComponent();
            DataContext = new RoomManagementViewModel();
        }
    }
} 