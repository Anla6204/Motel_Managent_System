using System;

namespace BusinessObject
{
    public class BookingStatistics
    {
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
}
