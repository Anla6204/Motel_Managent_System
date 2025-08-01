using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
namespace Repository
{
    public class CustomerRepository
    {
        private readonly List<CustomerModel> customers = DataSource.Instance.Customers;
        private readonly string connectionString;

        public CustomerRepository()
        {
            connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement")
                ?? throw new InvalidOperationException("Connection string 'FUMiniHotelManagement' not found in appsettings.json");
        }

        public List<CustomerModel> GetAll()
        {
            var customers = new List<CustomerModel>();

            string query = @"
                SELECT c.CustomerID, c.CustomerFullName, c.Telephone, c.EmailAddress, 
                       c.CustomerBirthday, c.CustomerStatus, c.Password, c.Gender, 
                       c.CCCD, c.CurrentRoomID, c.CheckInDate, c.Address, 
                       c.Occupation, c.EmergencyContact, c.Notes, c.CCCDFrontImagePath,
                       c.CCCDBackImagePath, r.RoomNumber
                FROM Customer c 
                LEFT JOIN RoomInformation r ON c.CurrentRoomID = r.RoomID 
                WHERE c.CustomerStatus = 1"; 

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    customers.Add(new CustomerModel
                    {
                        CustomerID = reader.GetInt32(0),
                        CustomerFullName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Telephone = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        EmailAddress = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        CustomerBirthday = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                        CustomerStatus = reader.IsDBNull(5) ? (byte)0 : reader.GetByte(5),
                        Password = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        Gender = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        CCCD = reader.IsDBNull(8) ? "" : reader.GetString(8),
                        CurrentRoomID = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                        CheckInDate = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10),
                        Address = reader.IsDBNull(11) ? "" : reader.GetString(11),
                        Occupation = reader.IsDBNull(12) ? "" : reader.GetString(12),
                        EmergencyContact = reader.IsDBNull(13) ? "" : reader.GetString(13),
                        Notes = reader.IsDBNull(14) ? "" : reader.GetString(14),
                        CCCDFrontImagePath = reader.IsDBNull(15) ? "" : reader.GetString(15),
                        CCCDBackImagePath = reader.IsDBNull(16) ? "" : reader.GetString(16),
                        CurrentRoomNumber = reader.IsDBNull(17) ? "" : reader.GetString(17)
                    });
                }
            }

            return customers;
        }

        public CustomerModel GetByEmail(string email) => customers.FirstOrDefault(c => c.EmailAddress == email);

        public CustomerModel GetByEmailAndPassword(string email, string password)
        {
            List<CustomerModel> customers = new List<CustomerModel>();

            string query = "SELECT * FROM Customer WHERE CustomerStatus = 1";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    customers.Add(new CustomerModel
                    {
                        CustomerID = reader.GetInt32(0),
                        CustomerFullName = reader.GetString(1),
                        Telephone = reader.GetString(2),
                        EmailAddress = reader.GetString(3),
                        CustomerBirthday = reader.GetDateTime(4),
                        CustomerStatus = reader.GetByte(5),
                        Password = reader.GetString(6)
                    });
                }
            }

            // LINQ to Objects
            return customers.FirstOrDefault(c =>
                c.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase)
                && c.Password == password);
        }

        public bool Add(CustomerModel customer)
        {
            try
            {
                string query = @"
                INSERT INTO Customer (CustomerFullName, Telephone, EmailAddress, CustomerBirthday, CustomerStatus, Password, 
                                    Gender, CCCD, CurrentRoomID, CheckInDate, Address, Occupation, EmergencyContact, Notes,
                                    CCCDFrontImagePath, CCCDBackImagePath)
                VALUES (@CustomerFullName, @Telephone, @EmailAddress, @CustomerBirthday, @CustomerStatus, @Password,
                       @Gender, @CCCD, @CurrentRoomID, @CheckInDate, @Address, @Occupation, @EmergencyContact, @Notes,
                       @CCCDFrontImagePath, @CCCDBackImagePath);
                SELECT SCOPE_IDENTITY();";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerFullName", customer.CustomerFullName ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Telephone", customer.Telephone ?? string.Empty);
                    cmd.Parameters.AddWithValue("@EmailAddress", customer.EmailAddress ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CustomerBirthday", customer.CustomerBirthday);
                    cmd.Parameters.AddWithValue("@CustomerStatus", customer.CustomerStatus);
                    cmd.Parameters.AddWithValue("@Password", customer.Password ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Gender", customer.Gender ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CCCD", customer.CCCD ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CurrentRoomID", (object)customer.CurrentRoomID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CheckInDate", (object)customer.CheckInDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", customer.Address ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Occupation", customer.Occupation ?? string.Empty);
                    cmd.Parameters.AddWithValue("@EmergencyContact", customer.EmergencyContact ?? string.Empty);
                    cmd.Parameters.AddWithValue("@Notes", customer.Notes ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CCCDFrontImagePath", customer.CCCDFrontImagePath ?? string.Empty);
                    cmd.Parameters.AddWithValue("@CCCDBackImagePath", customer.CCCDBackImagePath ?? string.Empty);

                    conn.Open();
                    // Get the newly created ID and assign it to the customer
                    customer.CustomerID = Convert.ToInt32(cmd.ExecuteScalar());
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Update(CustomerModel customer)
        {
            string query = @"
            UPDATE Customer
            SET 
                CustomerFullName = @CustomerFullName,
                Telephone = @Telephone,
                EmailAddress = @EmailAddress,
                CustomerBirthday = @CustomerBirthday,
                CustomerStatus = @CustomerStatus,
                Password = @Password,
                Gender = @Gender,
                CCCD = @CCCD,
                CurrentRoomID = @CurrentRoomID,
                CheckInDate = @CheckInDate,
                Address = @Address,
                Occupation = @Occupation,
                EmergencyContact = @EmergencyContact,
                Notes = @Notes,
                CCCDFrontImagePath = @CCCDFrontImagePath,
                CCCDBackImagePath = @CCCDBackImagePath
            WHERE CustomerID = @CustomerID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerFullName", customer.CustomerFullName ?? string.Empty);
                cmd.Parameters.AddWithValue("@Telephone", customer.Telephone ?? string.Empty);
                cmd.Parameters.AddWithValue("@EmailAddress", customer.EmailAddress ?? string.Empty);
                cmd.Parameters.AddWithValue("@CustomerBirthday", customer.CustomerBirthday);
                cmd.Parameters.AddWithValue("@CustomerStatus", customer.CustomerStatus);
                cmd.Parameters.AddWithValue("@Password", customer.Password ?? string.Empty);
                cmd.Parameters.AddWithValue("@Gender", customer.Gender ?? string.Empty);
                cmd.Parameters.AddWithValue("@CCCD", customer.CCCD ?? string.Empty);
                cmd.Parameters.AddWithValue("@CurrentRoomID", (object)customer.CurrentRoomID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CheckInDate", (object)customer.CheckInDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", customer.Address ?? string.Empty);
                cmd.Parameters.AddWithValue("@Occupation", customer.Occupation ?? string.Empty);
                cmd.Parameters.AddWithValue("@EmergencyContact", customer.EmergencyContact ?? string.Empty);
                cmd.Parameters.AddWithValue("@Notes", customer.Notes ?? string.Empty);
                cmd.Parameters.AddWithValue("@CCCDFrontImagePath", customer.CCCDFrontImagePath ?? string.Empty);
                cmd.Parameters.AddWithValue("@CCCDBackImagePath", customer.CCCDBackImagePath ?? string.Empty);
                cmd.Parameters.AddWithValue("@CustomerID", customer.CustomerID);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: No customer found with the specified ID.");
                }
            }
        }

        public void Delete(int customerId)
        {
            string query = @"DELETE FROM Customer WHERE CustomerID = @CustomerID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customerId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Delete failed: No customer found with the specified ID.");
                }
            }
        }

    }
}
