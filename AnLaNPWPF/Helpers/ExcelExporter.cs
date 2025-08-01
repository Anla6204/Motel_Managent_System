using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using BusinessObject;

namespace AnLaNPWPF.Helpers
{
    public static class ExcelExporter
    {
        /// <summary>
        /// Xuất danh sách phòng ra file Excel (CSV)
        /// </summary>
        public static void ExportRoomsToExcel(List<RoomInformationModel> rooms)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    AddExtension = true,
                    FileName = $"DanhSachPhong_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    
                    // Header
                    csv.AppendLine("Số Phòng,Loại Phòng,Trạng Thái,Khách Hàng,Giá/Tháng,Sức Chứa,Mô Tả");
                    
                    // Data rows
                    foreach (var room in rooms)
                    {
                        var line = $"\"{room.RoomNumber}\"," +
                                  $"\"{room.RoomTypes?.RoomTypeName ?? ""}\"," +
                                  $"\"{room.RoomStatusDisplay}\"," +
                                  $"\"{room.CustomerInfo ?? ""}\"," +
                                  $"\"{room.RoomPricePerMonth:N0}\"," +
                                  $"\"{room.OccupancyInfo ?? ""}\"," +
                                  $"\"{room.RoomDetailDescription ?? ""}\"";
                        csv.AppendLine(line);
                    }
                    
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Xuất file Excel thành công!\nĐường dẫn: {saveFileDialog.FileName}", 
                                  "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", 
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xuất danh sách khách hàng ra file Excel (CSV)
        /// </summary>
        public static void ExportCustomersToExcel(List<CustomerModel> customers)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    AddExtension = true,
                    FileName = $"DanhSachKhachHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    
                    // Header
                    csv.AppendLine("Họ Tên,Điện Thoại,Email,Giới Tính,CCCD,Phòng Hiện Tại,Ngày Vào,Địa Chỉ,Nghề Nghiệp");
                    
                    // Data rows
                    foreach (var customer in customers)
                    {
                        var line = $"\"{customer.CustomerFullName}\"," +
                                  $"\"{customer.Telephone}\"," +
                                  $"\"{customer.EmailAddress}\"," +
                                  $"\"{customer.Gender}\"," +
                                  $"\"{customer.CCCD}\"," +
                                  $"\"{customer.CurrentRoomNumber ?? ""}\"," +
                                  $"\"{customer.CheckInDate?.ToString("dd/MM/yyyy") ?? ""}\"," +
                                  $"\"{customer.Address}\"," +
                                  $"\"{customer.Occupation}\"";
                        csv.AppendLine(line);
                    }
                    
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Xuất file Excel thành công!\nĐường dẫn: {saveFileDialog.FileName}", 
                                  "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", 
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xuất danh sách hóa đơn ra file Excel (CSV)
        /// </summary>
        public static void ExportBillsToExcel(List<MonthlyBill> bills)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv",
                    AddExtension = true,
                    FileName = $"DanhSachHoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    
                    // Header
                    csv.AppendLine("Mã HĐ,Phòng,Khách Hàng,Tháng/Năm,Tiền Phòng,Tiền Điện,Tiền Nước,Tổng Tiền,Trạng Thái,Ngày Tạo");
                    
                    // Data rows
                    foreach (var bill in bills)
                    {
                        var line = $"\"{bill.BillId}\"," +
                                  $"\"{bill.RoomNumber}\"," +
                                  $"\"{bill.CustomerName}\"," +
                                  $"\"{bill.BillMonth}/{bill.BillYear}\"," +
                                  $"\"{bill.RoomRent:N0}\"," +
                                  $"\"{bill.ElectricityCost:N0}\"," +
                                  $"\"{bill.WaterCost:N0}\"," +
                                  $"\"{bill.TotalAmount:N0}\"," +
                                  $"\"{bill.BillStatus}\"," +
                                  $"\"{bill.CreatedAt:dd/MM/yyyy}\"";
                        csv.AppendLine(line);
                    }
                    
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Xuất file Excel thành công!\nĐường dẫn: {saveFileDialog.FileName}", 
                                  "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", 
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xuất danh sách hóa đơn ra file Excel (CSV) với tên file tùy chọn
        /// </summary>
        /// <param name="bills">Danh sách hóa đơn để xuất</param>
        /// <param name="customFileName">Tên file tùy chọn (không bao gồm phần mở rộng)</param>
        public static void ExportBillsToExcel(List<MonthlyBill> bills, string customFileName = null)
        {
            try
            {
                if (bills == null || bills.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu hóa đơn để xuất!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    AddExtension = true,
                    FileName = string.IsNullOrEmpty(customFileName) 
                        ? $"DanhSachHoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                        : $"{customFileName}_{DateTime.Now:yyyyMMdd_HHmms}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    
                    // Header
                    csv.AppendLine("Mã HĐ,Phòng,Khách Hàng,Tháng/Năm,Tiền Phòng,Tiền Điện,Tiền Nước,Tổng Tiền,Trạng Thái,Ngày Tạo");
                    
                    // Data rows
                    foreach (var bill in bills)
                    {
                        var line = $"\"{bill.BillId}\"," +
                                  $"\"{bill.RoomNumber}\"," +
                                  $"\"{bill.CustomerName}\"," +
                                  $"\"{bill.BillMonth}/{bill.BillYear}\"," +
                                  $"\"{bill.RoomRent:N0}\"," +
                                  $"\"{bill.ElectricityCost:N0}\"," +
                                  $"\"{bill.WaterCost:N0}\"," +
                                  $"\"{bill.TotalAmount:N0}\"," +
                                  $"\"{bill.StatusText}\"," +
                                  $"\"{bill.CreatedAt:dd/MM/yyyy}\"";
                        csv.AppendLine(line);
                    }
                    
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                    
                    MessageBox.Show($"Xuất file thành công!\nĐường dẫn: {saveFileDialog.FileName}", 
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất file Excel: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Xuất báo cáo dữ liệu ra file Excel
        /// </summary>
        /// <param name="reportData">Dữ liệu báo cáo để xuất</param>
        /// <param name="reportType">Loại báo cáo (Phòng, Doanh Thu, Hóa Đơn, v.v.)</param>
        public static void ExportReportToExcel(object reportData, string reportType)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    AddExtension = true,
                    FileName = $"BaoCao_{reportType}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var csv = new StringBuilder();
                    
                    // Header based on report type
                    switch (reportType.ToLower())
                    {
                        case "room":
                        case "phong":
                            csv.AppendLine("Số Phòng,Loại Phòng,Trạng Thái,Khách Hàng,Giá Thuê,Sức Chứa");
                            break;
                        case "revenue":
                        case "doanhthu":
                            csv.AppendLine("Tháng,Năm,Tổng Thu,Chi Tiết");
                            break;
                        case "bill":
                        case "hoadon":
                            csv.AppendLine("Mã HĐ,Phòng,Khách Hàng,Tháng/Năm,Tổng Tiền,Trạng Thái");
                            break;
                        default:
                            csv.AppendLine("Dữ liệu báo cáo");
                            break;
                    }
                    
                    // For now, add a simple data representation
                    csv.AppendLine($"\"Báo cáo {reportType} được tạo lúc {DateTime.Now:dd/MM/yyyy HH:mm:ss}\"");
                    
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                    
                    MessageBox.Show($"Xuất báo cáo {reportType} thành công!\nĐường dẫn: {saveFileDialog.FileName}", 
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất báo cáo: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
