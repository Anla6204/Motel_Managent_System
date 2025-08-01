using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BusinessObject;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for RoomDetailView.xaml
    /// </summary>
    public partial class RoomDetailView : Window
    {
        private RoomInformationModel _room;

        public RoomDetailView(RoomInformationModel room)
        {
            InitializeComponent();
            _room = room;
            LoadRoomDetails();
        }

        private void LoadRoomDetails()
        {
            try
            {
                if (_room == null)
                {
                    MessageBox.Show("Không có thông tin phòng!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Room Header
                txtRoomHeader.Text = $"Phòng {_room.RoomNumber} - {_room.RoomStatusText}";

                // Room Information
                txtRoomNumber.Text = _room.RoomNumber ?? "Chưa cập nhật";
                txtRoomType.Text = _room.RoomTypes?.RoomTypeName ?? "Chưa cập nhật";
                txtRoomStatus.Text = _room.RoomStatusText ?? "Chưa cập nhật";
                txtRoomPrice.Text = $"{_room.RoomPricePerMonth:N0} VNĐ/tháng";
                txtRoomCapacity.Text = $"{_room.RoomMaxCapacity} người";
                txtCurrentOccupancy.Text = _room.OccupancyInfo;
                txtRoomDescription.Text = _room.RoomDetailDescription ?? "Không có mô tả";

                // Load Customer List
                LoadCustomerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin phòng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCustomerList()
        {
            // Clear existing customer cards
            CustomerListPanel.Children.Clear();

            if (_room.HasCustomer && _room.CurrentCustomers?.Any() == true)
            {
                // Show customer list
                CustomerScrollViewer.Visibility = Visibility.Visible;
                NoCustomersPanel.Visibility = Visibility.Collapsed;

                // Update occupancy info
                txtOccupancyInfo.Text = $"({_room.CurrentOccupancy}/{_room.RoomMaxCapacity})";

                // Create customer cards
                foreach (var customer in _room.CurrentCustomers)
                {
                    var customerCard = CreateCustomerCard(customer);
                    CustomerListPanel.Children.Add(customerCard);
                }
            }
            else
            {
                // Show no customers message
                CustomerScrollViewer.Visibility = Visibility.Collapsed;
                NoCustomersPanel.Visibility = Visibility.Visible;
                txtOccupancyInfo.Text = "(0/0)";
            }
        }

        private Border CreateCustomerCard(CustomerModel customer)
        {
            var card = new Border
            {
                Style = (Style)Resources["CustomerCardStyle"],
                Margin = new Thickness(0, 5, 0, 10)
            };

            var mainPanel = new StackPanel();

            // Customer header
            var headerPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var nameText = new TextBlock
            {
                Text = $"👤 {customer.CustomerFullName ?? "Chưa cập nhật"}",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            };

            var checkInText = new TextBlock
            {
                Text = customer.CheckInDate?.ToString("dd/MM/yyyy") ?? "Chưa có",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(149, 165, 166)),
                Margin = new Thickness(15, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            headerPanel.Children.Add(nameText);
            headerPanel.Children.Add(checkInText);

            // Customer details grid
            var detailsGrid = new Grid();
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var details = new[]
            {
                ("CCCD:", customer.CCCD ?? "Chưa cập nhật"),
                ("Điện thoại:", customer.Telephone ?? "Chưa cập nhật"),
                ("Giới tính:", customer.Gender ?? "Chưa cập nhật"),
                ("Nghề nghiệp:", customer.Occupation ?? "Chưa cập nhật")
            };

            for (int i = 0; i < details.Length; i++)
            {
                detailsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var label = new TextBlock
                {
                    Text = details[i].Item1,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                    Margin = new Thickness(0, 2, 5, 2)
                };
                Grid.SetRow(label, i);
                Grid.SetColumn(label, 0);

                var value = new TextBlock
                {
                    Text = details[i].Item2,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                    Margin = new Thickness(0, 2, 0, 2),
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(value, i);
                Grid.SetColumn(value, 1);

                detailsGrid.Children.Add(label);
                detailsGrid.Children.Add(value);
            }

            // Action buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var viewDetailButton = new Button
            {
                Content = "👁️ Xem chi tiết",
                Style = (Style)Resources["ActionButtonStyle"],
                Margin = new Thickness(0, 0, 10, 0),
                Tag = customer
            };
            viewDetailButton.Click += ViewCustomerDetail_Click;

            buttonPanel.Children.Add(viewDetailButton);

            // Assemble the card
            mainPanel.Children.Add(headerPanel);
            mainPanel.Children.Add(detailsGrid);
            mainPanel.Children.Add(buttonPanel);

            card.Child = mainPanel;
            return card;
        }

        private void ViewCustomerDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is CustomerModel customer)
                {
                    var customerDetailView = new CustomerDetailView(customer);
                    customerDetailView.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở thông tin khách hàng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var roomDialog = new RoomDialog(_room);
                if (roomDialog.ShowDialog() == true)
                {
                    // Refresh room details after edit
                    _room = roomDialog.Room;
                    LoadRoomDetails();
                    MessageBox.Show("Cập nhật thông tin phòng thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chỉnh sửa phòng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
