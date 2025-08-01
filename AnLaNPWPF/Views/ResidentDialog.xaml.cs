using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for ResidentDialog.xaml
    /// </summary>
    public partial class ResidentDialog : Window
    {
        private CustomerModel _resident;
        private CustomerRepository _customerRepository;
        private RoomInfomationRepository _roomRepository;

        public CustomerModel Resident 
        { 
            get { return _resident; } 
            set 
            { 
                _resident = value;
                DataContext = _resident;
                LoadAvailableRooms();
            } 
        }

        public bool IsSaved { get; private set; } = false;

        public ResidentDialog()
        {
            InitializeComponent();
            _customerRepository = new CustomerRepository();
            _roomRepository = new RoomInfomationRepository();
            
            // Initialize with empty resident
            Resident = new CustomerModel
            {
                CustomerBirthday = DateTime.Now.AddYears(-25),
                CheckInDate = DateTime.Now,
                Gender = "Nam",
                CustomerStatus = 1 // Active status
            };
        }

        public ResidentDialog(CustomerModel resident) : this()
        {
            Resident = resident;
            LoadImages();
        }

        private void LoadImages()
        {
            try
            {
                if (!string.IsNullOrEmpty(_resident?.CCCDFrontImagePath) && System.IO.File.Exists(_resident.CCCDFrontImagePath))
                {
                    DisplayImage(imgCCCDFront, _resident.CCCDFrontImagePath);
                }

                if (!string.IsNullOrEmpty(_resident?.CCCDBackImagePath) && System.IO.File.Exists(_resident.CCCDBackImagePath))
                {
                    DisplayImage(imgCCCDBack, _resident.CCCDBackImagePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadAvailableRooms()
        {
            try
            {
                var rooms = _roomRepository.GetAll();
                
                // Create a list that includes the current room (if any) plus available rooms
                var availableRooms = new List<RoomInformationModel>();
                
                // Add current room if exists
                if (_resident?.CurrentRoomID != null)
                {
                    var currentRoom = rooms.FirstOrDefault(r => r.RoomID == _resident.CurrentRoomID);
                    if (currentRoom != null)
                    {
                        availableRooms.Add(currentRoom);
                    }
                }
                
                // Add available rooms (status 0 = Sẵn sàng thuê, status 1 = Đang được thuê)
                var availableForRent = rooms.Where(r => (r.RoomStatus == 0 || r.RoomStatus == 1) && 
                    (r.RoomID != _resident?.CurrentRoomID))
                    .OrderBy(r => r.RoomStatus) // Ưu tiên phòng trống (status 0) trước
                    .ThenBy(r => r.RoomNumber) // Sau đó sắp xếp theo số phòng
                    .ToList();
                availableRooms.AddRange(availableForRent);

                // Set the AvailableRooms property on the resident model
                if (_resident != null)
                {
                    _resident.AvailableRooms = availableRooms;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách phòng: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(_resident.CustomerFullName))
                {
                    MessageBox.Show("Vui lòng nhập họ và tên!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_resident.CCCD))
                {
                    MessageBox.Show("Vui lòng nhập số CCCD!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_resident.CurrentRoomID == null)
                {
                    MessageBox.Show("Vui lòng chọn phòng!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Set the current room number for display
                var selectedRoom = _resident.AvailableRooms?.FirstOrDefault(r => r.RoomID == _resident.CurrentRoomID);
                if (selectedRoom != null)
                {
                    _resident.CurrentRoomNumber = selectedRoom.RoomNumber;
                }

                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            DialogResult = false;
            Close();
        }

        #region CCCD Photo Methods

        private void btnCaptureFront_Click(object sender, RoutedEventArgs e)
        {
            CapturePhoto(true);
        }

        private void btnCaptureBack_Click(object sender, RoutedEventArgs e)
        {
            CapturePhoto(false);
        }

        private void btnSelectFront_Click(object sender, RoutedEventArgs e)
        {
            SelectImageFile(true);
        }

        private void btnSelectBack_Click(object sender, RoutedEventArgs e)
        {
            SelectImageFile(false);
        }

        private void btnRemoveFront_Click(object sender, RoutedEventArgs e)
        {
            RemoveImage(true);
        }

        private void btnRemoveBack_Click(object sender, RoutedEventArgs e)
        {
            RemoveImage(false);
        }

        private void CapturePhoto(bool isFront)
        {
            try
            {
                // Tạo thư mục lưu ảnh nếu chưa có
                string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MotelManagement", "Images");
                System.IO.Directory.CreateDirectory(appDataPath);

                // Tạo tên file duy nhất
                string fileName = $"CCCD_{(isFront ? "Front" : "Back")}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                string filePath = System.IO.Path.Combine(appDataPath, fileName);

                // Mở camera (simplified - trong thực tế cần thư viện camera)
                MessageBox.Show("Chức năng chụp ảnh sẽ được triển khai với thư viện camera.\nHiện tại vui lòng sử dụng 'Chọn' để chọn ảnh từ file.", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Fallback to file selection
                SelectImageFile(isFront);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chụp ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectImageFile(bool isFront)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = $"Chọn ảnh CCCD {(isFront ? "mặt trước" : "mặt sau")}",
                    Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;

                    // Tạo thư mục lưu ảnh
                    string appDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MotelManagement", "Images");
                    System.IO.Directory.CreateDirectory(appDataPath);

                    // Copy file to app directory
                    string fileName = $"CCCD_{(isFront ? "Front" : "Back")}_{DateTime.Now:yyyyMMdd_HHmmss}{System.IO.Path.GetExtension(selectedFilePath)}";
                    string destinationPath = System.IO.Path.Combine(appDataPath, fileName);

                    System.IO.File.Copy(selectedFilePath, destinationPath, true);

                    // Update model and display
                    if (isFront)
                    {
                        _resident.CCCDFrontImagePath = destinationPath;
                        DisplayImage(imgCCCDFront, destinationPath);
                    }
                    else
                    {
                        _resident.CCCDBackImagePath = destinationPath;
                        DisplayImage(imgCCCDBack, destinationPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chọn ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveImage(bool isFront)
        {
            try
            {
                if (isFront)
                {
                    _resident.CCCDFrontImagePath = null;
                    imgCCCDFront.Source = null;
                }
                else
                {
                    _resident.CCCDBackImagePath = null;
                    imgCCCDBack.Source = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayImage(Image imageControl, string imagePath)
        {
            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imageControl.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi hiển thị ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion
    }
}
