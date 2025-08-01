using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using BusinessObject;
using Repository;

namespace AnLaNPWPF.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private DateTime _startDate;
        private DateTime _endDate;
        private ObservableCollection<BookingReportModel> _bookingReports;
        private BookingStatistics _statistics;
        private readonly BookingRepository _bookingRepository;
        private readonly RoomInfomationRepository _roomRepository;
        private bool _suppressInitialMessageBox = false;

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        public ObservableCollection<BookingReportModel> BookingReports
        {
            get => _bookingReports;
            set { _bookingReports = value; OnPropertyChanged(); }
        }

        public BookingStatistics Statistics
        {
            get => _statistics;
            set { _statistics = value; OnPropertyChanged(); }
        }

        public ICommand GenerateReportCommand { get; }

        public ReportsViewModel(bool suppressInitialMessageBox = true)
        {
            _bookingRepository = new BookingRepository();
            _roomRepository = new RoomInfomationRepository();
            _suppressInitialMessageBox = suppressInitialMessageBox;
            
            // Initialize with default dates (current month)
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = DateTime.Now;
            
            GenerateReportCommand = new RelayCommand(ExecuteGenerateReport);
            
            // Load initial report
            ExecuteGenerateReport(null);
        }

        public void GenerateReport(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            ExecuteGenerateReport(null);
        }

        private void ExecuteGenerateReport(object parameter)
        {
            try
            {
                // Get bookings from repository
                var bookings = _bookingRepository.GetBookingsByDateRange(StartDate, EndDate);
                
                // Convert to report models and sort by booking date descending
                var reportData = bookings.Select(b => new BookingReportModel
                {
                    BookingId = b.BookingId,
                    CustomerName = b.CustomerName,
                    RoomNumber = b.RoomNumber,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    TotalPrice = b.TotalPrice,
                    BookingStatus = b.BookingStatus,
                    BookingDate = b.BookingDate
                }).OrderByDescending(b => b.BookingDate)
                  .ThenByDescending(b => b.CheckInDate)
                  .ToList();
                
                BookingReports = new ObservableCollection<BookingReportModel>(reportData);
                
                // Get statistics
                Statistics = _bookingRepository.GetBookingStatistics(StartDate, EndDate);
                Statistics.PeriodStart = StartDate;
                Statistics.PeriodEnd = EndDate;
                
                // Only show the message box if explicitly requested (through button click)
                if (!_suppressInitialMessageBox && parameter != null)
                {
                    MessageBox.Show($"Báo cáo đã được tạo!\nTổng số booking: {Statistics.TotalBookings}\nTổng doanh thu: {Statistics.TotalRevenue:C}", 
                        "Báo cáo thống kê", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ReportStatistics GetStatistics()
        {
            var rooms = _roomRepository.GetAll();
            var bookings = _bookingRepository.GetBookingsByDateRange(StartDate, EndDate);
            
            var activeRooms = rooms.Where(r => r.RoomStatus == 1 || r.HasCustomer).Count();
            var totalRooms = rooms.Count;
            var totalRevenue = bookings.Sum(b => b.TotalPrice);
            var occupancyRate = totalRooms > 0 ? (double)activeRooms / totalRooms : 0;
            
            return new ReportStatistics
            {
                TotalRevenue = totalRevenue,
                OccupancyRate = occupancyRate,
                ActiveRooms = activeRooms,
                TotalRooms = totalRooms,
                TotalBills = bookings.Count,
                RevenueGrowth = CalculateRevenueGrowth()
            };
        }

        public List<RoomAnalysisModel> GetRoomAnalysis()
        {
            var rooms = _roomRepository.GetAll();
            var bookings = _bookingRepository.GetBookingsByDateRange(StartDate, EndDate);
            
            return rooms.Select(room => 
            {
                var roomBookings = bookings.Where(b => b.RoomNumber == room.RoomNumber).ToList();
                var revenue = roomBookings.Sum(b => b.TotalPrice);
                var totalDays = (EndDate - StartDate).Days + 1;
                var bookedDays = roomBookings.Sum(b => (b.CheckOutDate - b.CheckInDate).Days);
                
                return new RoomAnalysisModel
                {
                    RoomNumber = room.RoomNumber,
                    RoomType = room.RoomTypes?.RoomTypeName ?? "Chưa xác định",
                    Status = room.RoomStatusDisplay,
                    CustomerName = room.CurrentCustomer?.CustomerFullName ?? "Trống",
                    StartDate = roomBookings.FirstOrDefault()?.CheckInDate,
                    EndDate = roomBookings.LastOrDefault()?.CheckOutDate,
                    Revenue = revenue,
                    UtilizationRate = totalDays > 0 ? (double)bookedDays / totalDays : 0
                };
            }).ToList();
        }

        public List<RevenueAnalysisModel> GetRevenueAnalysis()
        {
            var bookings = _bookingRepository.GetBookingsByDateRange(StartDate, EndDate);
            
            return bookings.GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month })
                          .Select(g => new RevenueAnalysisModel
                          {
                              Month = g.Key.Month,
                              Year = g.Key.Year,
                              RoomsRented = g.Count(),
                              TotalRevenue = g.Sum(b => b.TotalPrice),
                              AverageRevenuePerRoom = g.Count() > 0 ? g.Sum(b => b.TotalPrice) / g.Count() : 0,
                              GrowthRate = 0 // Có thể tính toán so với tháng trước
                          }).OrderBy(r => r.Year).ThenBy(r => r.Month).ToList();
        }

        public List<BillAnalysisModel> GetBillAnalysis()
        {
            // Giả sử có repository cho bills, hiện tại dùng booking data
            var bookings = _bookingRepository.GetBookingsByDateRange(StartDate, EndDate);
            
            return bookings.Select(b => new BillAnalysisModel
            {
                BillId = b.BookingId.ToString(),
                RoomNumber = b.RoomNumber,
                CustomerName = b.CustomerName,
                BillPeriod = $"{b.CheckInDate.Month}/{b.CheckInDate.Year}",
                Amount = b.TotalPrice,
                Status = b.BookingStatus == "Completed" ? "Đã thanh toán" : "Chờ thanh toán",
                DueDate = b.CheckOutDate.AddDays(7), // Hạn thanh toán 7 ngày sau checkout
                PaymentDate = b.BookingStatus == "Completed" ? (DateTime?)b.CheckOutDate : null
            }).ToList();
        }

        public BillStatistics GetBillStatistics()
        {
            var bills = GetBillAnalysis();
            
            return new BillStatistics
            {
                PendingBills = bills.Count(b => b.Status == "Chờ thanh toán"),
                PaidBills = bills.Count(b => b.Status == "Đã thanh toán"),
                OverdueBills = bills.Count(b => b.Status == "Chờ thanh toán" && b.DueDate < DateTime.Now),
                TotalBillAmount = bills.Sum(b => b.Amount)
            };
        }

        public List<PerformanceMetricModel> GetPerformanceMetrics()
        {
            var stats = GetStatistics();
            var lastMonthStats = GetLastMonthStatistics();
            
            return new List<PerformanceMetricModel>
            {
                new PerformanceMetricModel
                {
                    Metric = "Tỷ lệ lấp đầy",
                    CurrentMonth = $"{stats.OccupancyRate:P1}",
                    LastMonth = $"{lastMonthStats.OccupancyRate:P1}",
                    Change = $"{(stats.OccupancyRate - lastMonthStats.OccupancyRate):P1}",
                    Target = "80%",
                    Status = stats.OccupancyRate >= 0.8 ? "Đạt" : "Chưa đạt"
                },
                new PerformanceMetricModel
                {
                    Metric = "Doanh thu",
                    CurrentMonth = $"{stats.TotalRevenue:N0} VNĐ",
                    LastMonth = $"{lastMonthStats.TotalRevenue:N0} VNĐ",
                    Change = $"{((stats.TotalRevenue - lastMonthStats.TotalRevenue) / Math.Max(lastMonthStats.TotalRevenue, 1)):P1}",
                    Target = "50,000,000 VNĐ",
                    Status = stats.TotalRevenue >= 50000000 ? "Đạt" : "Chưa đạt"
                }
            };
        }

        public BusinessPerformance GetBusinessPerformance()
        {
            var stats = GetStatistics();
            
            return new BusinessPerformance
            {
                EfficiencyScore = Math.Min(stats.OccupancyRate * 1.2, 1.0), // Adjusted efficiency
                QualityScore = 4.2, // Mock data
                RevenueTargetAchievement = Math.Min((double)(stats.TotalRevenue / 50000000m), 1.0), // Target 50M
                KeyInsights = new List<string>
                {
                    $"Tỷ lệ lấp đầy hiện tại: {stats.OccupancyRate:P1}",
                    $"Phòng hoạt động: {stats.ActiveRooms}/{stats.TotalRooms}",
                    $"Doanh thu trung bình/phòng: {(stats.ActiveRooms > 0 ? stats.TotalRevenue / stats.ActiveRooms : 0):N0} VNĐ"
                },
                Recommendations = new List<string>
                {
                    "Tối ưu hóa giá phòng theo mùa",
                    "Cải thiện chất lượng dịch vụ",
                    "Mở rộng kênh marketing online"
                }
            };
        }

        public decimal GetDailyRevenue(DateTime date)
        {
            var bookings = _bookingRepository.GetBookingsByDateRange(date, date);
            return bookings.Sum(b => b.TotalPrice);
        }

        public decimal GetWeeklyRevenue(DateTime date)
        {
            var weekStart = date.AddDays(-(int)date.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);
            var bookings = _bookingRepository.GetBookingsByDateRange(weekStart, weekEnd);
            return bookings.Sum(b => b.TotalPrice);
        }

        public decimal GetMonthlyRevenue(DateTime date)
        {
            var monthStart = new DateTime(date.Year, date.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var bookings = _bookingRepository.GetBookingsByDateRange(monthStart, monthEnd);
            return bookings.Sum(b => b.TotalPrice);
        }

        public decimal GetYearlyRevenue(DateTime date)
        {
            var yearStart = new DateTime(date.Year, 1, 1);
            var yearEnd = new DateTime(date.Year, 12, 31);
            var bookings = _bookingRepository.GetBookingsByDateRange(yearStart, yearEnd);
            return bookings.Sum(b => b.TotalPrice);
        }

        public void ExportReport()
        {
            // Implementation for export
            System.Diagnostics.Process.Start("notepad.exe");
        }

        public void PrintReport()
        {
            // Implementation for print
        }

        public void RefreshData()
        {
            ExecuteGenerateReport(null);
        }

        private double CalculateRevenueGrowth()
        {
            var currentRevenue = Statistics?.TotalRevenue ?? 0;
            var lastMonthStats = GetLastMonthStatistics();
            var lastRevenue = lastMonthStats.TotalRevenue;
            
            return lastRevenue > 0 ? (double)(currentRevenue - lastRevenue) / (double)lastRevenue : 0;
        }

        private ReportStatistics GetLastMonthStatistics()
        {
            var lastMonth = StartDate.AddMonths(-1);
            var lastMonthStart = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);
            
            var bookings = _bookingRepository.GetBookingsByDateRange(lastMonthStart, lastMonthEnd);
            var rooms = _roomRepository.GetAll();
            
            return new ReportStatistics
            {
                TotalRevenue = bookings.Sum(b => b.TotalPrice),
                OccupancyRate = 0.75, // Mock data
                ActiveRooms = rooms.Count(r => r.RoomStatus == 1 || r.HasCustomer),
                TotalRooms = rooms.Count,
                TotalBills = bookings.Count,
                RevenueGrowth = 0
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Data Models
    public class BookingReportModel
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string BookingStatus { get; set; }
        public DateTime BookingDate { get; set; }
    }

    public class ReportStatistics
    {
        public decimal TotalRevenue { get; set; }
        public double OccupancyRate { get; set; }
        public int ActiveRooms { get; set; }
        public int TotalRooms { get; set; }
        public int TotalBills { get; set; }
        public double RevenueGrowth { get; set; }
    }

    public class RoomAnalysisModel
    {
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Revenue { get; set; }
        public double UtilizationRate { get; set; }
    }

    public class RevenueAnalysisModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int RoomsRented { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerRoom { get; set; }
        public double GrowthRate { get; set; }
    }

    public class BillAnalysisModel
    {
        public string BillId { get; set; }
        public string RoomNumber { get; set; }
        public string CustomerName { get; set; }
        public string BillPeriod { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class BillStatistics
    {
        public int PendingBills { get; set; }
        public int PaidBills { get; set; }
        public int OverdueBills { get; set; }
        public decimal TotalBillAmount { get; set; }
    }

    public class PerformanceMetricModel
    {
        public string Metric { get; set; }
        public string CurrentMonth { get; set; }
        public string LastMonth { get; set; }
        public string Change { get; set; }
        public string Target { get; set; }
        public string Status { get; set; }
    }

    public class BusinessPerformance
    {
        public double EfficiencyScore { get; set; }
        public double QualityScore { get; set; }
        public double RevenueTargetAchievement { get; set; }
        public List<string> KeyInsights { get; set; }
        public List<string> Recommendations { get; set; }
    }
} 