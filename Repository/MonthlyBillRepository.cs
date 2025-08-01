using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BusinessObject;

namespace Repository
{
    public class MonthlyBillRepository
    {
        private readonly string connectionString;

        public MonthlyBillRepository()
        {
            connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement");
        }

        public List<MonthlyBill> GetAll()
        {
            var bills = new List<MonthlyBill>();
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Simple query first to debug
                string query = @"SELECT COUNT(*) FROM MonthlyBill";
                
                using (var command = new SqlCommand(query, connection))
                {
                    var count = command.ExecuteScalar();
                    if (count == null || Convert.ToInt32(count) == 0)
                    {
                        // No bills in database, return empty list
                        return bills;
                    }
                }
                
                // If we have bills, try to load them with JOIN to get customer and room info
                query = @"SELECT mb.BillId, mb.ContractId, mb.BillMonth, mb.BillYear, mb.RoomRent, 
                                mb.ElectricityUsage, mb.ElectricityRate, mb.ElectricityCost,
                                mb.WaterUsage, mb.WaterRate, mb.WaterCost, mb.OtherCharges, 
                                mb.TotalAmount, mb.DueDate, mb.BillStatus, mb.CreatedAt, mb.UpdatedAt,
                                mb.PaidDate, mb.PaidAmount,
                                c.ContractNumber, cust.CustomerFullName, r.RoomNumber
                         FROM MonthlyBill mb
                         INNER JOIN RentalContract c ON mb.ContractId = c.ContractId
                         INNER JOIN Customer cust ON c.CustomerId = cust.CustomerId
                         INNER JOIN RoomInformation r ON c.RoomId = r.RoomId
                         ORDER BY mb.BillYear DESC, mb.BillMonth DESC, mb.CreatedAt DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var bill = MapReaderToBillSimple(reader);
                                
                                // Set additional display values
                                bill.SetDisplayValues(
                                    reader["CustomerFullName"]?.ToString() ?? "Chưa có",
                                    reader["RoomNumber"]?.ToString() ?? "Chưa có", 
                                    reader["ContractNumber"]?.ToString() ?? "Chưa có"
                                );
                                
                                bills.Add(bill);
                            }
                            catch (Exception ex)
                            {
                                // Skip this record and continue
                                System.Diagnostics.Debug.WriteLine($"Error mapping bill record: {ex.Message}");
                                continue;
                            }
                        }
                    }
                }
            }

            return bills;
        }

        public List<MonthlyBill> GetBillsByContract(int contractId)
        {
            var bills = new List<MonthlyBill>();
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string query = @"SELECT mb.BillId, mb.ContractId, mb.BillMonth, mb.BillYear, mb.RoomRent, 
                                       mb.ElectricityUsage, mb.ElectricityRate, mb.ElectricityCost,
                                       mb.WaterUsage, mb.WaterRate, mb.WaterCost, mb.OtherCharges, 
                                       mb.TotalAmount, mb.DueDate, mb.BillStatus, mb.CreatedAt, mb.UpdatedAt,
                                       mb.PaidDate, mb.PaidAmount,
                                       c.ContractNumber, cust.CustomerFullName, r.RoomNumber
                                FROM MonthlyBill mb
                                INNER JOIN RentalContract c ON mb.ContractId = c.ContractId
                                INNER JOIN Customer cust ON c.CustomerId = cust.CustomerId
                                INNER JOIN RoomInformation r ON c.RoomId = r.RoomId
                                WHERE mb.ContractId = @ContractId
                                ORDER BY mb.BillYear DESC, mb.BillMonth DESC, mb.CreatedAt DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", contractId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var bill = MapReaderToBillSimple(reader);
                                
                                // Set additional display values
                                bill.SetDisplayValues(
                                    reader["CustomerFullName"]?.ToString() ?? "Chưa có",
                                    reader["RoomNumber"]?.ToString() ?? "Chưa có", 
                                    reader["ContractNumber"]?.ToString() ?? "Chưa có"
                                );
                                
                                bills.Add(bill);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error mapping bill record: {ex.Message}");
                                continue;
                            }
                        }
                    }
                }
            }
            
            return bills;
        }

        public List<MonthlyBill> GetBillsByStatus(BillStatus status)
        {
            return GetAll().Where(b => b.BillStatus == status).ToList();
        }

        public List<MonthlyBill> GetOverdueBills()
        {
            return GetAll().Where(b => b.IsOverdue).ToList();
        }

        public List<MonthlyBill> GetBillsByPeriod(int month, int year)
        {
            return GetAll().Where(b => b.BillMonth == month && b.BillYear == year).ToList();
        }

        public List<MonthlyBill> GetUnpaidBills()
        {
            return GetAll().Where(b => b.BillStatus == BillStatus.Pending || b.BillStatus == BillStatus.Overdue || b.BillStatus == BillStatus.Partial).ToList();
        }

        public MonthlyBill GetById(int billId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT mb.*, c.ContractNumber, cust.CustomerFullName, r.RoomNumber 
                    FROM MonthlyBill mb
                    INNER JOIN RentalContract c ON mb.ContractId = c.ContractId
                    INNER JOIN Customer cust ON c.CustomerId = cust.CustomerId
                    INNER JOIN RoomInformation r ON c.RoomId = r.RoomId
                    WHERE mb.BillId = @BillId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BillId", billId);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToBill(reader);
                        }
                    }
                }
            }

            return null;
        }

        public MonthlyBill GetBillByContractAndPeriod(int contractId, int month, int year)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT mb.*, c.ContractNumber, cust.CustomerFullName, r.RoomNumber 
                    FROM MonthlyBill mb
                    INNER JOIN RentalContract c ON mb.ContractId = c.ContractId
                    INNER JOIN Customer cust ON c.CustomerId = cust.CustomerId
                    INNER JOIN RoomInformation r ON c.RoomId = r.RoomId
                    WHERE mb.ContractId = @ContractId AND mb.BillMonth = @Month AND mb.BillYear = @Year";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", contractId);
                    command.Parameters.AddWithValue("@Month", month);
                    command.Parameters.AddWithValue("@Year", year);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToBill(reader);
                        }
                    }
                }
            }

            return null;
        }

        public bool Add(MonthlyBill bill)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO MonthlyBill 
                        (ContractId, BillMonth, BillYear, RoomRent, ElectricityUsage, ElectricityRate, 
                         ElectricityCost, WaterUsage, WaterRate, WaterCost, OtherCharges, TotalAmount, 
                         BillStatus, DueDate, PaidDate, PaidAmount, CreatedAt, UpdatedAt)
                        VALUES 
                        (@ContractId, @BillMonth, @BillYear, @RoomRent, @ElectricityUsage, @ElectricityRate, 
                         @ElectricityCost, @WaterUsage, @WaterRate, @WaterCost, @OtherCharges, @TotalAmount, 
                         @BillStatus, @DueDate, @PaidDate, @PaidAmount, @CreatedAt, @UpdatedAt);
                        
                        SELECT SCOPE_IDENTITY();";

                    using (var command = new SqlCommand(query, connection))
                    {
                        AddBillParameters(command, bill);
                        
                        var newId = command.ExecuteScalar();
                        if (newId != null)
                        {
                            bill.BillId = Convert.ToInt32(newId);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding bill: {ex.Message}");
            }

            return false;
        }

        public bool Update(MonthlyBill bill)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE MonthlyBill SET
                            ContractId = @ContractId,
                            BillMonth = @BillMonth,
                            BillYear = @BillYear,
                            RoomRent = @RoomRent,
                            ElectricityUsage = @ElectricityUsage,
                            ElectricityRate = @ElectricityRate,
                            ElectricityCost = @ElectricityCost,
                            WaterUsage = @WaterUsage,
                            WaterRate = @WaterRate,
                            WaterCost = @WaterCost,
                            OtherCharges = @OtherCharges,
                            TotalAmount = @TotalAmount,
                            BillStatus = @BillStatus,
                            DueDate = @DueDate,
                            PaidDate = @PaidDate,
                            PaidAmount = @PaidAmount,
                            UpdatedAt = @UpdatedAt
                        WHERE BillId = @BillId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        AddBillParameters(command, bill);
                        command.Parameters.AddWithValue("@BillId", bill.BillId);
                        
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating bill: {ex.Message}");
            }

            return false;
        }

        public bool Delete(int billId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM MonthlyBill WHERE BillId = @BillId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BillId", billId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting bill: {ex.Message}");
            }

            return false;
        }

        public int GenerateMonthlyBillsForActiveContracts(int month, int year)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        INSERT INTO MonthlyBill (ContractId, BillMonth, BillYear, RoomRent, TotalAmount, DueDate, CreatedAt, UpdatedAt)
                        SELECT 
                            c.ContractId,
                            @Month,
                            @Year,
                            c.MonthlyRent,
                            c.MonthlyRent,
                            DATEFROMPARTS(@Year, @Month, 5),
                            GETDATE(),
                            GETDATE()
                        FROM RentalContract c
                        WHERE c.ContractStatus = 'Active'
                        AND NOT EXISTS (
                            SELECT 1 FROM MonthlyBill mb 
                            WHERE mb.ContractId = c.ContractId 
                            AND mb.BillMonth = @Month 
                            AND mb.BillYear = @Year
                        )";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Month", month);
                        command.Parameters.AddWithValue("@Year", year);
                        
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating monthly bills: {ex.Message}");
            }

            return 0;
        }

        public bool MarkAsPaid(int billId, decimal amount, DateTime? paymentDate = null)
        {
            try
            {
                var bill = GetById(billId);
                if (bill == null) return false;

                bill.MarkAsPaid(amount, paymentDate);
                return Update(bill);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking bill as paid: {ex.Message}");
            }

            return false;
        }

        public int MarkOverdueBills()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        UPDATE MonthlyBill 
                        SET BillStatus = 'Overdue', UpdatedAt = GETDATE()
                        WHERE BillStatus = 'Pending' AND DueDate < GETDATE()";

                    using (var command = new SqlCommand(query, connection))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking overdue bills: {ex.Message}");
            }

            return 0;
        }

        public decimal GetTotalRevenueByPeriod(int month, int year)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT ISNULL(SUM(PaidAmount), 0) 
                        FROM MonthlyBill 
                        WHERE BillMonth = @Month AND BillYear = @Year 
                        AND BillStatus IN ('Paid', 'Partial')";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Month", month);
                        command.Parameters.AddWithValue("@Year", year);
                        
                        var result = command.ExecuteScalar();
                        return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting revenue: {ex.Message}");
            }

            return 0;
        }

        public List<MonthlyBill> GetBillsByMonthYear(int month, int year)
        {
            var bills = new List<MonthlyBill>();
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string query = @"SELECT BillId, ContractId, BillMonth, BillYear, RoomRent, 
                                ElectricityUsage, ElectricityRate, ElectricityCost,
                                WaterUsage, WaterRate, WaterCost, OtherCharges, 
                                TotalAmount, DueDate, BillStatus, CreatedAt, UpdatedAt,
                                PaidDate, PaidAmount
                         FROM MonthlyBill 
                         WHERE BillMonth = @Month AND BillYear = @Year
                         ORDER BY CreatedAt DESC";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Month", month);
                    command.Parameters.AddWithValue("@Year", year);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var bill = MapReaderToBillSimple(reader);
                                bills.Add(bill);
                            }
                            catch (Exception ex)
                            {
                                // Log error but continue reading other bills
                                Console.WriteLine($"Error reading bill: {ex.Message}");
                                continue;
                            }
                        }
                    }
                }
            }
            
            return bills;
        }

        private void AddBillParameters(SqlCommand command, MonthlyBill bill)
        {
            command.Parameters.AddWithValue("@ContractId", bill.ContractId);
            command.Parameters.AddWithValue("@BillMonth", bill.BillMonth);
            command.Parameters.AddWithValue("@BillYear", bill.BillYear);
            command.Parameters.AddWithValue("@RoomRent", bill.RoomRent);
            command.Parameters.AddWithValue("@ElectricityUsage", bill.ElectricityUsage);
            command.Parameters.AddWithValue("@ElectricityRate", bill.ElectricityRate);
            command.Parameters.AddWithValue("@ElectricityCost", bill.ElectricityCost);
            command.Parameters.AddWithValue("@WaterUsage", bill.WaterUsage);
            command.Parameters.AddWithValue("@WaterRate", bill.WaterRate);
            command.Parameters.AddWithValue("@WaterCost", bill.WaterCost);
            command.Parameters.AddWithValue("@OtherCharges", bill.OtherCharges);
            command.Parameters.AddWithValue("@TotalAmount", bill.TotalAmount);
            command.Parameters.AddWithValue("@BillStatus", bill.BillStatus.ToString());
            command.Parameters.AddWithValue("@DueDate", bill.DueDate);
            command.Parameters.AddWithValue("@PaidDate", (object)bill.PaidDate ?? DBNull.Value);
            command.Parameters.AddWithValue("@PaidAmount", (object)bill.PaidAmount ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedAt", bill.CreatedAt);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
        }

        private MonthlyBill MapReaderToBill(SqlDataReader reader)
        {
            var bill = new MonthlyBill();
            
            try
            {
                bill.BillId = reader.GetInt32(0); // "BillId"
                bill.ContractId = reader.GetInt32(1); // "ContractId"
                bill.BillMonth = reader.GetInt32(2); // "BillMonth"
                bill.BillYear = reader.GetInt32(3); // "BillYear"
                bill.RoomRent = reader.GetDecimal(4); // "RoomRent"
                bill.ElectricityUsage = reader.GetDecimal(5); // "ElectricityUsage"
                bill.ElectricityRate = reader.GetDecimal(6); // "ElectricityRate"
                bill.ElectricityCost = reader.GetDecimal(7); // "ElectricityCost"
                bill.WaterUsage = reader.GetDecimal(8); // "WaterUsage"
                bill.WaterRate = reader.GetDecimal(9); // "WaterRate"
                bill.WaterCost = reader.GetDecimal(10); // "WaterCost"
                bill.OtherCharges = reader.GetDecimal(11); // "OtherCharges"
                bill.TotalAmount = reader.GetDecimal(12); // "TotalAmount"
                bill.DueDate = reader.GetDateTime(13); // "DueDate"
                bill.CreatedAt = reader.GetDateTime(14); // "CreatedAt"
                bill.UpdatedAt = reader.GetDateTime(15); // "UpdatedAt"

                // Parse status safely
                var statusValue = reader[16]; // "BillStatus"
                if (statusValue != null && statusValue != DBNull.Value)
                {
                    var statusStr = statusValue.ToString();
                    if (int.TryParse(statusStr, out var statusInt))
                    {
                        // If it's an integer, cast it directly
                        bill.BillStatus = (BillStatus)statusInt;
                    }
                    else if (Enum.TryParse<BillStatus>(statusStr, true, out var status))
                    {
                        // If it's a string, try to parse
                        bill.BillStatus = status;
                    }
                    else
                    {
                        // Default to Pending if cannot parse
                        bill.BillStatus = BillStatus.Pending;
                    }
                }
                else
                {
                    bill.BillStatus = BillStatus.Pending;
                }

                // Handle nullable fields safely
                try
                {
                    if (reader.FieldCount > 17 && !reader.IsDBNull(17))
                        bill.PaidDate = reader.GetDateTime(17); // "PaidDate"
                }
                catch { /* Ignore if column doesn't exist */ }
                    
                try
                {
                    if (reader.FieldCount > 18 && !reader.IsDBNull(18))
                        bill.PaidAmount = reader.GetDecimal(18); // "PaidAmount"
                }
                catch { /* Ignore if column doesn't exist */ }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error mapping bill data: {ex.Message}", ex);
            }
            
            return bill;
        }

        private MonthlyBill MapReaderToBillSimple(SqlDataReader reader)
        {
            var bill = new MonthlyBill();
            
            try
            {
                // Use column names instead of indices for safety
                bill.BillId = Convert.ToInt32(reader["BillId"]);
                bill.ContractId = Convert.ToInt32(reader["ContractId"]);
                bill.BillMonth = Convert.ToInt32(reader["BillMonth"]);
                bill.BillYear = Convert.ToInt32(reader["BillYear"]);
                bill.RoomRent = Convert.ToDecimal(reader["RoomRent"]);
                bill.ElectricityUsage = Convert.ToDecimal(reader["ElectricityUsage"]);
                bill.ElectricityRate = Convert.ToDecimal(reader["ElectricityRate"]);
                bill.ElectricityCost = Convert.ToDecimal(reader["ElectricityCost"]);
                bill.WaterUsage = Convert.ToDecimal(reader["WaterUsage"]);
                bill.WaterRate = Convert.ToDecimal(reader["WaterRate"]);
                bill.WaterCost = Convert.ToDecimal(reader["WaterCost"]);
                bill.OtherCharges = Convert.ToDecimal(reader["OtherCharges"]);
                bill.TotalAmount = Convert.ToDecimal(reader["TotalAmount"]);
                bill.DueDate = Convert.ToDateTime(reader["DueDate"]);
                bill.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
                bill.UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]);

                // Handle BillStatus safely
                var statusValue = reader["BillStatus"];
                if (statusValue != null && statusValue != DBNull.Value)
                {
                    var statusStr = statusValue.ToString();
                    
                    // Try parsing as integer first
                    if (int.TryParse(statusStr, out var statusInt))
                    {
                        if (Enum.IsDefined(typeof(BillStatus), statusInt))
                        {
                            bill.BillStatus = (BillStatus)statusInt;
                        }
                        else
                        {
                            bill.BillStatus = BillStatus.Pending;
                        }
                    }
                    // Try parsing as string
                    else if (Enum.TryParse<BillStatus>(statusStr, true, out var status))
                    {
                        bill.BillStatus = status;
                    }
                    else
                    {
                        bill.BillStatus = BillStatus.Pending;
                    }
                }
                else
                {
                    bill.BillStatus = BillStatus.Pending;
                }

                // Handle nullable fields
                if (reader["PaidDate"] != DBNull.Value)
                {
                    bill.PaidDate = Convert.ToDateTime(reader["PaidDate"]);
                }
                    
                if (reader["PaidAmount"] != DBNull.Value)
                {
                    bill.PaidAmount = Convert.ToDecimal(reader["PaidAmount"]);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error mapping bill ID {reader["BillId"]}: {ex.Message}");
            }
            
            return bill;
        }
    }
}
