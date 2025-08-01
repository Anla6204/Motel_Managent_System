using System.Windows;
using BusinessObject;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for BookingDetailsDialog.xaml
    /// </summary>
    public partial class BookingDetailsDialog : Window
    {
        public BookingDetailsDialog(BookingModel booking)
        {
            InitializeComponent();
            DataContext = new BookingDetailsViewModel(booking);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class BookingDetailsViewModel
    {
        public BookingModel Booking { get; set; }

        public int BookingId => Booking.BookingId;
        public int CustomerId => Booking.CustomerId;
        public string CustomerName => Booking.CustomerName;
        public int RoomId => Booking.RoomId;
        public string RoomNumber => Booking.RoomNumber;
        public System.DateTime BookingDate => Booking.BookingDate;
        public System.DateTime CheckInDate => Booking.CheckInDate;
        public System.DateTime CheckOutDate => Booking.CheckOutDate;
        public decimal TotalPrice => Booking.TotalPrice;
        public string BookingStatus => Booking.BookingStatus;
        public int NumberOfNights => (int)(CheckOutDate - CheckInDate).TotalDays;

        public BookingDetailsViewModel(BookingModel booking)
        {
            Booking = booking;
        }
    }
}
