using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Repository
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
                // Look for appsettings.json in the application's base directory
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _connectionStrings = ParseConnectionStrings(json);
                }
                else
                {
                    // Fallback: use default connection string
                    _connectionStrings = new Dictionary<string, string>
                    {
                        ["FUMiniHotelManagement"] = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=FUMiniHotelManagement;Integrated Security=True;Connect Timeout=30"
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load configuration", ex);
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
                    return _connectionStrings[name];
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
