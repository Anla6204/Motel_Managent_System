using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BusinessObject;
using Repository;
using AnLaNPWPF.Helpers;

namespace AnLaNPWPF.Views
{
    public partial class BillManagementView : UserControl
    {
        private MonthlyBillRepository _billRepository;
        
        public BillManagementView()
        {
            InitializeComponent();
            
            try
            {
                _billRepository = new MonthlyBillRepository();
                LoadStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo Bill Management: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi khởi tạo";
            }
        }

        private void LoadStatistics()
        {
            try
            {
                txtStatusInfo.Text = "Đang tải thống kê...";
                
                var allBills = _billRepository.GetAll();
                var totalBills = allBills.Count;
                var pendingBills = 0;
                var paidBills = 0;
                var overdueBills = 0;
                
                foreach (var bill in allBills)
                {
                    switch (bill.BillStatus)
                    {
                        case BillStatus.Pending:
                            pendingBills++;
                            if (bill.DueDate < DateTime.Today)
                                overdueBills++;
                            break;
                        case BillStatus.Paid:
                            paidBills++;
                            break;
                        case BillStatus.Partial:
                            pendingBills++;
                            break;
                        case BillStatus.Overdue:
                            overdueBills++;
                            break;
                    }
                }
                
                txtTotalBills.Text = totalBills.ToString();
                txtPendingBills.Text = pendingBills.ToString();
                txtPaidBills.Text = paidBills.ToString();
                txtOverdueBills.Text = overdueBills.ToString();
                
                txtStatusInfo.Text = "Tải thống kê thành công";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thống kê: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi tải thống kê";
            }
        }

        private void GenerateBills_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var billDialog = new BillGenerationDialog();
                if (billDialog.ShowDialog() == true && billDialog.IsGenerated)
                {
                    txtStatusInfo.Text = "Tạo hóa đơn hoàn tất";
                    
                    MessageBox.Show($"Đã tạo thành công {billDialog.BillsCreated} hóa đơn!", 
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Refresh statistics after generating bills
                    LoadStatistics();
                }
                else
                {
                    txtStatusInfo.Text = "Đã hủy tạo hóa đơn";
                }
            }
            catch (Exception ex)
            {
                txtStatusInfo.Text = "Lỗi tạo hóa đơn";
                MessageBox.Show($"Lỗi khi mở dialog tạo hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewAllBills_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var billHistoryDialog = new BillHistoryDialog();
                billHistoryDialog.ShowDialog();
                
                // Refresh statistics after viewing/updating bills
                LoadStatistics();
                txtStatusInfo.Text = "Đã xem lịch sử hóa đơn";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xem lịch sử hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi xem lịch sử";
            }
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatusInfo.Text = "Đang làm mới dữ liệu...";
                
                // Refresh all statistics and data
                LoadStatistics();
                
                txtStatusInfo.Text = "Sẵn sàng";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi làm mới dữ liệu: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi";
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatusInfo.Text = "Đang xuất danh sách hóa đơn...";
                
                var allBills = _billRepository.GetAll();
                if (allBills != null && allBills.Count > 0)
                {
                    ExcelExporter.ExportBillsToExcel(allBills);
                    txtStatusInfo.Text = "Xuất Excel thành công";
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu hóa đơn để xuất!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtStatusInfo.Text = "Không có dữ liệu";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi xuất Excel";
            }
        }

        // Card click events
        private void GenerateBillsCard_Click(object sender, MouseButtonEventArgs e)
        {
            GenerateBills_Click(sender, null);
        }

        private void BillHistoryCard_Click(object sender, MouseButtonEventArgs e)
        {
            ViewAllBills_Click(sender, null);
        }

        private void MonthlyReportCard_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                txtStatusInfo.Text = "Đang chuẩn bị báo cáo tháng...";
                
                // Future: Open monthly report dialog
                MessageBox.Show("Chức năng báo cáo tháng sẽ được phát triển trong phiên bản tiếp theo!", 
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                
                txtStatusInfo.Text = "Sẵn sàng";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi";
            }
        }

        private void OverdueBillsCard_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                txtStatusInfo.Text = "Đang tải hóa đơn quá hạn...";
                
                // Open bill history with overdue filter
                var billHistoryDialog = new BillHistoryDialog();
                billHistoryDialog.ShowDialog();
                
                // Future: Pre-filter to show only overdue bills
                LoadStatistics();
                txtStatusInfo.Text = "Đã xem hóa đơn quá hạn";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xem hóa đơn quá hạn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi xem hóa đơn quá hạn";
            }
        }
    }
}
