using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BusinessObject;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for CustomerDetailView.xaml
    /// </summary>
    public partial class CustomerDetailView : Window
    {
        private CustomerModel _customer;

        public CustomerDetailView(CustomerModel customer)
        {
            InitializeComponent();
            _customer = customer;
            LoadCustomerDetails();
        }

        private void LoadCustomerDetails()
        {
            try
            {
                if (_customer == null)
                {
                    MessageBox.Show("Không có thông tin khách hàng!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Header
                txtCustomerName.Text = _customer.CustomerFullName ?? "Chưa có tên";

                // Personal Information
                txtFullName.Text = _customer.CustomerFullName ?? "Chưa cập nhật";
                txtEmail.Text = _customer.EmailAddress ?? "Chưa cập nhật";
                txtTelephone.Text = _customer.Telephone ?? "Chưa cập nhật";
                txtGender.Text = _customer.Gender ?? "Chưa cập nhật";
                txtCCCD.Text = _customer.CCCD ?? "Chưa cập nhật";
                txtBirthday.Text = _customer.CustomerBirthday.ToString("dd/MM/yyyy");
                txtAddress.Text = _customer.Address ?? "Chưa cập nhật";

                // Residence Information
                txtCurrentRoom.Text = !string.IsNullOrEmpty(_customer.CurrentRoomNumber) 
                    ? $"Phòng {_customer.CurrentRoomNumber}" 
                    : "Chưa được phân phòng";
                txtCheckInDate.Text = _customer.CheckInDate?.ToString("dd/MM/yyyy") ?? "Chưa cập nhật";
                txtOccupation.Text = _customer.Occupation ?? "Chưa cập nhật";
                txtEmergencyContact.Text = _customer.EmergencyContact ?? "Chưa cập nhật";
                txtNotes.Text = _customer.Notes ?? "Không có ghi chú";

                // Load CCCD Images
                LoadCCCDImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin khách hàng: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCCCDImages()
        {
            try
            {
                // Load Front Image
                if (!string.IsNullOrEmpty(_customer.CCCDFrontImagePath) && 
                    File.Exists(_customer.CCCDFrontImagePath))
                {
                    BitmapImage frontImage = new BitmapImage();
                    frontImage.BeginInit();
                    frontImage.UriSource = new Uri(_customer.CCCDFrontImagePath);
                    frontImage.CacheOption = BitmapCacheOption.OnLoad;
                    frontImage.EndInit();
                    imgCCCDFront.Source = frontImage;
                    btnViewFront.IsEnabled = true;
                }
                else
                {
                    imgCCCDFront.Source = null;
                    btnViewFront.IsEnabled = false;
                }

                // Load Back Image
                if (!string.IsNullOrEmpty(_customer.CCCDBackImagePath) && 
                    File.Exists(_customer.CCCDBackImagePath))
                {
                    BitmapImage backImage = new BitmapImage();
                    backImage.BeginInit();
                    backImage.UriSource = new Uri(_customer.CCCDBackImagePath);
                    backImage.CacheOption = BitmapCacheOption.OnLoad;
                    backImage.EndInit();
                    imgCCCDBack.Source = backImage;
                    btnViewBack.IsEnabled = true;
                }
                else
                {
                    imgCCCDBack.Source = null;
                    btnViewBack.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ảnh CCCD: {ex.Message}", 
                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResidentDialog dialog = new ResidentDialog(_customer);
                if (dialog.ShowDialog() == true && dialog.IsSaved)
                {
                    // Refresh the display with updated information
                    _customer = dialog.Resident;
                    LoadCustomerDetails();
                    MessageBox.Show("Thông tin khách hàng đã được cập nhật!", 
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở cửa sổ chỉnh sửa: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create print dialog
                System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                
                if (printDialog.ShowDialog() == true)
                {
                    // Create printable content
                    var printContent = CreatePrintableContent();
                    
                    // Print the content
                    printDialog.PrintVisual(printContent, $"Thông tin khách hàng - {_customer.CustomerFullName}");
                    
                    MessageBox.Show("Đã gửi lệnh in thông tin khách hàng!", 
                        "In thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Windows.Controls.Grid CreatePrintableContent()
        {
            // Create a simplified grid for printing
            var printGrid = new System.Windows.Controls.Grid();
            printGrid.Background = System.Windows.Media.Brushes.White;
            printGrid.Margin = new Thickness(20);

            // Add content (simplified version of the display)
            var stackPanel = new System.Windows.Controls.StackPanel();
            
            // Title
            var title = new System.Windows.Controls.TextBlock
            {
                Text = "THÔNG TIN KHÁCH HÀNG",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(title);

            // Customer info
            var infoText = new System.Windows.Controls.TextBlock
            {
                Text = $"Họ tên: {_customer.CustomerFullName}\n" +
                       $"Email: {_customer.EmailAddress}\n" +
                       $"Số điện thoại: {_customer.Telephone}\n" +
                       $"CCCD: {_customer.CCCD}\n" +
                       $"Ngày sinh: {_customer.CustomerBirthday.ToString("dd/MM/yyyy")}\n" +
                       $"Địa chỉ: {_customer.Address}\n" +
                       $"Phòng: {_customer.CurrentRoomNumber}\n" +
                       $"Ngày vào ở: {_customer.CheckInDate?.ToString("dd/MM/yyyy")}",
                FontSize = 12,
                LineHeight = 20
            };
            stackPanel.Children.Add(infoText);

            printGrid.Children.Add(stackPanel);
            return printGrid;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnViewFront_Click(object sender, RoutedEventArgs e)
        {
            ViewImageFullScreen(_customer.CCCDFrontImagePath, "CCCD Mặt trước");
        }

        private void btnViewBack_Click(object sender, RoutedEventArgs e)
        {
            ViewImageFullScreen(_customer.CCCDBackImagePath, "CCCD Mặt sau");
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // Double click
            {
                var image = sender as System.Windows.Controls.Image;
                if (image == imgCCCDFront && !string.IsNullOrEmpty(_customer.CCCDFrontImagePath))
                {
                    ViewImageFullScreen(_customer.CCCDFrontImagePath, "CCCD Mặt trước");
                }
                else if (image == imgCCCDBack && !string.IsNullOrEmpty(_customer.CCCDBackImagePath))
                {
                    ViewImageFullScreen(_customer.CCCDBackImagePath, "CCCD Mặt sau");
                }
            }
        }

        private void ViewImageFullScreen(string imagePath, string title)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                {
                    MessageBox.Show("Không tìm thấy file ảnh!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                ImageViewerWindow viewer = new ImageViewerWindow(imagePath, title);
                viewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở ảnh: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
