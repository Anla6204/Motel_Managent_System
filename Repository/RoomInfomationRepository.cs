using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;

namespace Repository
{
    public class RoomInfomationRepository
    {
        private readonly List<RoomInformationModel> rooms = DataSource.Instance.Rooms;
        private readonly string connectionString;

        public RoomInfomationRepository()
        {
            connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement")
                ?? throw new InvalidOperationException("Connection string 'FUMiniHotelManagement' not found in appsettings.json");
        }

        public List<RoomInformationModel> GetAll()
        {
            var roomsDict = new Dictionary<int, RoomInformationModel>();
            string query = @"
                SELECT r.*, rt.RoomTypeName, rt.TypeDescription, rt.TypeNote,
                       c.CustomerID, c.CustomerFullName, c.Telephone, c.EmailAddress, 
                       c.CustomerBirthday, c.CCCD, c.Gender, c.Address, c.Occupation, 
                       c.EmergencyContact, c.Notes, c.CheckInDate,
                       c.CCCDFrontImagePath, c.CCCDBackImagePath
                FROM RoomInformation r
                JOIN RoomType rt ON r.RoomTypeID = rt.RoomTypeID
                LEFT JOIN Customer c ON r.RoomID = c.CurrentRoomID AND c.CustomerStatus = 1
                ORDER BY r.RoomID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int roomID = reader.GetInt32(reader.GetOrdinal("RoomID"));
                    
                    // Nếu phòng chưa tồn tại trong dictionary, tạo mới
                    if (!roomsDict.ContainsKey(roomID))
                    {
                        var room = new RoomInformationModel
                        {
                            RoomID = roomID,
                            RoomNumber = reader.GetString(reader.GetOrdinal("RoomNumber")),
                            RoomDetailDescription = reader.GetString(reader.GetOrdinal("RoomDetailDescription")),
                            RoomMaxCapacity = reader.GetInt32(reader.GetOrdinal("RoomMaxCapacity")),
                            RoomTypeID = reader.GetInt32(reader.GetOrdinal("RoomTypeID")),
                            RoomStatus = reader.GetByte(reader.GetOrdinal("RoomStatus")),
                            RoomPricePerMonth = reader.GetDecimal(reader.GetOrdinal("RoomPricePerDay")), // Tạm thời dùng column cũ
                            RoomTypes = new RoomTypeModel
                            {
                                RoomTypeID = reader.GetInt32(reader.GetOrdinal("RoomTypeID")),
                                RoomTypeName = reader.GetString(reader.GetOrdinal("RoomTypeName")),
                                TypeDescription = reader.GetString(reader.GetOrdinal("TypeDescription")),
                                TypeNote = reader.GetString(reader.GetOrdinal("TypeNote"))
                            },
                            CurrentCustomers = new List<CustomerModel>()
                        };
                        roomsDict[roomID] = room;
                    }

                    // Thêm khách hàng vào phòng (nếu có)
                    if (!reader.IsDBNull(reader.GetOrdinal("CustomerID")))
                    {
                        var customer = new CustomerModel
                        {
                            CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                            CustomerFullName = reader.GetString(reader.GetOrdinal("CustomerFullName")),
                            Telephone = reader.IsDBNull(reader.GetOrdinal("Telephone")) ? null : reader.GetString(reader.GetOrdinal("Telephone")),
                            EmailAddress = reader.IsDBNull(reader.GetOrdinal("EmailAddress")) ? null : reader.GetString(reader.GetOrdinal("EmailAddress")),
                            CustomerBirthday = reader.GetDateTime(reader.GetOrdinal("CustomerBirthday")),
                            CCCD = reader.IsDBNull(reader.GetOrdinal("CCCD")) ? null : reader.GetString(reader.GetOrdinal("CCCD")),
                            Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : reader.GetString(reader.GetOrdinal("Gender")),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                            Occupation = reader.IsDBNull(reader.GetOrdinal("Occupation")) ? null : reader.GetString(reader.GetOrdinal("Occupation")),
                            EmergencyContact = reader.IsDBNull(reader.GetOrdinal("EmergencyContact")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContact")),
                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                            CheckInDate = reader.IsDBNull(reader.GetOrdinal("CheckInDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("CheckInDate")),
                            CCCDFrontImagePath = reader.IsDBNull(reader.GetOrdinal("CCCDFrontImagePath")) ? null : reader.GetString(reader.GetOrdinal("CCCDFrontImagePath")),
                            CCCDBackImagePath = reader.IsDBNull(reader.GetOrdinal("CCCDBackImagePath")) ? null : reader.GetString(reader.GetOrdinal("CCCDBackImagePath")),
                            CurrentRoomID = roomID,
                            CurrentRoomNumber = roomsDict[roomID].RoomNumber
                        };
                        
                        roomsDict[roomID].CurrentCustomers.Add(customer);
                    }
                }
            }

            // Cập nhật thông tin hiển thị cho từng phòng
            foreach (var room in roomsDict.Values)
            {
                // Ưu tiên RoomStatus trước, sau đó mới kiểm tra HasCustomer
                switch (room.RoomStatus)
                {
                    case 0: // Phòng trống
                        room.RoomStatusText = "⭕ Phòng trống";
                        break;
                    case 1: // Hoạt động  
                        room.RoomStatusText = "🟢 Hoạt động";
                        break;
                    case 2: // Bảo trì
                        room.RoomStatusText = "🔧 Bảo trì";
                        break;
                    default:
                        room.RoomStatusText = "⭕ Phòng trống";
                        break;
                }

                // Set customer info
                if (room.HasCustomer)
                {
                    room.CustomerInfo = room.CurrentCustomers.Count == 1 ? 
                        room.CurrentCustomers[0].CustomerFullName : 
                        $"{room.CurrentCustomers.Count} người ({string.Join(", ", room.CurrentCustomers.Take(2).Select(c => c.CustomerFullName))}{(room.CurrentCustomers.Count > 2 ? "..." : "")})";
                }
                else
                {
                    room.CustomerInfo = "Không có khách";
                }
            }

            return roomsDict.Values.ToList();
        }

        public List<RoomTypeModel> GetAllRoomTypes()
        {
            var roomTypes = new List<RoomTypeModel>();
            string query = "SELECT * FROM RoomType";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    roomTypes.Add(new RoomTypeModel
                    {
                        RoomTypeID = reader.GetInt32(reader.GetOrdinal("RoomTypeID")),
                        RoomTypeName = reader.GetString(reader.GetOrdinal("RoomTypeName")),
                        TypeDescription = reader.GetString(reader.GetOrdinal("TypeDescription")),
                        TypeNote = reader.GetString(reader.GetOrdinal("TypeNote"))
                    });
                }
            }

            return roomTypes;
        }

        public void Add(RoomInformationModel room)
        {
            string query = @"
            INSERT INTO RoomInformation (RoomNumber, RoomDetailDescription, RoomMaxCapacity, RoomTypeID, RoomStatus, RoomPricePerDay)
            VALUES (@RoomNumber, @RoomDetailDescription, @RoomMaxCapacity, @RoomTypeID, @RoomStatus, @RoomPricePerDay);
            SELECT SCOPE_IDENTITY();";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RoomNumber", room.RoomNumber);
                cmd.Parameters.AddWithValue("@RoomDetailDescription", room.RoomDetailDescription ?? string.Empty);
                cmd.Parameters.AddWithValue("@RoomMaxCapacity", room.RoomMaxCapacity);
                cmd.Parameters.AddWithValue("@RoomTypeID", room.RoomTypeID);
                cmd.Parameters.AddWithValue("@RoomStatus", room.RoomStatus);
                cmd.Parameters.AddWithValue("@RoomPricePerDay", room.RoomPricePerMonth);

                conn.Open();
                // Get the newly created ID and assign it to the room
                room.RoomID = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void Update(RoomInformationModel room)
        {
            string query = @"
            UPDATE RoomInformation
            SET 
                RoomNumber = @RoomNumber,
                RoomDetailDescription = @RoomDetailDescription,
                RoomMaxCapacity = @RoomMaxCapacity,
                RoomTypeID = @RoomTypeID,
                RoomStatus = @RoomStatus,
                RoomPricePerDay = @RoomPricePerDay
            WHERE RoomID = @RoomID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RoomNumber", room.RoomNumber);
                cmd.Parameters.AddWithValue("@RoomDetailDescription", room.RoomDetailDescription ?? string.Empty);
                cmd.Parameters.AddWithValue("@RoomMaxCapacity", room.RoomMaxCapacity);
                cmd.Parameters.AddWithValue("@RoomTypeID", room.RoomTypeID);
                cmd.Parameters.AddWithValue("@RoomStatus", room.RoomStatus);
                cmd.Parameters.AddWithValue("@RoomPricePerDay", room.RoomPricePerMonth);
                cmd.Parameters.AddWithValue("@RoomID", room.RoomID);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Update failed: No room found with the specified ID.");
                }
            }
        }

        public void Delete(int roomId)
        {
            string query = @"DELETE FROM RoomInformation WHERE RoomID = @RoomID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RoomID", roomId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("Delete failed: No room found with the specified ID.");
                }
            }
        }

        // ✅ Thêm method cập nhật room status
        public void UpdateRoomStatus(int roomId, int newStatus)
        {
            string query = @"
                UPDATE RoomInformation
                SET RoomStatus = @RoomStatus
                WHERE RoomID = @RoomID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RoomStatus", newStatus);
                cmd.Parameters.AddWithValue("@RoomID", roomId);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new Exception("UpdateRoomStatus failed: No room found with the specified ID.");
                }
            }
        }

        // ✅ Method mới để xóa khách hàng khỏi phòng khi hợp đồng kết thúc
        public void RemoveCustomersFromRoom(int roomId)
        {
            try
            {
                // Cập nhật tất cả khách hàng trong phòng này thành không có phòng
                string query = @"
                    UPDATE Customer
                    SET CurrentRoomID = NULL, 
                        CheckInDate = NULL
                    WHERE CurrentRoomID = @RoomID";

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RoomID", roomId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa khách hàng khỏi phòng: {ex.Message}");
            }
        }

        // ✅ Method cập nhật hoàn chỉnh: cả trạng thái phòng và xóa khách hàng
        public void UpdateRoomStatusAndClearCustomers(int roomId, int newStatus)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Xóa khách hàng khỏi phòng nếu status = 0 (phòng trống)
                            if (newStatus == 0)
                            {
                                string clearCustomersQuery = @"
                                    UPDATE Customer
                                    SET CurrentRoomID = NULL, 
                                        CheckInDate = NULL
                                    WHERE CurrentRoomID = @RoomID";

                                using (SqlCommand cmd = new SqlCommand(clearCustomersQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@RoomID", roomId);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // 2. Cập nhật trạng thái phòng
                            string updateRoomQuery = @"
                                UPDATE RoomInformation
                                SET RoomStatus = @RoomStatus
                                WHERE RoomID = @RoomID";

                            using (SqlCommand cmd = new SqlCommand(updateRoomQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@RoomStatus", newStatus);
                                cmd.Parameters.AddWithValue("@RoomID", roomId);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    throw new Exception("Không tìm thấy phòng với ID đã cho.");
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật phòng và xóa khách hàng: {ex.Message}");
            }
        }
    }
}
