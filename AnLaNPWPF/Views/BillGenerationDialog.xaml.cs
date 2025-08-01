using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.Views
{
    public partial class BillGenerationDialog : Window
    {
        private RentalContractRepository _contractRepository;
        private MonthlyBillRepository _billRepository;
        private List<BillGenerationItem> _billItems;

        public bool IsGenerated { get; private set; } = false;
        public int BillsCreated { get; private set; } = 0;

        public BillGenerationDialog()
        {
            InitializeComponent();
            InitializeRepositories();
            InitializeCurrentMonth();
        }

        private void InitializeRepositories()
        {
            _contractRepository = new RentalContractRepository();
            _billRepository = new MonthlyBillRepository();
        }

        private void InitializeCurrentMonth()
        {
            var currentDate = DateTime.Now;
            txtYear.Text = currentDate.Year.ToString();
            
            // Select current month
            foreach (ComboBoxItem item in cmbMonth.Items)
            {
                if (item.Tag.ToString() == currentDate.Month.ToString())
                {
                    cmbMonth.SelectedItem = item;
                    break;
                }
            }
        }

        private void LoadContracts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbMonth.SelectedItem == null || string.IsNullOrWhiteSpace(txtYear.Text))
                {
                    MessageBox.Show("Vui lòng chọn tháng và năm!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var month = int.Parse(((ComboBoxItem)cmbMonth.SelectedItem).Tag.ToString());
                var year = int.Parse(txtYear.Text);

                // Load active contracts
                var activeContracts = _contractRepository.GetActiveContracts();
                
                if (activeContracts.Count == 0)
                {
                    MessageBox.Show("Không có hợp đồng đang hiệu lực!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create bill generation items with default values
                _billItems = activeContracts.Select(contract => new BillGenerationItem
                {
                    Contract = contract,
                    ContractNumber = contract.ContractNumber,
                    CustomerName = contract.CustomerName,
                    RoomNumber = contract.RoomNumber,
                    MonthlyRent = contract.MonthlyRent,
                    
                    // Default values - user will input actual readings
                    PreviousElectricReading = 0,
                    CurrentElectricReading = 0,
                    ElectricRate = 3500, // Default rate
                    
                    PreviousWaterReading = 0,
                    CurrentWaterReading = 0,
                    WaterRate = 15000, // Default rate per m³
                    
                    OtherFees = 50000, // Default other fees (garbage, internet, etc.)
                    
                    Month = month,
                    Year = year
                }).ToList();

                dgContracts.ItemsSource = _billItems;
                
                txtSubtitle.Text = $"Tìm thấy {_billItems.Count} hợp đồng cho tháng {month:D2}/{year}. Hãy nhập chỉ số điện nước.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateAll_Click(object sender, RoutedEventArgs e)
        {
            if (_billItems == null || _billItems.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu hợp đồng. Vui lòng tải danh sách trước!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                foreach (var item in _billItems)
                {
                    item.CalculateTotalAmount();
                }
                
                // Refresh DataGrid
                dgContracts.Items.Refresh();
                
                var totalAmount = _billItems.Sum(i => i.TotalAmount);
                txtSubtitle.Text = $"Đã tính toán xong. Tổng tiền tất cả hóa đơn: {totalAmount:N0} đ";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tính toán: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateBills_Click(object sender, RoutedEventArgs e)
        {
            if (_billItems == null || _billItems.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu hợp đồng!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate data
            var errors = new List<string>();
            foreach (var item in _billItems)
            {
                if (item.CurrentElectricReading < item.PreviousElectricReading)
                    errors.Add($"Phòng {item.RoomNumber}: Chỉ số điện mới phải lớn hơn chỉ số cũ");
                
                if (item.CurrentWaterReading < item.PreviousWaterReading)
                    errors.Add($"Phòng {item.RoomNumber}: Chỉ số nước mới phải lớn hơn chỉ số cũ");
                
                if (item.ElectricRate <= 0)
                    errors.Add($"Phòng {item.RoomNumber}: Giá điện phải lớn hơn 0");
                
                if (item.WaterRate <= 0)
                    errors.Add($"Phòng {item.RoomNumber}: Giá nước phải lớn hơn 0");
            }

            if (errors.Count > 0)
            {
                MessageBox.Show($"Có lỗi trong dữ liệu:\n\n{string.Join("\n", errors)}", "Lỗi dữ liệu", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Tạo {_billItems.Count} hóa đơn cho tháng {_billItems.First().Month:D2}/{_billItems.First().Year}?\n\n" +
                $"Tổng tiền: {_billItems.Sum(i => i.TotalAmount):N0} đ\n\n" +
                "Lưu ý: Hóa đơn đã tồn tại sẽ không được tạo lại.",
                "Xác nhận tạo hóa đơn", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    BillsCreated = 0;
                    
                    foreach (var item in _billItems)
                    {
                        // Recalculate to ensure accuracy
                        item.CalculateTotalAmount();
                        
                        // Create monthly bill
                        var bill = new MonthlyBill
                        {
                            ContractId = item.Contract.ContractId,
                            BillMonth = item.Month,
                            BillYear = item.Year,
                            RoomRent = item.MonthlyRent,
                            
                            ElectricityUsage = item.CurrentElectricReading - item.PreviousElectricReading,
                            ElectricityRate = item.ElectricRate,
                            ElectricityCost = (item.CurrentElectricReading - item.PreviousElectricReading) * item.ElectricRate,
                            
                            WaterUsage = item.CurrentWaterReading - item.PreviousWaterReading,
                            WaterRate = item.WaterRate,
                            WaterCost = (item.CurrentWaterReading - item.PreviousWaterReading) * item.WaterRate,
                            
                            OtherCharges = item.OtherFees,
                            TotalAmount = item.TotalAmount,
                            
                            BillStatus = BillStatus.Pending,
                            DueDate = DateTime.Today.AddDays(30),
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        if (_billRepository.Add(bill))
                        {
                            BillsCreated++;
                        }
                    }

                    IsGenerated = true;
                    MessageBox.Show($"Đã tạo thành công {BillsCreated}/{_billItems.Count} hóa đơn!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tạo hóa đơn: {ex.Message}", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsGenerated = false;
            DialogResult = false;
            Close();
        }
    }

    // Helper class for DataGrid binding
    public class BillGenerationItem
    {
        public RentalContract Contract { get; set; }
        public string ContractNumber { get; set; }
        public string CustomerName { get; set; }
        public string RoomNumber { get; set; }
        public decimal MonthlyRent { get; set; }
        
        public decimal PreviousElectricReading { get; set; }
        public decimal CurrentElectricReading { get; set; }
        public decimal ElectricRate { get; set; }
        
        public decimal PreviousWaterReading { get; set; }
        public decimal CurrentWaterReading { get; set; }
        public decimal WaterRate { get; set; }
        
        public decimal OtherFees { get; set; }
        public decimal TotalAmount { get; set; }
        
        public int Month { get; set; }
        public int Year { get; set; }

        public void CalculateTotalAmount()
        {
            var electricAmount = (CurrentElectricReading - PreviousElectricReading) * ElectricRate;
            var waterAmount = (CurrentWaterReading - PreviousWaterReading) * WaterRate;
            TotalAmount = MonthlyRent + electricAmount + waterAmount + OtherFees;
        }
    }
}
