using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using BusinessObject;
using Repository;
using AnLaNPWPF.Views;
using AnLaNPWPF.Helpers; // ✅ Add EventManager

namespace AnLaNPWPF.ViewModels
{
    public class RoomManagementViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<RoomInformationModel> _rooms;
        private RoomInformationModel _selectedRoom;
        private string _searchText;
        private int _selectedStatusFilter = -1; // -1 means all, 0 = empty, 1 = active, 2 = maintenance
        private readonly RoomInfomationRepository _repository;

        public ObservableCollection<RoomInformationModel> Rooms
        {
            get => _rooms;
            set { _rooms = value; OnPropertyChanged(); }
        }

        public RoomInformationModel SelectedRoom
        {
            get => _selectedRoom;
            set 
            { 
                _selectedRoom = value; 
                OnPropertyChanged();
                ((RelayCommand)ViewDetailCommand).RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public int SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); ExecuteSearch(null); }
        }

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewDetailCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportExcelCommand { get; }

        public RoomManagementViewModel()
        {
            _repository = new RoomInfomationRepository();

            SearchCommand = new RelayCommand(ExecuteSearch);
            AddCommand = new RelayCommand(ExecuteAdd);
            EditCommand = new RelayCommand(ExecuteEdit);
            DeleteCommand = new RelayCommand(ExecuteDelete);
            ViewDetailCommand = new RelayCommand(ExecuteViewDetail, CanExecuteViewDetail);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            ExportExcelCommand = new RelayCommand(ExecuteExportExcel);

            LoadRooms();
            
            // ✅ Subscribe to events from other views
            GlobalEventBus.ContractDataChanged += OnContractDataChanged;
            GlobalEventBus.BillDataChanged += OnBillDataChanged;
        }
        
        // ✅ Event handlers for auto-refresh
        private void OnContractDataChanged()
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => LoadRooms()));
        }
        
        private void OnBillDataChanged()
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() => LoadRooms()));
        }
        
        // ✅ Destructor to unsubscribe events
        ~RoomManagementViewModel()
        {
            GlobalEventBus.ContractDataChanged -= OnContractDataChanged;
            GlobalEventBus.BillDataChanged -= OnBillDataChanged;
        }
        
        // Public method to refresh data - called from other views
        public void RefreshData()
        {
            LoadRooms();
        }
        
        private void LoadRooms()
        {
            try
            {
                // Reset filters khi load lại
                _selectedStatusFilter = -1;
                OnPropertyChanged(nameof(SelectedStatusFilter));
                _searchText = string.Empty;
                OnPropertyChanged(nameof(SearchText));

                var roomList = _repository.GetAll(); // no need for null-safe call now
                if (roomList != null)
                {
                    Rooms = new ObservableCollection<RoomInformationModel>(roomList);
                }
                else
                {
                    MessageBox.Show("Room list is null!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading rooms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ExecuteSearch(object parameter)
        {
            try
            {
                var allRooms = _repository.GetAll();
                if (allRooms == null) return;

                // Áp dụng filter theo trạng thái
                var filteredRooms = allRooms.AsEnumerable();
                
                if (SelectedStatusFilter != -1)
                {
                    filteredRooms = filteredRooms.Where(r => r.RoomStatus == SelectedStatusFilter);
                }

                // Áp dụng search text
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredRooms = filteredRooms
                        .Where(r => r != null &&
                                    (r.RoomNumber != null && r.RoomNumber.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (r.RoomTypes != null && r.RoomTypes.RoomTypeName != null && 
                                     r.RoomTypes.RoomTypeName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (r.RoomDetailDescription != null && 
                                     r.RoomDetailDescription.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (r.CustomerInfo != null && 
                                     r.CustomerInfo.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0));
                }

                Rooms = new ObservableCollection<RoomInformationModel>(filteredRooms.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during search: " + ex.Message, "Search Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadRooms(); // Reload all rooms if search fails
            }
        }


        private void ExecuteAdd(object parameter)
        {
            var dialog = new RoomDialog(); // Call without parameter for new room
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _repository.Add(dialog.Room);
                    LoadRooms();
                    
                    // ✅ Trigger event for other views
                    GlobalEventBus.OnRoomDataChanged();
                    
                    MessageBox.Show("Thêm phòng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi thêm phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteEdit(object parameter)
        {
            if (parameter is RoomInformationModel room)
            {
                var dialog = new RoomDialog(room);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        _repository.Update(dialog.Room);
                        LoadRooms();
                        
                        // ✅ Trigger events for other views
                        GlobalEventBus.OnRoomStatusChanged(dialog.Room.RoomID);
                        
                        MessageBox.Show("Room updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating room: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ExecuteDelete(object parameter)
        {
            if (parameter is RoomInformationModel room)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete room {room.RoomNumber}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repository.Delete(room.RoomID);
                        LoadRooms();
                        MessageBox.Show("Room deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ExecuteViewDetail(object parameter)
        {
            try
            {
                if (SelectedRoom != null)
                {
                    var roomDetailView = new RoomDetailView(SelectedRoom);
                    roomDetailView.ShowDialog();
                    
                    // Refresh rooms list after viewing details (in case changes were made)
                    LoadRooms();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở chi tiết: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteViewDetail(object parameter)
        {
            return SelectedRoom != null;
        }

        private void ExecuteRefresh(object parameter)
        {
            RefreshData();
        }

        private void ExecuteExportExcel(object parameter)
        {
            try
            {
                if (Rooms != null && Rooms.Count > 0)
                {
                    ExcelExporter.ExportRoomsToExcel(Rooms.ToList());
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
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}