using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AnLaNPWPF
{
    public static class ConfigurationHelper
    {
        private static Dictionary<string, string> _connectionStrings;

        static ConfigurationHelper()
        {
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _connectionStrings = ParseConnectionStrings(json);
                }
                else
                {
                    // Fallback: try to use a default connection string
                    _connectionStrings = new Dictionary<string, string>
                    {
                        ["FUMiniHotelManagement"] = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database\\FUMiniHotelManagement.mdf;Initial Catalog=FUMiniHotelManagement_Local;Integrated Security=True;Connect Timeout=30"
                    };
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to load configuration: {ex.Message}");
                throw new InvalidOperationException("Failed to load configuration", ex);
            }
        }

        private static void LogError(string message)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_error.log");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private static Dictionary<string, string> ParseConnectionStrings(string json)
        {
            var connectionStrings = new Dictionary<string, string>();
            
            // Remove whitespace and newlines for easier parsing
            json = json.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
            
            // Simple JSON parsing for ConnectionStrings section - improved regex
            var connectionStringsMatch = Regex.Match(json, @"""ConnectionStrings""\s*:\s*\{([^}]*)\}");
            if (connectionStringsMatch.Success)
            {
                string connectionStringsContent = connectionStringsMatch.Groups[1].Value;
                
                // Updated regex to handle escaped backslashes in connection strings
                var matches = Regex.Matches(connectionStringsContent, @"""([^""]+)""\s*:\s*""([^""\\]*(?:\\.[^""\\]*)*)""");
                
                foreach (Match match in matches)
                {
                    string key = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    // Unescape the connection string
                    value = value.Replace("\\\\", "\\");
                    connectionStrings[key] = value;
                }
            }
            
            return connectionStrings;
        }

        public static string GetConnectionString(string name)
        {
            try
            {
                if (_connectionStrings == null)
                {
                    LoadConfiguration();
                }
                
                if (_connectionStrings != null && _connectionStrings.ContainsKey(name))
                {
                    string connectionString = _connectionStrings[name];
                    
                    // Replace |DataDirectory| token with actual application directory
                    if (connectionString.Contains("|DataDirectory|"))
                    {
                        string dataDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        // Ensure no double backslashes
                        if (dataDirectory.EndsWith("\\"))
                            dataDirectory = dataDirectory.TrimEnd('\\');
                        connectionString = connectionString.Replace("|DataDirectory|", dataDirectory);
                    }
                    
                    return connectionString;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get connection string '{name}'", ex);
            }
        }
    }
}
