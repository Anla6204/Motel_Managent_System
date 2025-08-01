using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BusinessObject;

namespace Repository
{
    public class ResidentRepository
    {
        private readonly string _connectionString;

        public ResidentRepository()
        {
            _connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement")
                ?? throw new InvalidOperationException("Connection string 'FUMiniHotelManagement' not found in appsettings.json");
        }

        public List<ResidentModel> GetAll()
        {
            var residents = new List<ResidentModel>();
            string query = @"
                SELECT r.*, room.RoomNumber 
                FROM Resident r
                LEFT JOIN RoomInformation room ON r.RoomId = room.RoomID
                WHERE r.Status = 1
                ORDER BY r.FullName";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            residents.Add(new ResidentModel
                            {
                                ResidentId = reader.GetInt32("ResidentId"),
                                FullName = reader.GetString("FullName"),
                                Gender = reader.GetString("Gender"),
                                IdentityCard = reader.GetString("IdentityCard"),
                                BirthYear = reader.GetDateTime("BirthYear"),
                                PhoneNumber = reader.IsDBNull("PhoneNumber") ? "" : reader.GetString("PhoneNumber"),
                                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                                Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                                RoomId = reader.GetInt32("RoomId"),
                                RoomNumber = reader.IsDBNull("RoomNumber") ? "" : reader.GetString("RoomNumber"),
                                CheckInDate = reader.GetDateTime("CheckInDate"),
                                CheckOutDate = reader.IsDBNull("CheckOutDate") ? null : reader.GetDateTime("CheckOutDate"),
                                Status = reader.GetInt32("Status"),
                                Notes = reader.IsDBNull("Notes") ? "" : reader.GetString("Notes")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving residents: {ex.Message}", ex);
            }

            return residents;
        }

        public ResidentModel GetById(int id)
        {
            string query = @"
                SELECT r.*, room.RoomNumber 
                FROM Resident r
                LEFT JOIN RoomInformation room ON r.RoomId = room.RoomID
                WHERE r.ResidentId = @Id";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ResidentModel
                            {
                                ResidentId = reader.GetInt32("ResidentId"),
                                FullName = reader.GetString("FullName"),
                                Gender = reader.GetString("Gender"),
                                IdentityCard = reader.GetString("IdentityCard"),
                                BirthYear = reader.GetDateTime("BirthYear"),
                                PhoneNumber = reader.IsDBNull("PhoneNumber") ? "" : reader.GetString("PhoneNumber"),
                                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                                Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                                RoomId = reader.GetInt32("RoomId"),
                                RoomNumber = reader.IsDBNull("RoomNumber") ? "" : reader.GetString("RoomNumber"),
                                CheckInDate = reader.GetDateTime("CheckInDate"),
                                CheckOutDate = reader.IsDBNull("CheckOutDate") ? null : reader.GetDateTime("CheckOutDate"),
                                Status = reader.GetInt32("Status"),
                                Notes = reader.IsDBNull("Notes") ? "" : reader.GetString("Notes")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving resident: {ex.Message}", ex);
            }

            return null;
        }

        public void Add(ResidentModel resident)
        {
            string query = @"
                INSERT INTO Resident (FullName, Gender, IdentityCard, BirthYear, PhoneNumber, Email, Address, 
                                    RoomId, CheckInDate, Status, Notes)
                VALUES (@FullName, @Gender, @IdentityCard, @BirthYear, @PhoneNumber, @Email, @Address, 
                        @RoomId, @CheckInDate, @Status, @Notes)";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", resident.FullName);
                    cmd.Parameters.AddWithValue("@Gender", resident.Gender);
                    cmd.Parameters.AddWithValue("@IdentityCard", resident.IdentityCard);
                    cmd.Parameters.AddWithValue("@BirthYear", resident.BirthYear);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object)resident.PhoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)resident.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)resident.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoomId", resident.RoomId);
                    cmd.Parameters.AddWithValue("@CheckInDate", resident.CheckInDate);
                    cmd.Parameters.AddWithValue("@Status", resident.Status);
                    cmd.Parameters.AddWithValue("@Notes", (object)resident.Notes ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding resident: {ex.Message}", ex);
            }
        }

        public void Update(ResidentModel resident)
        {
            string query = @"
                UPDATE Resident 
                SET FullName = @FullName, Gender = @Gender, IdentityCard = @IdentityCard, 
                    BirthYear = @BirthYear, PhoneNumber = @PhoneNumber, Email = @Email, 
                    Address = @Address, RoomId = @RoomId, CheckInDate = @CheckInDate,
                    CheckOutDate = @CheckOutDate, Status = @Status, Notes = @Notes
                WHERE ResidentId = @ResidentId";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ResidentId", resident.ResidentId);
                    cmd.Parameters.AddWithValue("@FullName", resident.FullName);
                    cmd.Parameters.AddWithValue("@Gender", resident.Gender);
                    cmd.Parameters.AddWithValue("@IdentityCard", resident.IdentityCard);
                    cmd.Parameters.AddWithValue("@BirthYear", resident.BirthYear);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object)resident.PhoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)resident.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)resident.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RoomId", resident.RoomId);
                    cmd.Parameters.AddWithValue("@CheckInDate", resident.CheckInDate);
                    cmd.Parameters.AddWithValue("@CheckOutDate", (object)resident.CheckOutDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", resident.Status);
                    cmd.Parameters.AddWithValue("@Notes", (object)resident.Notes ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating resident: {ex.Message}", ex);
            }
        }

        public void Delete(int id)
        {
            string query = "UPDATE Resident SET Status = 2, CheckOutDate = GETDATE() WHERE ResidentId = @Id";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing resident: {ex.Message}", ex);
            }
        }

        public List<ResidentModel> Search(string searchText)
        {
            var residents = new List<ResidentModel>();
            string query = @"
                SELECT r.*, room.RoomNumber 
                FROM Resident r
                LEFT JOIN RoomInformation room ON r.RoomId = room.RoomID
                WHERE r.Status = 1 AND (
                    r.FullName LIKE @SearchText OR 
                    r.IdentityCard LIKE @SearchText OR 
                    r.PhoneNumber LIKE @SearchText OR
                    room.RoomNumber LIKE @SearchText
                )
                ORDER BY r.FullName";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchText", $"%{searchText}%");
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            residents.Add(new ResidentModel
                            {
                                ResidentId = reader.GetInt32("ResidentId"),
                                FullName = reader.GetString("FullName"),
                                Gender = reader.GetString("Gender"),
                                IdentityCard = reader.GetString("IdentityCard"),
                                BirthYear = reader.GetDateTime("BirthYear"),
                                PhoneNumber = reader.IsDBNull("PhoneNumber") ? "" : reader.GetString("PhoneNumber"),
                                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                                Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                                RoomId = reader.GetInt32("RoomId"),
                                RoomNumber = reader.IsDBNull("RoomNumber") ? "" : reader.GetString("RoomNumber"),
                                CheckInDate = reader.GetDateTime("CheckInDate"),
                                CheckOutDate = reader.IsDBNull("CheckOutDate") ? null : reader.GetDateTime("CheckOutDate"),
                                Status = reader.GetInt32("Status"),
                                Notes = reader.IsDBNull("Notes") ? "" : reader.GetString("Notes")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching residents: {ex.Message}", ex);
            }

            return residents;
        }

        public List<RoomInformationModel> GetAvailableRooms()
        {
            var rooms = new List<RoomInformationModel>();
            string query = "SELECT * FROM RoomInformation WHERE RoomStatus = 1 ORDER BY RoomNumber";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rooms.Add(new RoomInformationModel
                            {
                                RoomID = reader.GetInt32("RoomID"),
                                RoomNumber = reader.GetString("RoomNumber"),
                                RoomDetailDescription = reader.IsDBNull("RoomDetailDescription") ? "" : reader.GetString("RoomDetailDescription"),
                                RoomMaxCapacity = reader.GetInt32("RoomMaxCapacity"),
                                RoomStatus = reader.GetInt32("RoomStatus"),
                                RoomPricePerDay = reader.GetDecimal("RoomPricePerDay"),
                                RoomTypeID = reader.GetInt32("RoomTypeID")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving available rooms: {ex.Message}", ex);
            }

            return rooms;
        }
    }
}
