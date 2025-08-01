using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class BookingReservationModel
    {
        public int BookingReservationID { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int CustomerID { get; set; }
        public int BookingStatus { get; set; }
    }
}
