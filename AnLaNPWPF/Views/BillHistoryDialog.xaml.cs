using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.Views
{
    public partial class BillHistoryDialog : Window
    {
        private MonthlyBillRepository _billRepository;
        private RentalContractRepository _contractRepository;
        private List<MonthlyBill> _allBills;
        private List<MonthlyBill> _filteredBills;
        private RentalContract _filterContract; // For filtering by specific contract

        public BillHistoryDialog()
        {
            InitializeComponent();
            InitializeDialog();
        }

        public BillHistoryDialog(RentalContract contract)
        {
            InitializeComponent();
            _filterContract = contract;
            
            // Update title when filtering by contract
            if (contract != null)
            {
                this.Title = $"Lịch Sử Hóa Đơn - Hợp đồng {contract.ContractNumber}";
            }
            
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            try
            {
                _billRepository = new MonthlyBillRepository();
                _contractRepository = new RentalContractRepository();
                LoadBills();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo quản lý hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                _allBills = new List<MonthlyBill>();
                _filteredBills = new List<MonthlyBill>();
                dgBills.ItemsSource = _filteredBills;
            }
        }

        private void LoadBills()
        {
            try
            {
                _allBills = _billRepository.GetAll();
                
                // Filter by specific contract if provided
                if (_filterContract != null)
                {
                    _allBills = _allBills.Where(b => b.ContractId == _filterContract.ContractId).ToList();
                }
                
                // Load contract details for each bill
                foreach (var bill in _allBills)
                {
                    if (bill.Contract == null)
                    {
                        try
                        {
                            bill.Contract = _contractRepository.GetById(bill.ContractId);
                        }
                        catch (Exception)
                        {
                            // If we can't load contract, create a minimal one to prevent UI errors
                            // We can't set readonly properties, so just leave it null
                            // The UI will handle null gracefully
                        }
                    }
                }
                
                _filteredBills = new List<MonthlyBill>(_allBills);
                dgBills.ItemsSource = _filteredBills;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Initialize empty collections to prevent crashes
                _allBills = new List<MonthlyBill>();
                _filteredBills = new List<MonthlyBill>();
                dgBills.ItemsSource = _filteredBills;
            }
        }

        private void UpdateSummary()
        {
            if (_filteredBills == null) return;

            var totalBills = _filteredBills.Count;
            var totalRevenue = _filteredBills.Where(b => b.BillStatus == BillStatus.Paid)
                                           .Sum(b => b.TotalAmount);
            
            var pendingCount = _filteredBills.Count(b => b.BillStatus == BillStatus.Pending);
            var paidCount = _filteredBills.Count(b => b.BillStatus == BillStatus.Paid);
            var overdueCount = _filteredBills.Count(b => b.BillStatus == BillStatus.Overdue);
            var partialCount = _filteredBills.Count(b => b.BillStatus == BillStatus.Partial);

            txtBillSummary.Text = $"Tổng: {totalBills} hóa đơn | " +
                                 $"Chờ: {pendingCount} | " +
                                 $"Đã TT: {paidCount} | " +
                                 $"Quá hạn: {overdueCount} | " +
                                 $"Một phần: {partialCount}";
                                 
            txtRevenueSummary.Text = $"Doanh thu: {totalRevenue:C0}";
        }

        private void ApplyFilters()
        {
            if (_allBills == null) return;

            var filtered = _allBills.AsEnumerable();

            // Filter by status
            var selectedStatus = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "All")
            {
                if (Enum.TryParse<BillStatus>(selectedStatus, out var status))
                {
                    filtered = filtered.Where(b => b.BillStatus == status);
                }
            }

            // Filter by month
            var selectedMonth = (cmbMonthFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (!string.IsNullOrEmpty(selectedMonth) && selectedMonth != "All")
            {
                if (int.TryParse(selectedMonth, out var month))
                {
                    filtered = filtered.Where(b => b.BillMonth == month);
                }
            }

            // Filter by year
            var selectedYear = (cmbYearFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (!string.IsNullOrEmpty(selectedYear) && selectedYear != "All")
            {
                if (int.TryParse(selectedYear, out var year))
                {
                    filtered = filtered.Where(b => b.BillYear == year);
                }
            }

            _filteredBills = filtered.ToList();
            dgBills.ItemsSource = _filteredBills;
            UpdateSummary();
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allBills != null) ApplyFilters();
        }

        private void MonthFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allBills != null) ApplyFilters();
        }

        private void YearFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_allBills != null) ApplyFilters();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBills();
        }

        private void UpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var bill = button?.Tag as MonthlyBill;
                
                if (bill == null) return;

                var statusDialog = new BillStatusUpdateDialog(bill);
                if (statusDialog.ShowDialog() == true && statusDialog.IsUpdated)
                {
                    if (_billRepository.Update(statusDialog.Bill))
                    {
                        LoadBills(); // Refresh to show updated status
                        MessageBox.Show("Cập nhật trạng thái hóa đơn thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể cập nhật trạng thái hóa đơn!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var bill = button?.Tag as MonthlyBill;
                
                if (bill == null) return;

                var detailDialog = new BillDetailDialog(bill);
                detailDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xem chi tiết hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintBill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var bill = button?.Tag as MonthlyBill;
                
                if (bill == null) return;

                // TODO: Implement print functionality
                MessageBox.Show($"Chức năng in hóa đơn #{bill.BillId} sẽ được triển khai sau.", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Implement Excel export functionality
                MessageBox.Show("Chức năng xuất Excel sẽ được triển khai sau.", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
