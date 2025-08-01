using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.Views
{
    public partial class ContractDialog : Window
    {
        private RentalContract _contract;
        private CustomerRepository _customerRepository;
        private RoomInfomationRepository _roomRepository;
        private RentalContractRepository _contractRepository;

        public RentalContract Contract
        {
            get { return _contract; }
            set
            {
                _contract = value;
                DataContext = _contract;
                LoadContractData();
            }
        }

        public bool IsSaved { get; private set; } = false;

        public ContractDialog()
        {
            InitializeComponent();
            InitializeRepositories();
            InitializeNewContract();
            LoadComboBoxData();
        }

        public ContractDialog(RentalContract contract) : this()
        {
            Contract = contract;
            txtTitle.Text = "📝 Chỉnh Sửa Hợp Đồng";
            txtSubtitle.Text = $"Cập nhật thông tin hợp đồng {contract.ContractNumber}";
        }

        private void InitializeRepositories()
        {
            _customerRepository = new CustomerRepository();
            _roomRepository = new RoomInfomationRepository();
            _contractRepository = new RentalContractRepository();
        }

        private void InitializeNewContract()
        {
            if (_contract == null)
            {
                Contract = new RentalContract();
                // Auto-generate contract number
                _contract.ContractNumber = GenerateNewContractNumber();
            }
        }

        private string GenerateNewContractNumber()
        {
            try
            {
                // Get current date for contract number format: HD-YYYYMM-XXX
                var currentDate = DateTime.Now;
                var yearMonth = currentDate.ToString("yyyyMM");
                
                // Get existing contracts for this month to determine next number
                var existingContracts = _contractRepository.GetAll()
                    .Where(c => c.ContractNumber.StartsWith($"HD-{yearMonth}"))
                    .ToList();
                
                var nextNumber = existingContracts.Count + 1;
                
                return $"HD-{yearMonth}-{nextNumber:D3}";
            }
            catch (Exception)
            {
                // Fallback: use simple timestamp-based number
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
                return $"HD-{timestamp}";
            }
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Load customers
                var customers = _customerRepository.GetAll();
                cmbCustomer.ItemsSource = customers;

                // Load available rooms
                var rooms = _roomRepository.GetAll()
                    .Where(r => r.RoomStatus == 0 || r.RoomStatus == 1) // Available or Occupied
                    .OrderBy(r => r.RoomNumber)
                    .ToList();
                cmbRoom.ItemsSource = rooms;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadContractData()
        {
            if (_contract == null) return;

            txtContractNumber.Text = _contract.ContractNumber;
            dpStartDate.SelectedDate = _contract.StartDate;
            dpEndDate.SelectedDate = _contract.EndDate;
            txtMonthlyRent.Text = _contract.MonthlyRent.ToString("0");
            txtSecurityDeposit.Text = _contract.SecurityDeposit.ToString("0");
            txtContractTerms.Text = _contract.ContractTerms;

            // Set status
            foreach (ComboBoxItem item in cmbContractStatus.Items)
            {
                if (item.Tag.ToString() == _contract.ContractStatus.ToString())
                {
                    cmbContractStatus.SelectedItem = item;
                    break;
                }
            }

            // Set duration based on contract period
            var duration = _contract.ContractDurationMonths;
            foreach (ComboBoxItem item in cmbContractDuration.Items)
            {
                if (int.Parse(item.Tag.ToString()) == duration)
                {
                    cmbContractDuration.SelectedItem = item;
                    break;
                }
            }

            // Set selected customer and room
            cmbCustomer.SelectedValue = _contract.CustomerId;
            cmbRoom.SelectedValue = _contract.RoomId;

            UpdateTotalValue();
        }

        private void StartDate_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateEndDate();
        }

        private void Duration_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateEndDate();
        }

        private void UpdateEndDate()
        {
            if (dpStartDate.SelectedDate.HasValue && cmbContractDuration.SelectedItem != null)
            {
                var startDate = dpStartDate.SelectedDate.Value;
                var duration = int.Parse(((ComboBoxItem)cmbContractDuration.SelectedItem).Tag.ToString());
                dpEndDate.SelectedDate = startDate.AddMonths(duration);
            }
        }

        private void Room_Changed(object sender, SelectionChangedEventArgs e)
        {
            var selectedRoom = cmbRoom.SelectedItem as RoomInformationModel;
            if (selectedRoom != null)
            {
                txtRoomInfo.Text = $"Phòng {selectedRoom.RoomNumber} - {selectedRoom.RoomTypes?.RoomTypeName ?? "N/A"} - " +
                                  $"Giá: {selectedRoom.RoomPricePerMonth:C0} - " +
                                  $"Trạng thái: {selectedRoom.RoomStatusText}";
                
                // Auto-fill monthly rent if not set
                if (string.IsNullOrEmpty(txtMonthlyRent.Text) || txtMonthlyRent.Text == "0")
                {
                    txtMonthlyRent.Text = selectedRoom.RoomPricePerMonth.ToString("0");
                    UpdateTotalValue();
                }
            }
            else
            {
                txtRoomInfo.Text = "Chọn phòng để xem thông tin";
            }
        }

        private void MonthlyRent_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateTotalValue();
        }

        private void UpdateTotalValue()
        {
            try
            {
                if (decimal.TryParse(txtMonthlyRent.Text, out var monthlyRent) && 
                    cmbContractDuration.SelectedItem != null)
                {
                    var duration = int.Parse(((ComboBoxItem)cmbContractDuration.SelectedItem).Tag.ToString());
                    var totalValue = monthlyRent * duration;
                    
                    txtTotalValue.Text = totalValue.ToString("C0", CultureInfo.GetCultureInfo("vi-VN"));
                }
                else
                {
                    txtTotalValue.Text = "0 VND";
                }
            }
            catch
            {
                txtTotalValue.Text = "0 VND";
            }
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var customerDialog = new ResidentDialog();
                if (customerDialog.ShowDialog() == true && customerDialog.IsSaved)
                {
                    if (_customerRepository.Add(customerDialog.Resident))
                    {
                        LoadComboBoxData(); // Reload customers
                        cmbCustomer.SelectedValue = customerDialog.Resident.CustomerID;
                        MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể thêm khách hàng!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm khách hàng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                {
                    return;
                }

                // Update contract properties
                _contract.CustomerId = (int)cmbCustomer.SelectedValue;
                _contract.RoomId = (int)cmbRoom.SelectedValue;
                _contract.ContractNumber = txtContractNumber.Text;
                _contract.StartDate = dpStartDate.SelectedDate.Value;
                _contract.EndDate = dpEndDate.SelectedDate.Value;
                _contract.MonthlyRent = decimal.Parse(txtMonthlyRent.Text);
                _contract.SecurityDeposit = decimal.Parse(txtSecurityDeposit.Text);
                _contract.ContractTerms = txtContractTerms.Text;

                // Set status
                var selectedStatus = ((ComboBoxItem)cmbContractStatus.SelectedItem).Tag.ToString();
                _contract.ContractStatus = (ContractStatus)Enum.Parse(typeof(ContractStatus), selectedStatus);

                // Set navigation properties for display
                _contract.Customer = cmbCustomer.SelectedItem as CustomerModel;
                _contract.Room = cmbRoom.SelectedItem as RoomInformationModel;

                // Validate room availability for new contracts or when changing room
                if (_contract.ContractId == 0 || HasRoomChanged())
                {
                    int? contractIdParam = _contract.ContractId == 0 ? (int?)null : _contract.ContractId;
                    if (!_contractRepository.IsRoomAvailable(_contract.RoomId, _contract.StartDate, 
                        _contract.EndDate, contractIdParam))
                    {
                        MessageBox.Show("Phòng đã được thuê trong khoảng thời gian này!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool HasRoomChanged()
        {
            var originalContract = _contractRepository.GetById(_contract.ContractId);
            return originalContract != null && originalContract.RoomId != _contract.RoomId;
        }

        private bool ValidateInput()
        {
            var errors = new List<string>();

            if (cmbCustomer.SelectedValue == null)
                errors.Add("Vui lòng chọn khách hàng");

            if (cmbRoom.SelectedValue == null)
                errors.Add("Vui lòng chọn phòng");

            if (!dpStartDate.SelectedDate.HasValue)
                errors.Add("Vui lòng chọn ngày bắt đầu");

            if (!dpEndDate.SelectedDate.HasValue)
                errors.Add("Ngày kết thúc không hợp lệ");

            if (string.IsNullOrWhiteSpace(txtMonthlyRent.Text) || 
                !decimal.TryParse(txtMonthlyRent.Text, out var monthlyRent) || monthlyRent <= 0)
                errors.Add("Vui lòng nhập tiền thuê hợp lệ");

            if (string.IsNullOrWhiteSpace(txtSecurityDeposit.Text) || 
                !decimal.TryParse(txtSecurityDeposit.Text, out var deposit) || deposit < 0)
                errors.Add("Vui lòng nhập tiền cọc hợp lệ");

            if (cmbContractStatus.SelectedItem == null)
                errors.Add("Vui lòng chọn trạng thái hợp đồng");

            if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue &&
                dpEndDate.SelectedDate.Value <= dpStartDate.SelectedDate.Value)
                errors.Add("Ngày kết thúc phải sau ngày bắt đầu");

            if (errors.Count > 0)
            {
                txtValidationMessage.Text = string.Join(", ", errors);
                return false;
            }

            txtValidationMessage.Text = "Thông tin hợp lệ";
            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            DialogResult = false;
            Close();
        }
    }
}
