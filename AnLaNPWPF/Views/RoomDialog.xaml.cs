using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for RoomDialog.xaml
    /// </summary>
    public partial class RoomDialog : Window
    {
        private readonly RoomInfomationRepository _repository;
        public RoomInformationModel Room { get; private set; }
        private bool _isEditMode;

        public RoomDialog(RoomInformationModel room = null)
        {
            InitializeComponent();
            _repository = new RoomInfomationRepository();

            _isEditMode = room != null && room.RoomID > 0;

            // Create a copy of the room to avoid direct modification
            if (room != null)
            {
                Room = new RoomInformationModel
                {
                    RoomID = room.RoomID,
                    RoomNumber = room.RoomNumber,
                    RoomDetailDescription = room.RoomDetailDescription,
                    RoomMaxCapacity = room.RoomMaxCapacity > 0 ? room.RoomMaxCapacity : 2,
                    RoomPricePerMonth = room.RoomPricePerMonth,
                    RoomStatus = room.RoomStatus >= 0 ? room.RoomStatus : 0, // Default to "Sẵn sàng cho thuê"
                    RoomTypeID = room.RoomTypeID,
                    RoomStatusText = room.RoomStatusText // Copy existing RoomStatusText
                };
                
                // Đảm bảo đồng bộ RoomStatusText với RoomStatus nếu chưa có
                if (string.IsNullOrEmpty(Room.RoomStatusText))
                {
                    SyncRoomStatusText();
                }
            }
            else
            {
                // New room with default values
                Room = new RoomInformationModel
                {
                    RoomID = 0,
                    RoomNumber = "",
                    RoomDetailDescription = "",
                    RoomMaxCapacity = 2,
                    RoomPricePerMonth = 0,
                    RoomStatus = 0, // Default: Sẵn sàng cho thuê
                    RoomTypeID = 0
                };
                
                // Set default RoomStatusText for new room
                SyncRoomStatusText();
            }

            InitializeUI();
            LoadRoomTypes();
            DataContext = Room;
        }

        private void InitializeUI()
        {
            // Set title based on mode
            TitleText.Text = _isEditMode ? "Chỉnh sửa thông tin phòng" : "Thêm phòng mới";

            // Initialize capacity combo box
            foreach (ComboBoxItem item in CapacityComboBox.Items)
            {
                if (int.Parse(item.Tag.ToString()) == Room.RoomMaxCapacity)
                {
                    CapacityComboBox.SelectedItem = item;
                    break;
                }
            }

            // Initialize status combo box
            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (int.Parse(item.Tag.ToString()) == Room.RoomStatus)
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }

            // Handle selection changed events
            CapacityComboBox.SelectionChanged += (s, e) =>
            {
                if (CapacityComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    Room.RoomMaxCapacity = int.Parse(selectedItem.Tag.ToString());
                }
            };

            StatusComboBox.SelectionChanged += (s, e) =>
            {
                if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    Room.RoomStatus = int.Parse(selectedItem.Tag.ToString());
                    
                    // Đồng bộ RoomStatusText theo format của Repository
                    switch (Room.RoomStatus)
                    {
                        case 0:
                            Room.SetRoomStatus("⭕ Phòng trống");
                            break;
                        case 1:
                            Room.SetRoomStatus("🟢 Hoạt động");
                            break;
                        case 2:
                            Room.SetRoomStatus("🔧 Bảo trì");
                            break;
                    }
                }
            };

            // Focus on room number for new rooms
            if (!_isEditMode)
            {
                Loaded += (s, e) => RoomNumberTextBox.Focus();
            }
        }

        private void LoadRoomTypes()
        {
            try
            {
                var roomTypes = _repository.GetAllRoomTypes();
                RoomTypeComboBox.ItemsSource = roomTypes;

                // Select the current room type if editing
                if (_isEditMode && Room.RoomTypeID > 0)
                {
                    RoomTypeComboBox.SelectedValue = Room.RoomTypeID;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách loại phòng: {ex.Message}", 
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
                return;

            try
            {
                // Set default values if not provided
                if (Room.RoomMaxCapacity <= 0)
                {
                    Room.RoomMaxCapacity = 2;
                }

                if (string.IsNullOrWhiteSpace(Room.RoomDetailDescription))
                {
                    Room.RoomDetailDescription = "Phòng tiêu chuẩn với đầy đủ tiện nghi cơ bản";
                }

                // Check for duplicate room number when adding new room or changing room number
                if (!_isEditMode || HasRoomNumberChanged())
                {
                    if (IsRoomNumberExists(Room.RoomNumber))
                    {
                        MessageBox.Show($"Số phòng '{Room.RoomNumber}' đã tồn tại. Vui lòng chọn số phòng khác.",
                                      "Trùng lặp số phòng", MessageBoxButton.OK, MessageBoxImage.Warning);
                        RoomNumberTextBox.Focus();
                        return;
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin phòng: {ex.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            // Validate room number
            if (string.IsNullOrWhiteSpace(Room.RoomNumber))
            {
                MessageBox.Show("Vui lòng nhập số phòng.", 
                              "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomNumberTextBox.Focus();
                return false;
            }

            // Validate room number format (optional: can add regex validation)
            if (Room.RoomNumber.Length < 2)
            {
                MessageBox.Show("Số phòng phải có ít nhất 2 ký tự.", 
                              "Số phòng không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomNumberTextBox.Focus();
                return false;
            }

            // Validate room type
            if (Room.RoomTypeID == 0 || RoomTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại phòng.", 
                              "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomTypeComboBox.Focus();
                return false;
            }

            // Validate room price
            if (Room.RoomPricePerMonth <= 0)
            {
                MessageBox.Show("Giá phòng phải lớn hơn 0.", 
                              "Giá phòng không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomPriceTextBox.Focus();
                return false;
            }

            // Validate maximum price (reasonable limit)
            if (Room.RoomPricePerMonth > 50000000) // 50 million VND per month
            {
                MessageBox.Show("Giá phòng không được vượt quá 50,000,000 VNĐ/tháng.", 
                              "Giá phòng quá cao", MessageBoxButton.OK, MessageBoxImage.Warning);
                RoomPriceTextBox.Focus();
                return false;
            }

            // Validate capacity
            if (Room.RoomMaxCapacity <= 0 || Room.RoomMaxCapacity > 10)
            {
                MessageBox.Show("Sức chứa phòng phải từ 1 đến 10 người.", 
                              "Sức chứa không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                CapacityComboBox.Focus();
                return false;
            }

            // Validate status
            if (Room.RoomStatus < 0 || Room.RoomStatus > 2)
            {
                MessageBox.Show("Vui lòng chọn trạng thái phòng hợp lệ.", 
                              "Trạng thái không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusComboBox.Focus();
                return false;
            }

            return true;
        }

        private bool HasRoomNumberChanged()
        {
            if (!_isEditMode) return false;

            try
            {
                var allRooms = _repository.GetAll();
                var originalRoom = allRooms.FirstOrDefault(r => r.RoomID == Room.RoomID);
                return originalRoom?.RoomNumber != Room.RoomNumber;
            }
            catch
            {
                return true; // Assume changed if can't check
            }
        }

        private bool IsRoomNumberExists(string roomNumber)
        {
            try
            {
                var allRooms = _repository.GetAll();
                return allRooms.Any(r => r.RoomNumber.Equals(roomNumber, StringComparison.OrdinalIgnoreCase) 
                                      && r.RoomID != Room.RoomID);
            }
            catch
            {
                return false; // Assume doesn't exist if can't check
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Room == null) return;

            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedValue != null)
            {
                // Update RoomStatus
                Room.RoomStatus = Convert.ToInt32(comboBox.SelectedValue);
                
                // Đồng bộ RoomStatusText theo logic nghiệp vụ
                switch (Room.RoomStatus)
                {
                    case 0:
                        Room.SetRoomStatus("Phòng trống");
                        break;
                    case 1:
                        Room.SetRoomStatus("Hoạt động");
                        break;
                    case 2:
                        Room.SetRoomStatus("Bảo trì");
                        break;
                }
            }
        }

        private void SyncRoomStatusText()
        {
            if (Room == null) return;
            
            // Đồng bộ RoomStatusText theo RoomStatus với format của Repository
            switch (Room.RoomStatus)
            {
                case 0:
                    Room.SetRoomStatus("⭕ Phòng trống");
                    break;
                case 1:
                    Room.SetRoomStatus("🟢 Hoạt động");
                    break;
                case 2:
                    Room.SetRoomStatus("🔧 Bảo trì");
                    break;
                default:
                    Room.SetRoomStatus("⭕ Phòng trống"); // Default
                    break;
            }
        }
    }
}