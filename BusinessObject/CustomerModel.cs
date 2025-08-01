using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class CustomerModel
    {
        public int CustomerID { get; set; }
        public string CustomerFullName { get; set; }
        public string Telephone { get; set; }
        public string EmailAddress { get; set; }
        public DateTime CustomerBirthday { get; set; }
        public int CustomerStatus { get; set; } // 1: Active, 2: Deleted
        public string Password { get; set; }
        
        // Thông tin bổ sung cho khách hàng
        public string Gender { get; set; } // Nam, Nữ, Khác
        public string CCCD { get; set; } // Căn cước công dân
        public int? CurrentRoomID { get; set; } // Phòng hiện tại đang ở
        public string CurrentRoomNumber { get; set; } // Số phòng (để hiển thị)
        public DateTime? CheckInDate { get; set; } // Ngày vào ở
        public string Address { get; set; } // Địa chỉ thường trú
        public string Occupation { get; set; } // Nghề nghiệp
        public string EmergencyContact { get; set; } // Liên hệ khẩn cấp
        public string Notes { get; set; } // Ghi chú
        
        // Ảnh CCCD
        public string CCCDFrontImagePath { get; set; } // Đường dẫn ảnh CCCD mặt trước
        public string CCCDBackImagePath { get; set; } // Đường dẫn ảnh CCCD mặt sau
        
        // Property cho binding trong UI - không lưu vào database
        public List<RoomInformationModel> AvailableRooms { get; set; }
        
        // Thông tin hợp đồng - sẽ được thêm sau khi tạo đầy đủ RentalContract
        // public List<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
        // public RentalContract ActiveContract => RentalContracts?.FirstOrDefault(c => c.Status == ContractStatus.Active);
        // public bool HasActiveContract => ActiveContract != null;
    }
}
