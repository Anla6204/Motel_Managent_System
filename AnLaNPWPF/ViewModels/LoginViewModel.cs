using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AnLaNPWPF.Helpers;
using AnLaNPWPF.Views;

namespace AnLaNPWPF.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string email;
        private readonly PasswordBox passwordBox;

        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(PasswordBox pwdBox)
        {
            passwordBox = pwdBox;
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            string password = passwordBox.Password;
            
            if (Email == "admin" && password == "123")
            {
                MessageDialogHelper.ShowMessage("Đăng nhập Admin thành công!", "Thông báo đăng nhập", MessageType.Success);
                
                // Navigate to Admin Dashboard
                AdminDashboard adminDashboard = new AdminDashboard();
                adminDashboard.Show();
                CloseLoginWindow();
            }
            else
            {
                MessageDialogHelper.ShowMessage("Tên đăng nhập hoặc mật khẩu không đúng!", 
                    "Lỗi đăng nhập", MessageType.Error);
            }
        }

        private void CloseLoginWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);
        public void Execute(object parameter) => execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
