using System;
using System.IO;
using System.Windows;
using AnLaNPWPF.Views;
using AnLaNPWPF.Helpers;

namespace AnLaNPWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Add global exception handlers
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            base.OnStartup(e);
            
            // Check database connection before starting the application
            if (!StartupChecker.CheckDatabaseConnection())
            {
                // Database check failed, application will close
                this.Shutdown(1);
                return;
            }
            
            //// Show the login window on startup
            //LoginWindow loginWindow = new LoginWindow();
            //loginWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);
            MessageBox.Show($"Lỗi ứng dụng: {e.Exception.Message}\n\nVui lòng kiểm tra file log để biết thêm chi tiết.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject as Exception);
            MessageBox.Show($"Lỗi nghiêm trọng: {(e.ExceptionObject as Exception)?.Message}\n\nỨng dụng sẽ đóng.", "Lỗi Nghiêm Trọng", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LogException(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex?.ToString()}\n\n";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}
