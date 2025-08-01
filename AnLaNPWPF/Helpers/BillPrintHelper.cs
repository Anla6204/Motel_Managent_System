using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using BusinessObject;

namespace AnLaNPWPF.Helpers
{
    public static class BillPrintHelper
    {
        public static void PrintBill(MonthlyBill bill)
        {
            try
            {
                // Tạo HTML content cho hóa đơn
                string htmlContent = GenerateBillHtml(bill);
                
                // Lưu vào file tạm
                string tempFile = Path.Combine(Path.GetTempPath(), $"HoaDon_{bill.BillId}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                File.WriteAllText(tempFile, htmlContent, Encoding.UTF8);
                
                // Mở trong trình duyệt để in
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
                
                MessageBox.Show($"Hóa đơn đã được mở trong trình duyệt để in.\nFile: {tempFile}", 
                    "In hóa đơn", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private static string GenerateBillHtml(MonthlyBill bill)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='vi'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>Hóa đơn thanh toán</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        @media print {");
            html.AppendLine("            body { margin: 0; }");
            html.AppendLine("            .no-print { display: none; }");
            html.AppendLine("            @page { margin: 15mm; size: A4; }");
            html.AppendLine("            html, body { height: auto !important; overflow: hidden !important; }");
            html.AppendLine("        }");
            html.AppendLine("        body {");
            html.AppendLine("            font-family: 'Times New Roman', serif;");
            html.AppendLine("            max-width: 800px;");
            html.AppendLine("            margin: 20px auto;");
            html.AppendLine("            padding: 20px;");
            html.AppendLine("            background: white;");
            html.AppendLine("            color: #333;");
            html.AppendLine("            page-break-after: avoid;");
            html.AppendLine("        }");
            html.AppendLine("        .header {");
            html.AppendLine("            text-align: center;");
            html.AppendLine("            border-bottom: 2px solid #333;");
            html.AppendLine("            padding-bottom: 15px;");
            html.AppendLine("            margin-bottom: 20px;");
            html.AppendLine("        }");
            html.AppendLine("        .company-name {");
            html.AppendLine("            font-size: 24px;");
            html.AppendLine("            font-weight: bold;");
            html.AppendLine("            color: #2c3e50;");
            html.AppendLine("            margin-bottom: 5px;");
            html.AppendLine("        }");
            html.AppendLine("        .company-info {");
            html.AppendLine("            font-size: 14px;");
            html.AppendLine("            color: #666;");
            html.AppendLine("            margin-bottom: 10px;");
            html.AppendLine("        }");
            html.AppendLine("        .bill-title {");
            html.AppendLine("            font-size: 22px;");
            html.AppendLine("            font-weight: bold;");
            html.AppendLine("            color: #e74c3c;");
            html.AppendLine("            margin-top: 15px;");
            html.AppendLine("        }");
            html.AppendLine("        .bill-info {");
            html.AppendLine("            display: flex;");
            html.AppendLine("            justify-content: space-between;");
            html.AppendLine("            margin-bottom: 20px;");
            html.AppendLine("        }");
            html.AppendLine("        .info-section {");
            html.AppendLine("            flex: 1;");
            html.AppendLine("        }");
            html.AppendLine("        .info-section h3 {");
            html.AppendLine("            font-size: 16px;");
            html.AppendLine("            color: #2c3e50;");
            html.AppendLine("            border-bottom: 1px solid #bdc3c7;");
            html.AppendLine("            padding-bottom: 5px;");
            html.AppendLine("            margin-bottom: 10px;");
            html.AppendLine("        }");
            html.AppendLine("        .info-item {");
            html.AppendLine("            margin: 5px 0;");
            html.AppendLine("            display: flex;");
            html.AppendLine("        }");
            html.AppendLine("        .info-label {");
            html.AppendLine("            font-weight: bold;");
            html.AppendLine("            width: 120px;");
            html.AppendLine("            color: #34495e;");
            html.AppendLine("        }");
            html.AppendLine("        .info-value {");
            html.AppendLine("            flex: 1;");
            html.AppendLine("        }");
            html.AppendLine("        .charges-table {");
            html.AppendLine("            width: 100%;");
            html.AppendLine("            border-collapse: collapse;");
            html.AppendLine("            margin: 15px 0;");
            html.AppendLine("        }");
            html.AppendLine("        .charges-table th,");
            html.AppendLine("        .charges-table td {");
            html.AppendLine("            border: 1px solid #bdc3c7;");
            html.AppendLine("            padding: 8px;");
            html.AppendLine("            text-align: left;");
            html.AppendLine("        }");
            html.AppendLine("        .charges-table th {");
            html.AppendLine("            background-color: #34495e;");
            html.AppendLine("            color: white;");
            html.AppendLine("            font-weight: bold;");
            html.AppendLine("        }");
            html.AppendLine("        .charges-table .amount {");
            html.AppendLine("            text-align: right;");
            html.AppendLine("            font-weight: bold;");
            html.AppendLine("        }");
            html.AppendLine("        .total-row {");
            html.AppendLine("            background-color: #ecf0f1;");
            html.AppendLine("            font-weight: bold;");
            html.AppendLine("            font-size: 16px;");
            html.AppendLine("        }");
            html.AppendLine("        .total-amount {");
            html.AppendLine("            color: #e74c3c;");
            html.AppendLine("            font-size: 18px;");
            html.AppendLine("        }");
            html.AppendLine("        .payment-status {");
            html.AppendLine("            margin: 15px 0;");
            html.AppendLine("            padding: 10px;");
            html.AppendLine("            border-radius: 5px;");
            html.AppendLine("            text-align: center;");
            html.AppendLine("            font-weight: bold;");
            html.AppendLine("        }");
            html.AppendLine("        .status-pending {");
            html.AppendLine("            background-color: #fff3cd;");
            html.AppendLine("            color: #856404;");
            html.AppendLine("            border: 1px solid #ffeaa7;");
            html.AppendLine("        }");
            html.AppendLine("        .status-paid {");
            html.AppendLine("            background-color: #d4edda;");
            html.AppendLine("            color: #155724;");
            html.AppendLine("            border: 1px solid #c3e6cb;");
            html.AppendLine("        }");
            html.AppendLine("        .status-overdue {");
            html.AppendLine("            background-color: #f8d7da;");
            html.AppendLine("            color: #721c24;");
            html.AppendLine("            border: 1px solid #f5c6cb;");
            html.AppendLine("        }");
            html.AppendLine("        .footer {");
            html.AppendLine("            margin-top: 40px;");
            html.AppendLine("            border-top: 1px solid #bdc3c7;");
            html.AppendLine("            padding-top: 20px;");
            html.AppendLine("            text-align: center;");
            html.AppendLine("            color: #666;");
            html.AppendLine("            font-size: 12px;");
            html.AppendLine("        }");
            html.AppendLine("        .print-button {");
            html.AppendLine("            background-color: #3498db;");
            html.AppendLine("            color: white;");
            html.AppendLine("            padding: 10px 20px;");
            html.AppendLine("            border: none;");
            html.AppendLine("            border-radius: 5px;");
            html.AppendLine("            font-size: 16px;");
            html.AppendLine("            cursor: pointer;");
            html.AppendLine("            margin: 20px auto;");
            html.AppendLine("            display: block;");
            html.AppendLine("        }");
            html.AppendLine("        .print-button:hover {");
            html.AppendLine("            background-color: #2980b9;");
            html.AppendLine("        }");
            html.AppendLine("        ");
            html.AppendLine("        /* CSS để ẩn đường dẫn file và URL khi in */");
            html.AppendLine("        @media print {");
            html.AppendLine("            @page {");
            html.AppendLine("                margin: 15mm;");
            html.AppendLine("                size: A4;");
            html.AppendLine("                /* Ẩn header và footer của trình duyệt */");
            html.AppendLine("            }");
            html.AppendLine("            ");
            html.AppendLine("            html {");
            html.AppendLine("                -webkit-print-color-adjust: exact;");
            html.AppendLine("                color-adjust: exact;");
            html.AppendLine("            }");
            html.AppendLine("            ");
            html.AppendLine("            /* Ẩn URL và đường dẫn file */");
            html.AppendLine("            body::before, body::after {");
            html.AppendLine("                display: none !important;");
            html.AppendLine("            }");
            html.AppendLine("            ");
            html.AppendLine("            /* Đảm bảo nội dung chỉ hiển thị trong 1 trang */");
            html.AppendLine("            body {");
            html.AppendLine("                max-height: 90vh !important;");
            html.AppendLine("                overflow: hidden !important;");
            html.AppendLine("                page-break-after: avoid !important;");
            html.AppendLine("            }");
            html.AppendLine("            ");
            html.AppendLine("            .footer {");
            html.AppendLine("                margin-top: 20px !important;");
            html.AppendLine("                page-break-inside: avoid;");
            html.AppendLine("            }");
            html.AppendLine("        }");
            html.AppendLine("    </style>");
            html.AppendLine("    <script>");
            html.AppendLine("        // JavaScript để cài đặt print options");
            html.AppendLine("        window.onload = function() {");
            html.AppendLine("            // Tự động focus vào nút in");
            html.AppendLine("            document.querySelector('.print-button').focus();");
            html.AppendLine("        };");
            html.AppendLine("        ");
            html.AppendLine("        function printBill() {");
            html.AppendLine("            // Cài đặt print options để ẩn header/footer");
            html.AppendLine("            var printOptions = {");
            html.AppendLine("                silent: false,");
            html.AppendLine("                printBackground: true,");
            html.AppendLine("                deviceName: null");
            html.AppendLine("            };");
            html.AppendLine("            window.print();");
            html.AppendLine("        }");
            html.AppendLine("    </script>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine("    <div class='header'>");
            html.AppendLine("        <div class='company-name'>NHÀ TRỌ THÀNH ĐẠT 3636</div>");
            html.AppendLine("        <div class='company-info'>Địa chỉ: Số 83, Đường Quản Bạch, Phường Cao Lãnh, tỉnh Đồng Tháp</div>");
            html.AppendLine("        <div class='company-info'>Điện thoại: 0977.663.636 - 0917.224.997</div>");
            html.AppendLine("        <div class='bill-title'>HÓA ĐƠN THANH TOÁN</div>");
            html.AppendLine("    </div>");
            
            // Print button (hidden when printing)
            html.AppendLine("    <button class='print-button no-print' onclick='printBill()'>🖨️ In hóa đơn</button>");
            
            // Bill info
            html.AppendLine("    <div class='bill-info'>");
            html.AppendLine("        <div class='info-section'>");
            html.AppendLine("            <h3>Thông tin hóa đơn</h3>");
            html.AppendLine($"            <div class='info-item'>");
            html.AppendLine($"                <span class='info-label'>Số hóa đơn:</span>");
            html.AppendLine($"                <span class='info-value'>#{bill.BillId}</span>");
            html.AppendLine($"            </div>");
            html.AppendLine($"            <div class='info-item'>");
            html.AppendLine($"                <span class='info-label'>Kỳ thanh toán:</span>");
            html.AppendLine($"                <span class='info-value'>{bill.BillPeriod}</span>");
            html.AppendLine($"            </div>");
            html.AppendLine($"            <div class='info-item'>");
            html.AppendLine($"                <span class='info-label'>Hạn thanh toán:</span>");
            html.AppendLine($"                <span class='info-value'>{bill.FormattedDueDate}</span>");
            html.AppendLine($"            </div>");
            html.AppendLine($"            <div class='info-item'>");
            html.AppendLine($"                <span class='info-label'>Ngày tạo:</span>");
            html.AppendLine($"                <span class='info-value'>{bill.CreatedAt:dd/MM/yyyy}</span>");
            html.AppendLine($"            </div>");
            html.AppendLine("        </div>");
            
            html.AppendLine("        <div class='info-section'>");
            html.AppendLine("            <h3>Thông tin khách hàng</h3>");
            html.AppendLine($"            <div class='info-item'>");
            html.AppendLine($"                <span class='info-label'>Họ tên:</span>");
            html.AppendLine($"                <span class='info-value'>{bill.Contract?.Customer?.CustomerFullName ?? "N/A"}</span>");
            html.AppendLine($"            </div>");
            html.AppendLine($"            <div class='info-item'>");
            html.AppendLine($"                <span class='info-label'>Số phòng:</span>");
            html.AppendLine($"                <span class='info-value'>{bill.Contract?.Room?.RoomNumber ?? "N/A"}</span>");
            html.AppendLine($"            </div>");
            html.AppendLine($"            <div class='info-item'>");
            html.AppendLine($"                <span class='info-label'>Hợp đồng:</span>");
            html.AppendLine($"                <span class='info-value'>{bill.Contract?.ContractNumber ?? "N/A"}</span>");
            html.AppendLine($"            </div>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            
            // Charges table
            html.AppendLine("    <table class='charges-table'>");
            html.AppendLine("        <thead>");
            html.AppendLine("            <tr>");
            html.AppendLine("                <th>Khoản thu</th>");
            html.AppendLine("                <th>Chi tiết</th>");
            html.AppendLine("                <th>Số tiền</th>");
            html.AppendLine("            </tr>");
            html.AppendLine("        </thead>");
            html.AppendLine("        <tbody>");
            
            // Room rent
            html.AppendLine("            <tr>");
            html.AppendLine("                <td>Tiền phòng</td>");
            html.AppendLine("                <td>Tiền thuê phòng tháng</td>");
            html.AppendLine($"                <td class='amount'>{bill.FormattedRoomCharge}</td>");
            html.AppendLine("            </tr>");
            
            // Electricity
            html.AppendLine("            <tr>");
            html.AppendLine("                <td>Tiền điện</td>");
            html.AppendLine($"                <td>{bill.ElectricityUsage:N1} kWh × {bill.ElectricityRate:N0} đ/kWh</td>");
            html.AppendLine($"                <td class='amount'>{bill.FormattedElectricityCost}</td>");
            html.AppendLine("            </tr>");
            
            // Water
            html.AppendLine("            <tr>");
            html.AppendLine("                <td>Tiền nước</td>");
            html.AppendLine($"                <td>{bill.WaterUsage:N1} m³ × {bill.WaterRate:N0} đ/m³</td>");
            html.AppendLine($"                <td class='amount'>{bill.FormattedWaterCost}</td>");
            html.AppendLine("            </tr>");
            
            // Other charges
            if (bill.OtherCharges > 0)
            {
                html.AppendLine("            <tr>");
                html.AppendLine("                <td>Phí khác</td>");
                html.AppendLine("                <td>Phí dịch vụ, vệ sinh, bảo trì</td>");
                html.AppendLine($"                <td class='amount'>{bill.FormattedOtherCharges}</td>");
                html.AppendLine("            </tr>");
            }
            
            // Total
            html.AppendLine("            <tr class='total-row'>");
            html.AppendLine("                <td colspan='2'>TỔNG CỘNG</td>");
            html.AppendLine($"                <td class='amount total-amount'>{bill.FormattedTotalAmount}</td>");
            html.AppendLine("            </tr>");
            html.AppendLine("        </tbody>");
            html.AppendLine("    </table>");
            
            // Payment status
            string statusClass;
            switch (bill.BillStatus)
            {
                case BillStatus.Paid:
                    statusClass = "status-paid";
                    break;
                case BillStatus.Overdue:
                    statusClass = "status-overdue";
                    break;
                default:
                    statusClass = "status-pending";
                    break;
            }
            
            html.AppendLine($"    <div class='payment-status {statusClass}'>");
            html.AppendLine($"        TRẠNG THÁI: {bill.PaymentStatusText}");
            if (bill.BillStatus == BillStatus.Paid && bill.PaidDate.HasValue)
            {
                html.AppendLine($"<br>Đã thanh toán ngày: {bill.PaidDate.Value:dd/MM/yyyy}");
                if (bill.PaidAmount.HasValue)
                {
                    html.AppendLine($"<br>Số tiền đã thanh toán: {bill.PaidAmount.Value:C0}");
                }
            }
            html.AppendLine("    </div>");
            
            // Notes - compact version
            html.AppendLine("    <div style='margin: 15px 0; padding: 10px; border: 1px solid #bdc3c7; border-radius: 5px;'>");
            html.AppendLine("        <h4 style='margin-top: 0; margin-bottom: 8px; color: #2c3e50; font-size: 14px;'>Ghi chú:</h4>");
            html.AppendLine("        <ul style='margin: 5px 0; padding-left: 20px; color: #666; font-size: 12px;'>");
            html.AppendLine("            <li>Vui lòng thanh toán đúng hạn để tránh phát sinh phí trễ hạn</li>");
            html.AppendLine("            <li>Mọi thắc mắc xin liên hệ ban quản lý - Cảm ơn quý khách</li>");
            html.AppendLine("        </ul>");
            html.AppendLine("    </div>");
            
            // Footer
            html.AppendLine("    <div class='footer'>");
            // Footer - compact
            html.AppendLine("    <div class='footer'>");
            html.AppendLine($"        <p style='margin: 5px 0; font-size: 11px;'>Hóa đơn được tạo tự động vào {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>");
            html.AppendLine("        <p style='margin: 5px 0; font-size: 11px;'>© 2025 Nhà trọ Thành Đạt 3636 - Phần mềm quản lý nhà trọ</p>");
            html.AppendLine("    </div>");
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
    }
}
