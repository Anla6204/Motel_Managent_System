using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BusinessObject;

namespace Repository
{
    public class BookingRepository
    {
        private readonly string connectionString;

        public BookingRepository()
        {
            // Get connection string from appsettings.json
            connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement")
                ?? throw new InvalidOperationException("Connection string 'FUMiniHotelManagement' not found in appsettings.json");
        }

        public List<BookingModel> GetAll()
        {
            List<BookingModel> bookings = new List<BookingModel>();
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string query = @"
                    SELECT 
                        br.BookingReservationID,
                        br.BookingDate,
                        br.TotalPrice,
                        br.CustomerID,
                        br.BookingStatus,
                        bd.RoomID,
                        bd.StartDate,
                        bd.EndDate,
                        bd.ActualPrice,
                        c.CustomerFullName,
                        ri.RoomNumber
                    FROM BookingReservation br
                    INNER JOIN BookingDetail bd ON br.BookingReservationID = bd.BookingReservationID
                    INNER JOIN Customer c ON br.CustomerID = c.CustomerID
                    INNER JOIN RoomInformation ri ON bd.RoomID = ri.RoomID
                    ORDER BY br.BookingDate DESC";
                
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    bookings.Add(new BookingModel
                    {
                        BookingId = (int)reader["BookingReservationID"],
                        CustomerId = (int)reader["CustomerID"],
                        RoomId = (int)reader["RoomID"],
                        RoomNumber = reader["RoomNumber"].ToString(),
                        CustomerName = reader["CustomerFullName"].ToString(),
                        BookingDate = (DateTime)reader["BookingDate"],
                        CheckInDate = (DateTime)reader["StartDate"],
                        CheckOutDate = (DateTime)reader["EndDate"],
                        TotalPrice = (decimal)reader["TotalPrice"],
                        BookingStatusId = (byte)reader["BookingStatus"],
                        BookingStatus = GetBookingStatusText((byte)reader["BookingStatus"])
                    });
                }
            }
            
            return bookings;
        }
        
        private string GetBookingStatusText(byte statusId)
        {
            switch (statusId)
            {
                case 1: return "Active";
                case 2: return "Confirmed";
                case 3: return "Completed";
                case 4: return "Cancelled";
                default: return "Unknown";
            }
        }

        public List<BookingModel> GetByCustomerId(int customerId)
        {
            // In a real implementation, filter bookings by customer ID from the database
            return GetAll().Where(b => b.CustomerId == customerId).ToList();
        }

        public BookingModel GetById(int id)
        {
            return GetAll().FirstOrDefault(b => b.BookingId == id);
        }

        public void Add(BookingModel booking)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get next BookingReservationID
                        string getMaxIdQuery = "SELECT ISNULL(MAX(BookingReservationID), 0) + 1 FROM BookingReservation";
                        SqlCommand getMaxIdCmd = new SqlCommand(getMaxIdQuery, connection, transaction);
                        int nextBookingId = (int)getMaxIdCmd.ExecuteScalar();

                        // Insert into BookingReservation table
                        string insertBookingQuery = @"
                            INSERT INTO BookingReservation (BookingReservationID, BookingDate, TotalPrice, CustomerID, BookingStatus) 
                            VALUES (@BookingReservationID, @BookingDate, @TotalPrice, @CustomerID, @BookingStatus)";

                        SqlCommand insertBookingCmd = new SqlCommand(insertBookingQuery, connection, transaction);
                        insertBookingCmd.Parameters.AddWithValue("@BookingReservationID", nextBookingId);
                        insertBookingCmd.Parameters.AddWithValue("@BookingDate", DateTime.Now.Date);
                        insertBookingCmd.Parameters.AddWithValue("@TotalPrice", booking.TotalPrice);
                        insertBookingCmd.Parameters.AddWithValue("@CustomerID", booking.CustomerId);
                        insertBookingCmd.Parameters.AddWithValue("@BookingStatus", 1); // 1 = Pending/Active
                        
                        insertBookingCmd.ExecuteNonQuery();

                        // Insert into BookingDetail table
                        string insertDetailQuery = @"
                            INSERT INTO BookingDetail (BookingReservationID, RoomID, StartDate, EndDate, ActualPrice) 
                            VALUES (@BookingReservationID, @RoomID, @StartDate, @EndDate, @ActualPrice)";

                        SqlCommand insertDetailCmd = new SqlCommand(insertDetailQuery, connection, transaction);
                        insertDetailCmd.Parameters.AddWithValue("@BookingReservationID", nextBookingId);
                        insertDetailCmd.Parameters.AddWithValue("@RoomID", booking.RoomId);
                        insertDetailCmd.Parameters.AddWithValue("@StartDate", booking.CheckInDate.Date);
                        insertDetailCmd.Parameters.AddWithValue("@EndDate", booking.CheckOutDate.Date);
                        insertDetailCmd.Parameters.AddWithValue("@ActualPrice", booking.TotalPrice);
                        
                        insertDetailCmd.ExecuteNonQuery();

                        transaction.Commit();
                        
                        // Update booking object with generated ID
                        booking.BookingId = nextBookingId;
                        booking.BookingDate = DateTime.Now.Date;
                        booking.BookingStatusId = 1;
                        booking.BookingStatus = "Active";
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Update(BookingModel booking)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update BookingReservation table
                        string updateBookingQuery = @"
                            UPDATE BookingReservation 
                            SET TotalPrice = @TotalPrice, BookingStatus = @BookingStatus 
                            WHERE BookingReservationID = @BookingReservationID";

                        SqlCommand updateBookingCmd = new SqlCommand(updateBookingQuery, connection, transaction);
                        updateBookingCmd.Parameters.AddWithValue("@TotalPrice", booking.TotalPrice);
                        updateBookingCmd.Parameters.AddWithValue("@BookingStatus", booking.BookingStatusId);
                        updateBookingCmd.Parameters.AddWithValue("@BookingReservationID", booking.BookingId);
                        
                        updateBookingCmd.ExecuteNonQuery();

                        // Update BookingDetail table
                        string updateDetailQuery = @"
                            UPDATE BookingDetail 
                            SET StartDate = @StartDate, EndDate = @EndDate, ActualPrice = @ActualPrice 
                            WHERE BookingReservationID = @BookingReservationID AND RoomID = @RoomID";

                        SqlCommand updateDetailCmd = new SqlCommand(updateDetailQuery, connection, transaction);
                        updateDetailCmd.Parameters.AddWithValue("@StartDate", booking.CheckInDate.Date);
                        updateDetailCmd.Parameters.AddWithValue("@EndDate", booking.CheckOutDate.Date);
                        updateDetailCmd.Parameters.AddWithValue("@ActualPrice", booking.TotalPrice);
                        updateDetailCmd.Parameters.AddWithValue("@BookingReservationID", booking.BookingId);
                        updateDetailCmd.Parameters.AddWithValue("@RoomID", booking.RoomId);
                        
                        updateDetailCmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Delete(int id)
        {
            // In a real implementation, delete from database or mark as deleted
        }

        public void CancelBooking(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string query = "UPDATE BookingReservation SET BookingStatus = 4 WHERE BookingReservationID = @BookingReservationID";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BookingReservationID", id);
                
                cmd.ExecuteNonQuery();
            }
        }

        public void ConfirmBooking(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string query = "UPDATE BookingReservation SET BookingStatus = 2 WHERE BookingReservationID = @BookingReservationID";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BookingReservationID", id);
                
                cmd.ExecuteNonQuery();
            }
        }

        public bool IsRoomAvailable(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string query = @"
                    SELECT COUNT(*) 
                    FROM BookingDetail bd
                    INNER JOIN BookingReservation br ON bd.BookingReservationID = br.BookingReservationID
                    WHERE bd.RoomID = @RoomID 
                    AND br.BookingStatus = 1 
                    AND (
                        (bd.StartDate <= @CheckOutDate AND bd.EndDate >= @CheckInDate)
                    )";
                
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RoomID", roomId);
                cmd.Parameters.AddWithValue("@CheckInDate", checkInDate.Date);
                cmd.Parameters.AddWithValue("@CheckOutDate", checkOutDate.Date);
                
                int conflictCount = (int)cmd.ExecuteScalar();
                return conflictCount == 0;
            }
        }

        public List<RoomInformationModel> GetAvailableRooms(DateTime checkInDate, DateTime checkOutDate)
        {
            // Get all rooms
            var roomRepository = new RoomInfomationRepository();
            var allRooms = roomRepository.GetAll();
            
            // Filter out booked rooms
            var availableRooms = new List<RoomInformationModel>();
            
            foreach (var room in allRooms)
            {
                if (IsRoomAvailable(room.RoomID, checkInDate, checkOutDate))
                {
                    availableRooms.Add(room);
                }
            }
            
            return availableRooms;
        }

        public List<BookingModel> GetBookingsByDateRange(DateTime startDate, DateTime endDate)
        {
            List<BookingModel> bookings = new List<BookingModel>();
            
            string query = @"
                SELECT br.BookingReservationID, br.CustomerID, bd.RoomID, br.BookingDate, 
                       bd.StartDate, bd.EndDate, br.TotalPrice, br.BookingStatus,
                       c.CustomerFullName, ri.RoomNumber
                FROM BookingReservation br
                INNER JOIN BookingDetail bd ON br.BookingReservationID = bd.BookingReservationID
                INNER JOIN Customer c ON br.CustomerID = c.CustomerID
                INNER JOIN RoomInformation ri ON bd.RoomID = ri.RoomID
                WHERE bd.StartDate >= @StartDate AND bd.EndDate <= @EndDate
                ORDER BY br.BookingDate DESC, bd.StartDate DESC";
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    bookings.Add(new BookingModel
                    {
                        BookingId = (int)reader["BookingReservationID"],
                        CustomerId = (int)reader["CustomerID"],
                        RoomId = (int)reader["RoomID"],
                        CustomerName = reader["CustomerFullName"].ToString(),
                        RoomNumber = reader["RoomNumber"].ToString(),
                        BookingDate = (DateTime)reader["BookingDate"],
                        CheckInDate = (DateTime)reader["StartDate"],
                        CheckOutDate = (DateTime)reader["EndDate"],
                        TotalPrice = (decimal)reader["TotalPrice"],
                        BookingStatusId = (byte)reader["BookingStatus"],
                        BookingStatus = GetBookingStatusText((byte)reader["BookingStatus"])
                    });
                }
            }
            
            return bookings;
        }

        public BookingStatistics GetBookingStatistics(DateTime startDate, DateTime endDate)
        {
            var statistics = new BookingStatistics();
            
            string query = @"
                SELECT 
                    COUNT(DISTINCT br.BookingReservationID) as TotalBookings,
                    SUM(CASE WHEN br.BookingStatus = 1 THEN 1 ELSE 0 END) as ActiveBookings,
                    SUM(CASE WHEN br.BookingStatus = 2 THEN 1 ELSE 0 END) as ConfirmedBookings,
                    SUM(CASE WHEN br.BookingStatus = 3 THEN 1 ELSE 0 END) as CompletedBookings,
                    SUM(br.TotalPrice) as TotalRevenue,
                    AVG(br.TotalPrice) as AverageBookingValue
                FROM BookingReservation br
                INNER JOIN BookingDetail bd ON br.BookingReservationID = bd.BookingReservationID
                WHERE bd.StartDate >= @StartDate AND bd.EndDate <= @EndDate";
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    statistics.TotalBookings = reader["TotalBookings"] != DBNull.Value ? (int)reader["TotalBookings"] : 0;
                    statistics.ActiveBookings = reader["ActiveBookings"] != DBNull.Value ? (int)reader["ActiveBookings"] : 0;
                    statistics.ConfirmedBookings = reader["ConfirmedBookings"] != DBNull.Value ? (int)reader["ConfirmedBookings"] : 0;
                    statistics.CompletedBookings = reader["CompletedBookings"] != DBNull.Value ? (int)reader["CompletedBookings"] : 0;
                    statistics.TotalRevenue = reader["TotalRevenue"] != DBNull.Value ? (decimal)reader["TotalRevenue"] : 0;
                    statistics.AverageBookingValue = reader["AverageBookingValue"] != DBNull.Value ? (decimal)reader["AverageBookingValue"] : 0;
                }
            }
            
            return statistics;
        }
    }
}