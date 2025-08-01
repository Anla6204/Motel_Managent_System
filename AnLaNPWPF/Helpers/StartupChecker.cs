using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AnLaNPWPF.Helpers
{
    public static class StartupChecker
    {
        public static bool CheckDatabaseConnection()
        {
            try
            {
                string connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    LogError("Không thể lấy connection string từ cấu hình.");
                    return false;
                }

                // Try to connect to the database
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError($"Database connection failed: {ex.Message}");
                
                // Try to setup database automatically
                if (TrySetupDatabase())
                {
                    // Try connection again after setup
                    try
                    {
                        string connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement");
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            return true;
                        }
                    }
                    catch (Exception ex2)
                    {
                        LogError($"Database connection still failed after setup: {ex2.Message}");
                    }
                }
                
                // Show user-friendly error message
                MessageBox.Show(
                    "Không thể kết nối đến cơ sở dữ liệu.\n\n" +
                    "Vui lòng đảm bảo SQL Server LocalDB đã được cài đặt trên máy tính.\n\n" +
                    "Bạn có thể tải LocalDB từ trang web của Microsoft.",
                    "Lỗi Database", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                
                return false;
            }
        }

        private static bool TrySetupDatabase()
        {
            try
            {
                LogError("Attempting to setup database automatically...");
                
                // Start LocalDB instance if not running
                StartLocalDB();
                
                // Try to create database if it doesn't exist
                return CreateDatabase();
            }
            catch (Exception ex)
            {
                LogError($"Failed to setup database: {ex.Message}");
                return false;
            }
        }

        private static void StartLocalDB()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "sqllocaldb",
                    Arguments = "start MSSQLLocalDB",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    LogError($"LocalDB start result: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to start LocalDB: {ex.Message}");
            }
        }

        private static bool CreateDatabase()
        {
            try
            {
                // Get the database file path from connection string
                string connectionString = ConfigurationHelper.GetConnectionString("FUMiniHotelManagement");
                string dbFilePath = ExtractDbFilePathFromConnectionString(connectionString);
                
                if (string.IsNullOrEmpty(dbFilePath))
                {
                    LogError("Cannot extract database file path from connection string");
                    return false;
                }
                
                // Ensure database directory exists
                string dbDirectory = Path.GetDirectoryName(dbFilePath);
                if (!Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                    LogError($"Created database directory: {dbDirectory}");
                }
                
                // Check if database files exist
                if (File.Exists(dbFilePath))
                {
                    LogError("Database file already exists");
                    return true;
                }
                
                // Copy database files from project if they exist
                string sourceDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "FUMiniHotelManagement.mdf");
                string sourceLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "FUMiniHotelManagement_log.ldf");
                
                if (File.Exists(sourceDbPath))
                {
                    File.Copy(sourceDbPath, dbFilePath, true);
                    LogError($"Copied database file from {sourceDbPath} to {dbFilePath}");
                    
                    string logFilePath = Path.ChangeExtension(dbFilePath, ".ldf");
                    if (File.Exists(sourceLogPath))
                    {
                        File.Copy(sourceLogPath, logFilePath, true);
                        LogError($"Copied log file from {sourceLogPath} to {logFilePath}");
                    }
                    
                    return true;
                }
                else
                {
                    LogError($"Source database file not found: {sourceDbPath}");
                    
                    // Try to create an empty database using master connection
                    return CreateEmptyDatabase(dbFilePath);
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create database: {ex.Message}");
                return false;
            }
        }
        
        private static string ExtractDbFilePathFromConnectionString(string connectionString)
        {
            try
            {
                if (connectionString.Contains("AttachDbFilename="))
                {
                    int startIndex = connectionString.IndexOf("AttachDbFilename=") + "AttachDbFilename=".Length;
                    int endIndex = connectionString.IndexOf(";", startIndex);
                    if (endIndex == -1) endIndex = connectionString.Length;
                    
                    return connectionString.Substring(startIndex, endIndex - startIndex);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error extracting db file path: {ex.Message}");
            }
            return null;
        }
        
        private static bool CreateEmptyDatabase(string dbFilePath)
        {
            try
            {
                string masterConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30";
                
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();
                    
                    string createDbQuery = $@"
                        CREATE DATABASE [FUMiniHotelManagement] 
                        ON (NAME = 'FUMiniHotelManagement', FILENAME = '{dbFilePath}')";
                    
                    using (var cmd = new SqlCommand(createDbQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                        LogError($"Empty database created at: {dbFilePath}");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create empty database: {ex.Message}");
                return false;
            }
        }

        private static void LogError(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_error.log");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
