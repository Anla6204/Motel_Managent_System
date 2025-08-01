using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;


namespace Repository
{
    public class DataSource
    {
        private static DataSource instance;
        public static DataSource Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataSource();
                }
                return instance;
            }
        }

        public List<CustomerModel> Customers { get; set; }
        public List<RoomInformationModel> Rooms { get; set; }
        public List<RoomTypeModel> RoomTypes { get; set; }
        public List<BookingReservationModel> Reservations { get; set; }
        public List<BookingDetail> BookingDetails { get; set; }

        private DataSource()
        {
            // Dummy seed data
            Customers = new List<CustomerModel>();
            Rooms = new List<RoomInformationModel>();
            RoomTypes = new List<RoomTypeModel>();
            Reservations = new List<BookingReservationModel>();
            BookingDetails = new List<BookingDetail>();
        }
    }
}
