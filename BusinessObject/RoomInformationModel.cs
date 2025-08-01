using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class RoomInformationModel
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; }
        public string RoomDetailDescription { get; set; }
        public int RoomMaxCapacity { get; set; }
        public int RoomTypeID { get; set; }
        public RoomTypeModel RoomTypes { get; set; }
        public int RoomStatus { get; set; }
        public decimal RoomPricePerMonth { get; set; } // Đổi từ RoomPricePerDay thành RoomPricePerMonth
        
        // Backward compatibility property
        public decimal RoomPricePerDay => RoomPricePerMonth; // Alias cho tương thích ngược
        
        // Danh sách khách hàng đang ở trong phòng
        public List<CustomerModel> CurrentCustomers { get; set; } = new List<CustomerModel>();
        
        // Backward compatibility - lấy khách đầu tiên nếu có
        public CustomerModel CurrentCustomer => CurrentCustomers?.FirstOrDefault();
        
        // Properties for UI display
        public string RoomStatusText { get; set; }
        public string CustomerInfo { get; set; }
        
        // Property để hiển thị trạng thái phòng từ database
        public string RoomStatusFromDB
        {
            get
            {
                switch (RoomStatus)
                {
                    case 0: return "🏠 Sẵn sàng thuê";
                    case 1: return "🏡 Đang được thuê";
                    case 2: return "🔧 Bảo trì";
                    default: return "❓ Không xác định";
                }
            }
        }

        // Property mới cho trạng thái phòng với logic đồng bộ
        public string RoomStatusDisplay
        {
            get
            {
                // Logic đồng bộ với RoomStatusText
                if (string.IsNullOrEmpty(RoomStatusText))
                {
                    // Fallback về RoomStatusFromDB nếu RoomStatusText chưa được set
                    return RoomStatusFromDB;
                }

                // Đồng bộ theo logic:
                // "Phòng trống" hoặc "⭕ Phòng trống" -> "Sẵn sàng thuê"
                // "Hoạt động" hoặc "🟢 Hoạt động" -> "Đang được thuê" 
                // "Bảo trì" -> "Bảo trì"
                
                string status = RoomStatusText.ToLower();
                if (status.Contains("phòng trống") || status.Contains("trống"))
                {
                    return "🏠 Sẵn sàng thuê";
                }
                else if (status.Contains("hoạt động") || status.Contains("đang hoạt động"))
                {
                    return "🏡 Đang được thuê";
                }
                else if (status.Contains("bảo trì"))
                {
                    return "🔧 Bảo trì";
                }
                else
                {
                    // Nếu không khớp, fallback về logic RoomStatus
                    return RoomStatusFromDB;
                }
            }
        }
        
        // Property để hiển thị trong ComboBox với thông tin chi tiết
        public string RoomDisplayInfo
        {
            get
            {
                string status = RoomStatus == 0 ? "🏠 Trống" : "🏡 Có người";
                return $"Phòng {RoomNumber} - {status} ({CurrentOccupancy}/{RoomMaxCapacity})";
            }
        }
        
        public bool HasCustomer => CurrentCustomers?.Any() == true;
        public int CurrentOccupancy => CurrentCustomers?.Count ?? 0;
        public string OccupancyInfo => $"{CurrentOccupancy}/{RoomMaxCapacity} người";
        
        // Thông tin hợp đồng - sẽ được thêm sau khi tạo đầy đủ RentalContract và MonthlyBill
        // public List<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
        // public RentalContract ActiveContract => RentalContracts?.FirstOrDefault(c => c.Status == ContractStatus.Active);
        // public bool HasActiveContract => ActiveContract != null;
        
        // Thông tin thanh toán
        // public MonthlyBill CurrentMonthBill => ActiveContract?.MonthlyBills?.FirstOrDefault(b => 
        //     b.BillingMonth == DateTime.Now.Month && b.BillingYear == DateTime.Now.Year);
        // public bool IsCurrentMonthPaid => CurrentMonthBill?.Status == BillStatus.Paid;

        // Method để đồng bộ RoomStatus từ RoomStatusText
        public void SyncRoomStatusFromText()
        {
            if (string.IsNullOrEmpty(RoomStatusText))
                return;

            string status = RoomStatusText.ToLower();
            
            // Mapping logic:
            // "Phòng trống" hoặc "⭕ Phòng trống" -> RoomStatus = 0 (Sẵn sàng thuê)
            // "Hoạt động" hoặc "🟢 Hoạt động" -> RoomStatus = 1 (Đang được thuê)
            // "Bảo trì" -> RoomStatus = 2 (Bảo trì)
            
            if (status.Contains("phòng trống") || status.Contains("trống"))
            {
                RoomStatus = 0; // Sẵn sàng thuê
            }
            else if (status.Contains("hoạt động") || status.Contains("đang hoạt động"))
            {
                RoomStatus = 1; // Đang được thuê
            }
            else if (status.Contains("bảo trì"))
            {
                RoomStatus = 2; // Bảo trì
            }
            // Nếu không khớp pattern nào, giữ nguyên RoomStatus hiện tại
        }

        // Method để set trạng thái phòng theo logic nghiệp vụ
        public void SetRoomStatus(string statusText)
        {
            RoomStatusText = statusText;
            SyncRoomStatusFromText();
        }
    }
}
