using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AnLaNPWPF.ViewModels;
using AnLaNPWPF.Helpers;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for ReportsView.xaml
    /// </summary>
    public partial class ReportsView : Page
    {
        private ReportsViewModel viewModel;

        public ReportsView()
        {
            InitializeComponent();
            viewModel = new ReportsViewModel(suppressInitialMessageBox: true);
            DataContext = viewModel;
            
            // Initialize date range to current month
            InitializeDateRange();
        }

        private void InitializeDateRange()
        {
            var today = DateTime.Today;
            dpStartDate.SelectedDate = new DateTime(today.Year, today.Month, 1);
            dpEndDate.SelectedDate = today;
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (dpStartDate.SelectedDate.HasValue && dpEndDate.SelectedDate.HasValue)
            {
                viewModel.GenerateReport(dpStartDate.SelectedDate.Value, dpEndDate.SelectedDate.Value);
                LoadReportData();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn khoảng thời gian để tạo báo cáo.", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ThisMonth_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            dpStartDate.SelectedDate = new DateTime(today.Year, today.Month, 1);
            dpEndDate.SelectedDate = today;
            GenerateReport_Click(sender, e);
        }

        private void LastMonth_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var lastMonth = today.AddMonths(-1);
            dpStartDate.SelectedDate = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            dpEndDate.SelectedDate = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
            GenerateReport_Click(sender, e);
        }

        private void ThisQuarter_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            var quarter = (today.Month - 1) / 3 + 1;
            var startMonth = (quarter - 1) * 3 + 1;
            dpStartDate.SelectedDate = new DateTime(today.Year, startMonth, 1);
            dpEndDate.SelectedDate = today;
            GenerateReport_Click(sender, e);
        }

        private void ThisYear_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            dpStartDate.SelectedDate = new DateTime(today.Year, 1, 1);
            dpEndDate.SelectedDate = today;
            GenerateReport_Click(sender, e);
        }

        private void LoadReportData()
        {
            try
            {
                // Load KPI data
                LoadKPIData();
                
                // Load detailed analysis data
                LoadRoomAnalysis();
                LoadRevenueAnalysis();
                LoadBillAnalysis();
                LoadPerformanceSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu báo cáo: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadKPIData()
        {
            var stats = viewModel.GetStatistics();
            
            // Update KPI cards
            txtTotalRevenue.Text = $"{stats.TotalRevenue:N0} VNĐ";
            txtOccupancyRate.Text = $"{stats.OccupancyRate:P1}";
            txtActiveRooms.Text = $"{stats.ActiveRooms}/{stats.TotalRooms}";
            txtActiveRoomPercent.Text = $"{(stats.TotalRooms > 0 ? (double)stats.ActiveRooms / stats.TotalRooms : 0):P0}";
            txtAvgRevenuePerRoom.Text = $"{(stats.ActiveRooms > 0 ? stats.TotalRevenue / stats.ActiveRooms : 0):N0} VNĐ";
            txtMonthlyBills.Text = $"{stats.TotalBills} HĐ";
            
            // Update trend indicators
            txtRevenueGrowth.Text = stats.RevenueGrowth >= 0 ? $"↗️ +{stats.RevenueGrowth:P1}" : $"↘️ {stats.RevenueGrowth:P1}";
            txtOccupancyTrend.Text = stats.OccupancyRate > 0.8 ? "📈 Tốt" : stats.OccupancyRate > 0.6 ? "📊 Ổn" : "📉 Cần cải thiện";
            txtAvgRevenueTrend.Text = "💹 Ổn định";
            txtBillsStatus.Text = "✅ Cập nhật";
        }

        private void LoadRoomAnalysis()
        {
            dgRoomAnalysis.ItemsSource = viewModel.GetRoomAnalysis();
        }

        private void LoadRevenueAnalysis()
        {
            var revenueData = viewModel.GetRevenueAnalysis();
            dgRevenueAnalysis.ItemsSource = revenueData;
            
            // Update revenue period cards
            var today = DateTime.Today;
            txtTodayRevenue.Text = $"{viewModel.GetDailyRevenue(today):N0} VNĐ";
            txtWeekRevenue.Text = $"{viewModel.GetWeeklyRevenue(today):N0} VNĐ";
            txtMonthRevenue.Text = $"{viewModel.GetMonthlyRevenue(today):N0} VNĐ";
            txtYearRevenue.Text = $"{viewModel.GetYearlyRevenue(today):N0} VNĐ";
        }

        private void LoadBillAnalysis()
        {
            var billData = viewModel.GetBillAnalysis();
            dgBillAnalysis.ItemsSource = billData;
            
            // Update bill status cards
            var billStats = viewModel.GetBillStatistics();
            txtPendingBills.Text = billStats.PendingBills.ToString();
            txtPaidBills.Text = billStats.PaidBills.ToString();
            txtOverdueBills.Text = billStats.OverdueBills.ToString();
            txtTotalBillAmount.Text = $"{billStats.TotalBillAmount:N0} VNĐ";
        }

        private void LoadPerformanceSummary()
        {
            dgPerformanceSummary.ItemsSource = viewModel.GetPerformanceMetrics();
            
            // Update performance indicators
            var performance = viewModel.GetBusinessPerformance();
            txtBusinessEfficiency.Text = $"{performance.EfficiencyScore:P0}";
            txtQualityScore.Text = $"{performance.QualityScore:F1}/5";
            txtRevenueTarget.Text = $"{performance.RevenueTargetAchievement:P0}";
            
            // Update insights and recommendations
            txtKeyInsights.Text = string.Join("\n", performance.KeyInsights.Select(x => $"• {x}"));
            txtRecommendations.Text = string.Join("\n", performance.Recommendations.Select(x => $"• {x}"));
        }

        private void ExportReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.ExportReport();
                MessageBox.Show("Báo cáo đã được xuất thành công!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất báo cáo: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.PrintReport();
                MessageBox.Show("Báo cáo đã được gửi đến máy in!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in báo cáo: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.RefreshData();
                LoadReportData();
                MessageBox.Show("Dữ liệu đã được làm mới!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi làm mới dữ liệu: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get current active tab
                var activeTab = tabReports.SelectedIndex;
                
                switch (activeTab)
                {
                    case 0: // Room Analysis
                        ExportRoomAnalysis();
                        break;
                    case 1: // Revenue Analysis  
                        ExportRevenueAnalysis();
                        break;
                    case 2: // Bill Analysis
                        ExportBillAnalysis();
                        break;
                    case 3: // Performance Summary
                        ExportPerformanceSummary();
                        break;
                    default:
                        ExportFullReport();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportRoomAnalysis()
        {
            if (dgRoomAnalysis.ItemsSource != null)
            {
                ExcelExporter.ExportReportToExcel(dgRoomAnalysis.ItemsSource, "PhongPhong");
            }
            else
            {
                MessageBox.Show("Không có dữ liệu phân tích phòng để xuất!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportRevenueAnalysis()
        {
            if (dgRevenueAnalysis.ItemsSource != null)
            {
                ExcelExporter.ExportReportToExcel(dgRevenueAnalysis.ItemsSource, "DoanhThu");
            }
            else
            {
                MessageBox.Show("Không có dữ liệu phân tích doanh thu để xuất!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportBillAnalysis()
        {
            if (dgBillAnalysis.ItemsSource != null)
            {
                ExcelExporter.ExportReportToExcel(dgBillAnalysis.ItemsSource, "HoaDon");
            }
            else
            {
                MessageBox.Show("Không có dữ liệu phân tích hóa đơn để xuất!", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExportPerformanceSummary()
        {
            ExcelExporter.ExportReportToExcel(GetKPIData(), "HieuQua");
        }

        private void ExportFullReport()
        {
            ExcelExporter.ExportReportToExcel(GetAllReportData(), "TongQuan");
        }

        private object GetKPIData()
        {
            return new
            {
                TotalRevenue = txtTotalRevenue.Text,
                OccupancyRate = txtOccupancyRate.Text,
                ActiveRooms = txtActiveRooms.Text,
                AvgRevenuePerRoom = txtAvgRevenuePerRoom.Text,
                MonthlyBills = txtMonthlyBills.Text
            };
        }

        private object GetAllReportData()
        {
            return new
            {
                KPIData = GetKPIData(),
                RoomData = dgRoomAnalysis.ItemsSource,
                RevenueData = dgRevenueAnalysis.ItemsSource,
                BillData = dgBillAnalysis.ItemsSource
            };
        }
    }
} 