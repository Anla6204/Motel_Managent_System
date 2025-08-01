using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BusinessObject;
using Repository;
using AnLaNPWPF.Views;
using AnLaNPWPF.Helpers;

namespace AnLaNPWPF.ViewModels
{
    public class ResidentManagementViewModel : INotifyPropertyChanged
    {
        private CustomerRepository _customerRepository;
        private RoomInfomationRepository _roomRepository;
        
        private ObservableCollection<CustomerModel> _residents;
        private CustomerModel _selectedResident;
        private string _searchText;

        public ObservableCollection<CustomerModel> Residents
        {
            get { return _residents; }
            set
            {
                _residents = value;
                OnPropertyChanged(nameof(Residents));
            }
        }

        public CustomerModel SelectedResident
        {
            get { return _selectedResident; }
            set
            {
                _selectedResident = value;
                OnPropertyChanged(nameof(SelectedResident));
                ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ViewDetailCommand).RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        // Commands
        public ICommand AddCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ViewDetailCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand ExportExcelCommand { get; private set; }

        public ResidentManagementViewModel()
        {
            _customerRepository = new CustomerRepository();
            _roomRepository = new RoomInfomationRepository();
            
            Residents = new ObservableCollection<CustomerModel>();
            
            InitializeCommands();
            LoadResidents();
        }

        private void InitializeCommands()
        {
            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteDelete);
            ViewDetailCommand = new RelayCommand(ExecuteViewDetail, CanExecuteViewDetail);
            SearchCommand = new RelayCommand(ExecuteSearch);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            ExportExcelCommand = new RelayCommand(ExecuteExportExcel);
        }

        // Public method to refresh data - called from other views
        public void RefreshData()
        {
            LoadResidents();
        }

        private void LoadResidents()
        {
            try
            {
                var residents = _customerRepository.GetAll();
                Residents.Clear();
                foreach (var resident in residents)
                {
                    Residents.Add(resident);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách người ở: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAdd(object parameter)
        {
            try
            {
                var dialog = new ResidentDialog();
                if (dialog.ShowDialog() == true && dialog.IsSaved)
                {
                    _customerRepository.Add(dialog.Resident);
                    LoadResidents();
                    
                    MessageBox.Show("Thêm người ở thành công!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm người ở: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEdit(object parameter)
        {
            try
            {
                if (SelectedResident != null)
                {
                    var dialog = new ResidentDialog(SelectedResident);
                    if (dialog.ShowDialog() == true && dialog.IsSaved)
                    {
                        _customerRepository.Update(dialog.Resident);
                        LoadResidents();
                        
                        MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật thông tin: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            return SelectedResident != null;
        }

        private void ExecuteViewDetail(object parameter)
        {
            try
            {
                if (SelectedResident != null)
                {
                    var detailWindow = new CustomerDetailView(SelectedResident);
                    detailWindow.ShowDialog();
                    
                    // Refresh the list in case information was updated from detail view
                    LoadResidents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở chi tiết thông tin: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteViewDetail(object parameter)
        {
            return SelectedResident != null;
        }

        private void ExecuteDelete(object parameter)
        {
            try
            {
                if (SelectedResident != null)
                {
                    var result = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa thông tin của {SelectedResident.CustomerFullName}?",
                        "Xác nhận xóa", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _customerRepository.Delete(SelectedResident.CustomerID);
                        LoadResidents();
                        
                        MessageBox.Show("Xóa thông tin thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa thông tin: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteDelete(object parameter)
        {
            return SelectedResident != null;
        }

        private void ExecuteSearch(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadResidents();
                    return;
                }

                var allResidents = _customerRepository.GetAll();
                var filteredResidents = allResidents.Where(r =>
                    r.CustomerFullName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.CCCD.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.Telephone.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.CurrentRoomNumber.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();

                Residents.Clear();
                foreach (var resident in filteredResidents)
                {
                    Residents.Add(resident);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteRefresh(object parameter)
        {
            SearchText = string.Empty;
            LoadResidents();
        }

        private void ExecuteExportExcel(object parameter)
        {
            try
            {
                if (Residents != null && Residents.Count > 0)
                {
                    ExcelExporter.ExportCustomersToExcel(Residents.ToList());
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
