using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class BookingModel
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } // For display purposes
        public string CustomerName { get; set; } // For display purposes
        public DateTime BookingDate { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int BookingStatusId { get; set; } // 1: Pending, 2: Confirmed, 3: Completed, 4: Cancelled
        public string BookingStatus { get; set; } // For display purposes
        
        // Navigation properties
        public CustomerModel Customer { get; set; }
        public RoomInformationModel Room { get; set; }
    }
}