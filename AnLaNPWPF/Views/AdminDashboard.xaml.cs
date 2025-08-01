using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for AdminDashboard.xaml
    /// </summary>
    public partial class AdminDashboard : Window
    {
        private DispatcherTimer timer;
        
        public AdminDashboard()
        {
            InitializeComponent();
            
            // Load the views into frames - removed ContractManagementView to avoid early loading
            RoomManagementFrame.Content = new RoomManagementView();
            ResidentManagementFrame.Content = new ResidentManagementView();
            
            // Initialize bill management with history view
            var billHistoryView = new BillHistoryView();
            billHistoryView.ShowBillGeneration += BillHistoryView_ShowBillGeneration;
            BookingManagementFrame.Content = billHistoryView;
            
            // ContractManagementFrame.Content will be loaded when needed
            ReportsFrame.Content = new ReportsView();
            
            // Start timer for current time display
            StartTimeDisplay();
            
            // Set default view to Room Management
            ShowFrame("RoomManagement");
            UpdateActiveButton("Room");
        }
        
        private void StartTimeDisplay()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            Timer_Tick(null, null); // Update immediately
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        
        private void RoomManagementBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowFrame("RoomManagement");
            UpdateActiveButton("Room");
            PageTitle.Text = "Quản lý phòng";
        }
        
        private void ResidentManagementBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowFrame("ResidentManagement");
            UpdateActiveButton("Resident");
            PageTitle.Text = "Quản lý khách hàng";
        }
        
        private void BookingManagementBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowFrame("BookingManagement");
            UpdateActiveButton("Booking");
            PageTitle.Text = "Quản lý hóa đơn";
        }
        
        private void ContractManagementBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowFrame("ContractManagement");
            UpdateActiveButton("Contract");
            PageTitle.Text = "Quản lý hợp đồng";
        }
        
        private void ReportsBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowFrame("Reports");
            UpdateActiveButton("Reports");
            PageTitle.Text = "Báo cáo thống kê";
        }
        
        private void ShowFrame(string frameName)
        {
            // Hide all frames
            RoomManagementFrame.Visibility = Visibility.Collapsed;
            BookingManagementFrame.Visibility = Visibility.Collapsed;
            ReportsFrame.Visibility = Visibility.Collapsed;
            ResidentManagementFrame.Visibility = Visibility.Collapsed;
            ContractManagementFrame.Visibility = Visibility.Collapsed;
            
            // Show selected frame
            switch (frameName)
            {
                case "RoomManagement":
                    RoomManagementFrame.Visibility = Visibility.Visible;
                    break;
                case "BookingManagement":
                    BookingManagementFrame.Visibility = Visibility.Visible;
                    break;
                case "Reports":
                    ReportsFrame.Visibility = Visibility.Visible;
                    break;
                case "ResidentManagement":
                    ResidentManagementFrame.Visibility = Visibility.Visible;
                    break;
                case "ContractManagement":
                    // Lazy load ContractManagementView if not already loaded
                    if (ContractManagementFrame.Content == null)
                    {
                        try
                        {
                            ContractManagementFrame.Content = new ContractManagementView();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi khi tải trang quản lý hợp đồng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    ContractManagementFrame.Visibility = Visibility.Visible;
                    break;
            }
        }
        
        private void UpdateActiveButton(string activeButton)
        {
            // Reset all buttons to normal style
            RoomManagementBtn.Style = (Style)FindResource("NavButton");
            BookingManagementBtn.Style = (Style)FindResource("NavButton");
            ReportsBtn.Style = (Style)FindResource("NavButton");
            ResidentManagementBtn.Style = (Style)FindResource("NavButton");
            ContractManagementBtn.Style = (Style)FindResource("NavButton");
            
            // Set active button style
            switch (activeButton)
            {
                case "Room":
                    RoomManagementBtn.Style = (Style)FindResource("ActiveNavButton");
                    break;
                case "Booking":
                    BookingManagementBtn.Style = (Style)FindResource("ActiveNavButton");
                    break;
                case "Reports":
                    ReportsBtn.Style = (Style)FindResource("ActiveNavButton");
                    break;
                case "Resident":
                    ResidentManagementBtn.Style = (Style)FindResource("ActiveNavButton");
                    break;
                case "Contract":
                    ContractManagementBtn.Style = (Style)FindResource("ActiveNavButton");
                    break;
            }
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            // Stop timer
            timer?.Stop();
            
            // Mở cửa sổ đăng nhập
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Đóng cửa sổ hiện tại
            this.Close();
        }
        
        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            base.OnClosed(e);
        }

        private void BillHistoryView_ShowBillGeneration(object sender, EventArgs e)
        {
            try
            {
                var billGenerationView = new BillGenerationView();
                billGenerationView.BackRequested += BillGenerationView_BackRequested;
                BookingManagementFrame.Content = billGenerationView;
                PageTitle.Text = "Tạo Hóa Đơn Tháng";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển sang tạo hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BillGenerationView_BackRequested(object sender, EventArgs e)
        {
            try
            {
                var billHistoryView = new BillHistoryView();
                billHistoryView.ShowBillGeneration += BillHistoryView_ShowBillGeneration;
                BookingManagementFrame.Content = billHistoryView;
                PageTitle.Text = "Quản lý hóa đơn";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi quay lại danh sách hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}