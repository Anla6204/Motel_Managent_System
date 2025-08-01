using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.Views
{
    public partial class BillHistoryView : UserControl
    {
        private MonthlyBillRepository _billRepository;
        private RentalContractRepository _contractRepository;
        private List<MonthlyBill> _allBills;
        private List<MonthlyBill> _filteredBills;

        public event EventHandler ShowBillGeneration;

        public BillHistoryView()
        {
            InitializeComponent();
            
            try
            {
                _billRepository = new MonthlyBillRepository();
                _contractRepository = new RentalContractRepository();
                InitializeFilters();
                LoadBills();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo Bill History: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeFilters()
        {
            try
            {
                // Initialize year filter
                var currentYear = DateTime.Now.Year;
                var cmbYearFilter = this.FindName("cmbYearFilter") as ComboBox;
                if (cmbYearFilter != null)
                {
                    for (int year = currentYear - 2; year <= currentYear + 1; year++)
                    {
                        var item = new ComboBoxItem { Content = year.ToString(), Tag = year };
                        if (year == currentYear)
                            item.IsSelected = true;
                        cmbYearFilter.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing filters: {ex.Message}");
            }
        }

        private void LoadBills()
        {
            try
            {
                _allBills = _billRepository.GetAll();
                
                // Load contract information for bills that don't have it
                foreach (var bill in _allBills)
                {
                    if (bill.Contract == null && bill.ContractId > 0)
                    {
                        try
                        {
                            bill.Contract = _contractRepository.GetById(bill.ContractId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Could not load contract {bill.ContractId}: {ex.Message}");
                        }
                    }
                }
                
                // Apply initial filters
                ApplyFilters();
                
                UpdateSummary();
                UpdateStatusInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách hóa đơn: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummary()
        {
            if (_filteredBills == null) return;

            var totalAmount = _filteredBills.Sum(b => b.TotalAmount);
            var paidAmount = _filteredBills.Where(b => b.BillStatus == BillStatus.Paid)
                                    .Sum(b => b.PaidAmount ?? b.TotalAmount);
            var pendingCount = _filteredBills.Count(b => b.BillStatus == BillStatus.Pending);
            var overdueCount = _filteredBills.Count(b => b.BillStatus == BillStatus.Overdue || 
                                                   (b.BillStatus == BillStatus.Pending && b.DueDate < DateTime.Today));

            // Summary will be displayed in status bar or console for now
            Console.WriteLine($"Hiển thị {_filteredBills.Count}/{_allBills?.Count ?? 0} hóa đơn | Tổng giá trị: {totalAmount:N0} VND | " +
                             $"Đã thu: {paidAmount:N0} VND | Chờ TT: {pendingCount} | Quá hạn: {overdueCount}");
        }

        private void UpdateStatusInfo()
        {
            // Status info will be displayed in console for now
            Console.WriteLine($"Hiển thị: {_filteredBills?.Count ?? 0} / {_allBills?.Count ?? 0} hóa đơn");
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBills();
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void MonthFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void YearFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void RefreshFilter_Click(object sender, RoutedEventArgs e)
        {
            // Reset all filters
            var cmbStatusFilter = this.FindName("cmbStatusFilter") as ComboBox;
            var cmbMonthFilter = this.FindName("cmbMonthFilter") as ComboBox;
            var cmbYearFilter = this.FindName("cmbYearFilter") as ComboBox;
            var txtSearch = this.FindName("txtSearch") as TextBox;

            if (cmbStatusFilter != null) cmbStatusFilter.SelectedIndex = 0;
            if (cmbMonthFilter != null) cmbMonthFilter.SelectedIndex = 0;
            if (txtSearch != null) txtSearch.Text = "";
            
            // Set year to current year
            if (cmbYearFilter != null)
            {
                var currentYear = DateTime.Now.Year;
                foreach (ComboBoxItem item in cmbYearFilter.Items)
                {
                    if (item.Tag?.ToString() == currentYear.ToString())
                    {
                        cmbYearFilter.SelectedItem = item;
                        break;
                    }
                }
            }

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allBills == null) return;

            try
            {
                var cmbStatusFilter = this.FindName("cmbStatusFilter") as ComboBox;
                var cmbMonthFilter = this.FindName("cmbMonthFilter") as ComboBox;
                var cmbYearFilter = this.FindName("cmbYearFilter") as ComboBox;
                var txtSearch = this.FindName("txtSearch") as TextBox;

                var statusFilter = (cmbStatusFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                var monthFilter = (cmbMonthFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                var yearFilter = (cmbYearFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                var searchText = txtSearch?.Text?.Trim().ToLower() ?? "";

                _filteredBills = _allBills.Where(bill =>
                {
                    // Status filter
                    if (statusFilter != "All" && bill.BillStatus.ToString() != statusFilter)
                        return false;

                    // Month filter
                    if (monthFilter != "All" && monthFilter != null && 
                        int.TryParse(monthFilter, out int month) && bill.BillMonth != month)
                        return false;

                    // Year filter
                    if (yearFilter != null && int.TryParse(yearFilter, out int year) && bill.BillYear != year)
                        return false;

                    // Search filter
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        var billId = bill.BillId.ToString().ToLower();
                        var roomNumber = bill.Contract?.RoomNumber?.ToLower() ?? "";
                        var customerName = bill.Contract?.CustomerName?.ToLower() ?? "";
                        
                        if (!billId.Contains(searchText) && 
                            !roomNumber.Contains(searchText) && 
                            !customerName.Contains(searchText))
                            return false;
                    }

                    return true;
                }).ToList();

                // Update DataGrid
                var dataGrid = this.FindName("dgBills") as DataGrid;
                if (dataGrid != null)
                {
                    dataGrid.ItemsSource = _filteredBills;
                }

                UpdateFilterInfo();
                UpdateBillCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying filters: {ex.Message}");
            }
        }

        private void UpdateFilterInfo()
        {
            try
            {
                var cmbMonthFilter = this.FindName("cmbMonthFilter") as ComboBox;
                var cmbYearFilter = this.FindName("cmbYearFilter") as ComboBox;
                var txtFilterInfo = this.FindName("txtFilterInfo") as TextBlock;

                if (txtFilterInfo != null)
                {
                    var monthFilter = (cmbMonthFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString();
                    var yearFilter = (cmbYearFilter?.SelectedItem as ComboBoxItem)?.Tag?.ToString();

                    if (monthFilter == "All")
                    {
                        txtFilterInfo.Text = $"Hiển thị tất cả tháng năm {yearFilter}";
                    }
                    else
                    {
                        txtFilterInfo.Text = $"Hiển thị tháng {monthFilter}/{yearFilter}";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating filter info: {ex.Message}");
            }
        }

        private void UpdateBillCount()
        {
            try
            {
                var txtBillCount = this.FindName("txtBillCount") as TextBlock;
                if (txtBillCount != null)
                {
                    var totalAmount = _filteredBills?.Sum(b => b.TotalAmount) ?? 0;
                    txtBillCount.Text = $"Hiển thị: {_filteredBills?.Count ?? 0} / {_allBills?.Count ?? 0} hóa đơn " +
                                       $"- Tổng: {totalAmount:N0} VND";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating bill count: {ex.Message}");
            }
        }

        private void GenerateBills_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowBillGeneration?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi chuyển sang tạo hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Future: Implement Excel export functionality
                MessageBox.Show("Chức năng xuất Excel sẽ được phát triển trong phiên bản tiếp theo!", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", 
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
                    // Update the bill in repository
                    if (_billRepository.Update(statusDialog.Bill))
                    {
                        // Refresh the list to show updated status
                        LoadBills();
                        
                        MessageBox.Show("Cập nhật trạng thái hóa đơn thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể cập nhật trạng thái hóa đơn. Vui lòng thử lại!", "Lỗi", 
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
    }
}
