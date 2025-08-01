using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.Views
{
    public partial class BillGenerationView : UserControl
    {
        private RentalContractRepository _contractRepository;
        private MonthlyBillRepository _billRepository;
        private DispatcherTimer _timer;
        private List<ContractBillItem> _contracts;

        public event EventHandler BackRequested;
        public bool IsGenerated { get; private set; }
        public int BillsCreated { get; private set; }

        public BillGenerationView()
        {
            InitializeComponent();
            
            try
            {
                _contractRepository = new RentalContractRepository();
                _billRepository = new MonthlyBillRepository();
                
                InitializeControls();
                StartTimeDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo trang tạo hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Lỗi khởi tạo";
            }
        }

        private void InitializeControls()
        {
            // Initialize month combobox
            for (int i = 1; i <= 12; i++)
            {
                cmbMonth.Items.Add(new ComboBoxItem { Content = $"Tháng {i}", Tag = i });
            }

            // Initialize year combobox
            var currentYear = DateTime.Now.Year;
            for (int year = currentYear - 1; year <= currentYear + 1; year++)
            {
                cmbYear.Items.Add(new ComboBoxItem { Content = year.ToString(), Tag = year });
            }

            // Set current month/year as default
            var currentMonth = DateTime.Now.Month;
            cmbMonth.SelectedIndex = currentMonth - 1;
            cmbYear.SelectedIndex = 1; // Current year

            // Set default due date (end of current month + 5 days)
            var nextMonth = DateTime.Now.AddMonths(1);
            var defaultDueDate = new DateTime(nextMonth.Year, nextMonth.Month, 5);
            dpDueDate.SelectedDate = defaultDueDate;

            txtStatus.Text = "Chọn tháng và nhấn 'Tải HĐ' để bắt đầu";
        }

        private void StartTimeDisplay()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            Timer_Tick(null, null); // Update immediately
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            txtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LoadContracts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbMonth.SelectedItem == null || cmbYear.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn tháng và năm!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpDueDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Vui lòng chọn ngày đến hạn!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var month = (int)((ComboBoxItem)cmbMonth.SelectedItem).Tag;
                var year = (int)((ComboBoxItem)cmbYear.SelectedItem).Tag;
                var dueDate = dpDueDate.SelectedDate.Value;

                txtStatus.Text = "Đang tải hợp đồng...";

                LoadContractsForBilling(month, year, dueDate);

                txtStatus.Text = "Tải hợp đồng hoàn tất";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Lỗi tải hợp đồng";
            }
        }

        private void LoadContractsForBilling(int month, int year, DateTime dueDate)
        {
            var activeContracts = _contractRepository.GetActiveContracts();
            var existingBills = _billRepository.GetBillsByMonthYear(month, year);
            
            _contracts = new List<ContractBillItem>();

            foreach (var contract in activeContracts)
            {
                var existingBill = existingBills.FirstOrDefault(b => b.ContractId == contract.ContractId);
                bool hasExistingBill = existingBill != null;

                var item = new ContractBillItem
                {
                    ContractId = contract.ContractId,
                    RoomNumber = contract.RoomNumber,
                    CustomerName = contract.CustomerName,
                    RoomRent = contract.MonthlyRent,
                    FormattedRoomRent = contract.MonthlyRent.ToString("N0") + " VND",
                    ElectricityUsage = hasExistingBill ? existingBill.ElectricityUsage : 0,
                    ElectricityRate = hasExistingBill ? existingBill.ElectricityRate : 3500,
                    WaterUsage = hasExistingBill ? existingBill.WaterUsage : 0,
                    WaterRate = hasExistingBill ? existingBill.WaterRate : 25000,
                    OtherCharges = hasExistingBill ? existingBill.OtherCharges : 0,
                    DueDate = dueDate,
                    BillMonth = month,
                    BillYear = year,
                    HasExistingBill = hasExistingBill,
                    ExistingBillId = hasExistingBill ? existingBill.BillId : (int?)null,
                    IsSelected = !hasExistingBill // Auto-select only new bills
                };

                item.CalculateTotal();
                _contracts.Add(item);
            }

            var dataGrid = this.FindName("dgContracts") as DataGrid;
            if (dataGrid != null)
            {
                dataGrid.ItemsSource = _contracts;
            }
            UpdateSummary();
            UpdateContractCount();
        }

        private void UpdateSummary()
        {
            if (_contracts == null) return;

            var totalContracts = _contracts.Count;
            var newBills = _contracts.Count(c => !c.HasExistingBill);
            var existingBills = _contracts.Count(c => c.HasExistingBill);
            var selectedCount = _contracts.Count(c => c.IsSelected);
            var totalAmount = _contracts.Where(c => c.IsSelected).Sum(c => c.TotalAmount);

            var txtSummary = this.FindName("txtSummary") as TextBlock;
            if (txtSummary != null)
            {
                txtSummary.Text = $"Tổng {totalContracts} hợp đồng | Mới: {newBills} | Đã tồn tại: {existingBills} | " +
                                 $"Đã chọn: {selectedCount} | Tổng tiền: {totalAmount:N0} VND";
            }
        }

        private void UpdateContractCount()
        {
            var selectedCount = _contracts?.Count(c => c.IsSelected) ?? 0;
            var totalCount = _contracts?.Count ?? 0;
            
            var txtContractCount = this.FindName("txtContractCount") as TextBlock;
            if (txtContractCount != null)
            {
                txtContractCount.Text = $"Đã chọn: {selectedCount}/{totalCount} hợp đồng";
            }
        }

        private void GenerateAll_Click(object sender, RoutedEventArgs e)
        {
            if (_contracts == null || !_contracts.Any())
            {
                MessageBox.Show("Chưa có hợp đồng nào để tạo hóa đơn!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Select all eligible contracts (those without existing bills)
            foreach (var contract in _contracts.Where(c => !c.HasExistingBill))
            {
                contract.IsSelected = true;
            }

            var dataGrid = this.FindName("dgContracts") as DataGrid;
            if (dataGrid != null)
            {
                dataGrid.Items.Refresh();
            }
            UpdateSummary();
            UpdateContractCount();

            GenerateBills();
        }

        private void GenerateSelected_Click(object sender, RoutedEventArgs e)
        {
            GenerateBills();
        }

        private void GenerateBills()
        {
            try
            {
                var selectedContracts = _contracts?.Where(c => c.IsSelected && !c.HasExistingBill).ToList();
                
                if (selectedContracts == null || !selectedContracts.Any())
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một hợp đồng để tạo hóa đơn!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn tạo {selectedContracts.Count} hóa đơn?\n\n" +
                    $"Tổng giá trị: {selectedContracts.Sum(c => c.TotalAmount):N0} VND",
                    "Xác nhận tạo hóa đơn", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                var txtStatus = this.FindName("txtStatus") as TextBlock;
                if (txtStatus != null)
                {
                    txtStatus.Text = "Đang tạo hóa đơn...";
                }
                BillsCreated = 0;

                foreach (var contract in selectedContracts)
                {
                    // Recalculate total based on current values in DataGrid
                    var electricityCost = contract.ElectricityUsage * contract.ElectricityRate;
                    var waterCost = contract.WaterUsage * contract.WaterRate;
                    var totalAmount = contract.RoomRent + electricityCost + waterCost + contract.OtherCharges;
                    
                    var bill = new MonthlyBill
                    {
                        ContractId = contract.ContractId,
                        BillMonth = contract.BillMonth,
                        BillYear = contract.BillYear,
                        RoomRent = contract.RoomRent,
                        ElectricityUsage = contract.ElectricityUsage,
                        ElectricityRate = contract.ElectricityRate,
                        ElectricityCost = electricityCost,
                        WaterUsage = contract.WaterUsage,
                        WaterRate = contract.WaterRate,
                        WaterCost = waterCost,
                        OtherCharges = contract.OtherCharges,
                        TotalAmount = totalAmount,
                        BillStatus = BillStatus.Pending,
                        DueDate = contract.DueDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    if (_billRepository.Add(bill))
                    {
                        BillsCreated++;
                        contract.HasExistingBill = true;
                        contract.IsSelected = false;
                        contract.StatusText = "Đã tạo";
                    }
                    else
                    {
                        contract.StatusText = "Lỗi";
                    }
                }

                var dataGrid = this.FindName("dgContracts") as DataGrid;
                if (dataGrid != null)
                {
                    dataGrid.Items.Refresh();
                }
                UpdateSummary();
                UpdateContractCount();

                IsGenerated = BillsCreated > 0;
                
                if (txtStatus != null)
                {
                    txtStatus.Text = $"Đã tạo {BillsCreated}/{selectedContracts.Count} hóa đơn";
                }

                MessageBox.Show($"Đã tạo thành công {BillsCreated} hóa đơn!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                var txtStatus = this.FindName("txtStatus") as TextBlock;
                if (txtStatus != null)
                {
                    txtStatus.Text = "Lỗi tạo hóa đơn";
                }
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
        }
    }

    // Helper class for DataGrid binding
    public class ContractBillItem : INotifyPropertyChanged
    {
        public int ContractId { get; set; }
        public string RoomNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal RoomRent { get; set; }
        public string FormattedRoomRent { get; set; }
        
        private decimal _electricityUsage;
        public decimal ElectricityUsage 
        { 
            get => _electricityUsage;
            set 
            { 
                _electricityUsage = value;
                OnPropertyChanged(nameof(ElectricityUsage));
                CalculateTotal();
            }
        }
        
        public decimal ElectricityRate { get; set; }
        
        private decimal _waterUsage;
        public decimal WaterUsage 
        { 
            get => _waterUsage;
            set 
            { 
                _waterUsage = value;
                OnPropertyChanged(nameof(WaterUsage));
                CalculateTotal();
            }
        }
        
        public decimal WaterRate { get; set; }
        
        private decimal _otherCharges;
        public decimal OtherCharges 
        { 
            get => _otherCharges;
            set 
            { 
                _otherCharges = value;
                OnPropertyChanged(nameof(OtherCharges));
                CalculateTotal();
            }
        }
        
        private decimal _totalAmount;
        public decimal TotalAmount 
        { 
            get => _totalAmount;
            set 
            { 
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(FormattedTotal));
            }
        }
        
        public string FormattedTotal => TotalAmount.ToString("N0") + " VND";
        public DateTime DueDate { get; set; }
        public int BillMonth { get; set; }
        public int BillYear { get; set; }
        public bool HasExistingBill { get; set; }
        public int? ExistingBillId { get; set; }
        public bool IsSelected { get; set; }
        public string StatusText { get; set; } = "Chưa tạo";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void CalculateTotal()
        {
            var electricityCost = ElectricityUsage * ElectricityRate;
            var waterCost = WaterUsage * WaterRate;
            TotalAmount = RoomRent + electricityCost + waterCost + OtherCharges;
            
            if (HasExistingBill)
            {
                StatusText = "Đã tồn tại";
            }
        }
    }
}
