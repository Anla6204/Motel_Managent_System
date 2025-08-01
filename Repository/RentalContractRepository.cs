using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BusinessObject;

namespace Repository
{
    public class RentalContractRepository
    {
        private readonly string connectionString;

        public RentalContractRepository()
        {
            connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement");
        }

        public List<RentalContract> GetAll()
        {
            var contracts = new List<RentalContract>();
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT c.*, cust.CustomerFullName, r.RoomNumber 
                    FROM RentalContract c
                    LEFT JOIN Customer cust ON c.CustomerId = cust.CustomerId
                    LEFT JOIN RoomInformation r ON c.RoomId = r.RoomId
                    ORDER BY c.CreatedAt DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            contracts.Add(MapReaderToContract(reader));
                        }
                    }
                }
            }

            return contracts;
        }

        public List<RentalContract> GetActiveContracts()
        {
            return GetAll().Where(c => c.ContractStatus == ContractStatus.Active).ToList();
        }

        public List<RentalContract> GetExpiringContracts(int daysAhead = 30)
        {
            return GetActiveContracts().Where(c => c.DaysRemaining <= daysAhead && c.DaysRemaining > 0).ToList();
        }

        public List<RentalContract> GetExpiredContracts()
        {
            return GetAll().Where(c => c.IsExpired).ToList();
        }

        public List<RentalContract> GetContractsByCustomer(int customerId)
        {
            return GetAll().Where(c => c.CustomerId == customerId).ToList();
        }

        public List<RentalContract> GetContractsByRoom(int roomId)
        {
            return GetAll().Where(c => c.RoomId == roomId).ToList();
        }

        public RentalContract GetById(int contractId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT c.*, cust.CustomerFullName, r.RoomNumber 
                    FROM RentalContract c
                    LEFT JOIN Customer cust ON c.CustomerId = cust.CustomerId
                    LEFT JOIN RoomInformation r ON c.RoomId = r.RoomId
                    WHERE c.ContractId = @ContractId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", contractId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToContract(reader);
                        }
                    }
                }
            }

            return null;
        }

        public bool Add(RentalContract contract)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO RentalContract 
                        (CustomerId, RoomId, ContractNumber, StartDate, EndDate, MonthlyRent, 
                         SecurityDeposit, ContractStatus, ContractTerms, CreatedAt, UpdatedAt)
                        VALUES 
                        (@CustomerId, @RoomId, @ContractNumber, @StartDate, @EndDate, @MonthlyRent, 
                         @SecurityDeposit, @ContractStatus, @ContractTerms, @CreatedAt, @UpdatedAt);
                        
                        SELECT SCOPE_IDENTITY();";

                    using (var command = new SqlCommand(query, connection))
                    {
                        AddContractParameters(command, contract);
                        
                        var newId = command.ExecuteScalar();
                        if (newId != null)
                        {
                            contract.ContractId = Convert.ToInt32(newId);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding contract: {ex.Message}");
            }

            return false;
        }

        public bool Update(RentalContract contract)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE RentalContract SET
                            CustomerId = @CustomerId,
                            RoomId = @RoomId,
                            ContractNumber = @ContractNumber,
                            StartDate = @StartDate,
                            EndDate = @EndDate,
                            MonthlyRent = @MonthlyRent,
                            SecurityDeposit = @SecurityDeposit,
                            ContractStatus = @ContractStatus,
                            ContractTerms = @ContractTerms,
                            UpdatedAt = @UpdatedAt
                        WHERE ContractId = @ContractId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        AddContractParameters(command, contract);
                        command.Parameters.AddWithValue("@ContractId", contract.ContractId);
                        
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating contract: {ex.Message}");
            }

            return false;
        }

        public bool Delete(int contractId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM RentalContract WHERE ContractId = @ContractId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ContractId", contractId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting contract: {ex.Message}");
            }

            return false;
        }

        public int ExpireOutdatedContracts()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // 1. Lấy danh sách phòng sẽ bị ảnh hưởng
                            string getRoomsQuery = @"
                                SELECT DISTINCT RoomId 
                                FROM RentalContract 
                                WHERE ContractStatus = 'Active' AND EndDate < GETDATE()";

                            var roomIds = new List<int>();
                            using (var getRoomsCommand = new SqlCommand(getRoomsQuery, connection, transaction))
                            using (var reader = getRoomsCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    roomIds.Add(reader.GetInt32(0));
                                }
                            }

                            // 2. Cập nhật trạng thái hợp đồng
                            string updateContractsQuery = @"
                                UPDATE RentalContract 
                                SET ContractStatus = 'Expired', UpdatedAt = GETDATE()
                                WHERE ContractStatus = 'Active' AND EndDate < GETDATE()";

                            int contractsUpdated;
                            using (var updateContractsCommand = new SqlCommand(updateContractsQuery, connection, transaction))
                            {
                                contractsUpdated = updateContractsCommand.ExecuteNonQuery();
                            }

                            // 3. Xóa khách hàng khỏi các phòng và cập nhật trạng thái phòng
                            foreach (var roomId in roomIds)
                            {
                                // Xóa khách hàng khỏi phòng
                                string clearCustomersQuery = @"
                                    UPDATE Customer
                                    SET CurrentRoomID = NULL, 
                                        CheckInDate = NULL
                                    WHERE CurrentRoomID = @RoomID";

                                using (var clearCommand = new SqlCommand(clearCustomersQuery, connection, transaction))
                                {
                                    clearCommand.Parameters.AddWithValue("@RoomID", roomId);
                                    clearCommand.ExecuteNonQuery();
                                }

                                // Cập nhật trạng thái phòng về trống
                                string updateRoomQuery = @"
                                    UPDATE RoomInformation
                                    SET RoomStatus = 0
                                    WHERE RoomID = @RoomID";

                                using (var updateRoomCommand = new SqlCommand(updateRoomQuery, connection, transaction))
                                {
                                    updateRoomCommand.Parameters.AddWithValue("@RoomID", roomId);
                                    updateRoomCommand.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return contractsUpdated;
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
                System.Diagnostics.Debug.WriteLine($"Error expiring contracts: {ex.Message}");
                throw new Exception($"Lỗi khi cập nhật hợp đồng hết hạn: {ex.Message}");
            }
        }

        public bool IsRoomAvailable(int roomId, DateTime startDate, DateTime endDate, int? excludeContractId = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT COUNT(*) FROM RentalContract 
                    WHERE RoomId = @RoomId 
                    AND ContractStatus IN ('Active', 'Pending')
                    AND (
                        (@StartDate BETWEEN StartDate AND EndDate) OR
                        (@EndDate BETWEEN StartDate AND EndDate) OR
                        (StartDate BETWEEN @StartDate AND @EndDate)
                    )";

                if (excludeContractId.HasValue)
                {
                    query += " AND ContractId != @ExcludeContractId";
                }

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RoomId", roomId);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    
                    if (excludeContractId.HasValue)
                    {
                        command.Parameters.AddWithValue("@ExcludeContractId", excludeContractId.Value);
                    }

                    var count = (int)command.ExecuteScalar();
                    return count == 0;
                }
            }
        }

        private void AddContractParameters(SqlCommand command, RentalContract contract)
        {
            command.Parameters.AddWithValue("@CustomerId", contract.CustomerId);
            command.Parameters.AddWithValue("@RoomId", contract.RoomId);
            command.Parameters.AddWithValue("@ContractNumber", contract.ContractNumber ?? "");
            command.Parameters.AddWithValue("@StartDate", contract.StartDate);
            command.Parameters.AddWithValue("@EndDate", contract.EndDate);
            command.Parameters.AddWithValue("@MonthlyRent", contract.MonthlyRent);
            command.Parameters.AddWithValue("@SecurityDeposit", contract.SecurityDeposit);
            command.Parameters.AddWithValue("@ContractStatus", contract.ContractStatus.ToString());
            command.Parameters.AddWithValue("@ContractTerms", contract.ContractTerms ?? "");
            command.Parameters.AddWithValue("@CreatedAt", contract.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
        }

        private RentalContract MapReaderToContract(SqlDataReader reader)
        {
            var contract = new RentalContract
            {
                ContractId = reader.GetInt32(0), // "ContractId"
                CustomerId = reader.GetInt32(1), // "CustomerId"  
                RoomId = reader.GetInt32(2), // "RoomId"
                ContractNumber = reader.GetString(3), // "ContractNumber"
                StartDate = reader.GetDateTime(4), // "StartDate"
                EndDate = reader.GetDateTime(5), // "EndDate"
                MonthlyRent = reader.GetDecimal(6), // "MonthlyRent"
                SecurityDeposit = reader.GetDecimal(7), // "SecurityDeposit"
                ContractTerms = reader.IsDBNull(9) ? "" : reader.GetString(9), // "ContractTerms" - index 9
                CreatedAt = reader.GetDateTime(10), // "CreatedAt" - index 10
                UpdatedAt = reader.GetDateTime(11) // "UpdatedAt" - index 11
            };

            // Parse status - ContractStatus is at index 8
            var statusStr = reader.GetString(8); // "ContractStatus"
            if (Enum.TryParse<ContractStatus>(statusStr, out var status))
            {
                contract.ContractStatus = status;
            }

            // Set navigation properties if available
            if (reader.FieldCount > 12 && !reader.IsDBNull(12))
            {
                contract.Customer = new CustomerModel
                {
                    CustomerID = contract.CustomerId,
                    CustomerFullName = reader.GetString(12) // "CustomerFullName"
                };
            }

            if (reader.FieldCount > 13 && !reader.IsDBNull(13))
            {
                contract.Room = new RoomInformationModel
                {
                    RoomID = contract.RoomId,
                    RoomNumber = reader.GetString(13) // "RoomNumber"
                };
            }

            return contract;
        }
    }
}
