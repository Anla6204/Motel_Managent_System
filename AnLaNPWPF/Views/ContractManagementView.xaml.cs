using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;
using AnLaNPWPF.Helpers; // ✅ Add EventManager

namespace AnLaNPWPF.Views
{
    public partial class ContractManagementView : UserControl
    {
        private RentalContractRepository _contractRepository;
        private MonthlyBillRepository _billRepository;
        private RoomInfomationRepository _roomRepository; // ✅ Thêm RoomRepository
        private List<RentalContract> _allContracts;
        private List<RentalContract> _filteredContracts;

        public ContractManagementView()
        {
            InitializeComponent();
            
            try
            {
                _contractRepository = new RentalContractRepository();
                _billRepository = new MonthlyBillRepository();
                _roomRepository = new RoomInfomationRepository(); // Khởi tạo RoomRepository
                LoadContracts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo Contract Management: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Initialize empty collections to prevent further crashes
                _allContracts = new List<RentalContract>();
                _filteredContracts = new List<RentalContract>();
                dgContracts.ItemsSource = _filteredContracts;
            }
        }

        private void LoadContracts()
        {
            try
            {
                txtStatusInfo.Text = "Đang tải dữ liệu...";
                
                _allContracts = _contractRepository.GetAll();
                _filteredContracts = new List<RentalContract>(_allContracts);
                
                dgContracts.ItemsSource = _filteredContracts;
                UpdateSummary();
                UpdateStatusInfo();
                
                txtStatusInfo.Text = "Tải dữ liệu thành công";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách hợp đồng: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi tải dữ liệu";
            }
        }

        private void UpdateSummary()
        {
            if (_allContracts == null) return;

            var activeCount = _allContracts.Count(c => c.ContractStatus == ContractStatus.Active);
            var pendingCount = _allContracts.Count(c => c.ContractStatus == ContractStatus.Pending);
            var expiredCount = _allContracts.Count(c => c.ContractStatus == ContractStatus.Expired);
            var expiringSoonCount = _allContracts.Count(c => c.IsExpiringSoon);

            txtSummary.Text = $"Tổng {_allContracts.Count} hợp đồng | " +
                             $"Hiệu lực: {activeCount} | " +
                             $"Chờ xử lý: {pendingCount} | " +
                             $"Hết hạn: {expiredCount} | " +
                             $"Sắp hết hạn: {expiringSoonCount}";
        }

        private void UpdateStatusInfo()
        {
            txtContractCount.Text = $"Hiển thị: {_filteredContracts?.Count ?? 0} / {_allContracts?.Count ?? 0} hợp đồng";
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadContracts();
        }

        private void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allContracts == null) return;

            var filtered = _allContracts.AsEnumerable();

            // Filter by status
            var selectedStatus = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "All")
            {
                if (Enum.TryParse<ContractStatus>(selectedStatus, out var status))
                {
                    filtered = filtered.Where(c => c.ContractStatus == status);
                }
            }

            // Filter by search text
            var searchText = txtSearch.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                filtered = filtered.Where(c =>
                    (c.ContractNumber?.ToLower().Contains(searchText) == true) ||
                    (c.CustomerName?.ToLower().Contains(searchText) == true) ||
                    (c.RoomNumber?.ToLower().Contains(searchText) == true));
            }

            _filteredContracts = filtered.ToList();
            dgContracts.ItemsSource = _filteredContracts;
            UpdateStatusInfo();
        }

        private void AddContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var contractDialog = new ContractDialog();
                if (contractDialog.ShowDialog() == true && contractDialog.IsSaved)
                {
                    if (_contractRepository.Add(contractDialog.Contract))
                    {
                        // ✅ Cập nhật room status thành "Hoạt động" khi có hợp đồng mới
                        UpdateRoomStatusAfterContract(contractDialog.Contract.RoomId, 1); // 1 = Hoạt động
                        
                        LoadContracts();
                        
                        // ✅ Trigger events for other views to refresh
                        GlobalEventBus.OnContractDataChanged();
                        GlobalEventBus.OnRoomStatusChanged(contractDialog.Contract.RoomId);
                        
                        MessageBox.Show("Tạo hợp đồng thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể tạo hợp đồng. Vui lòng thử lại!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var contract = button?.Tag as RentalContract;
                
                if (contract == null) return;

                var contractDialog = new ContractDialog(contract);
                if (contractDialog.ShowDialog() == true && contractDialog.IsSaved)
                {
                    if (_contractRepository.Update(contractDialog.Contract))
                    {
                        // ✅ Cập nhật room status dựa trên contract status
                        UpdateRoomStatusBasedOnContract(contractDialog.Contract);
                        
                        LoadContracts();
                        
                        // ✅ Trigger events for other views to refresh
                        GlobalEventBus.OnContractStatusChanged(contractDialog.Contract.ContractId);
                        GlobalEventBus.OnRoomStatusChanged(contractDialog.Contract.RoomId);
                        
                        MessageBox.Show("Cập nhật hợp đồng thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể cập nhật hợp đồng. Vui lòng thử lại!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteContract_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var contract = button?.Tag as RentalContract;
                
                if (contract == null) return;

                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa hợp đồng {contract.ContractNumber}?\n" +
                    $"Khách hàng: {contract.CustomerName}\n" +
                    $"Phòng: {contract.RoomNumber}\n\n" +
                    "Thao tác này không thể hoàn tác!",
                    "Xác nhận xóa", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (_contractRepository.Delete(contract.ContractId))
                    {
                        LoadContracts();
                        MessageBox.Show("Xóa hợp đồng thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa hợp đồng. Vui lòng thử lại!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewBills_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var contract = button?.Tag as RentalContract;
                
                if (contract == null) return;

                // Tạo window mới để hiển thị popup
                var window = new Window
                {
                    Title = $"Danh sách hóa đơn - Hợp đồng {contract.ContractNumber}",
                    Width = 950,
                    Height = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.CanResize
                };

                // Tạo và thêm ContractBillViewDialog vào window
                var billViewDialog = new ContractBillViewDialog(contract, window);
                window.Content = billViewDialog;
                
                // Hiển thị window
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xem hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContractGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedContract = dgContracts.SelectedItem as RentalContract;
            if (selectedContract != null)
            {
                EditContract_Click(new Button { Tag = selectedContract }, null);
            }
        }

        private void ExpireContracts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Cập nhật trạng thái hết hạn cho tất cả hợp đồng đã quá ngày kết thúc?\n\n" +
                    "Thao tác này sẽ đánh dấu các hợp đồng đã hết hạn thành trạng thái 'Hết hạn'.",
                    "Xác nhận cập nhật", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    txtStatusInfo.Text = "Đang cập nhật trạng thái...";
                    
                    var contractsExpired = _contractRepository.ExpireOutdatedContracts();
                    
                    if (contractsExpired > 0)
                    {
                        LoadContracts(); // Refresh to show updated status
                    }
                    
                    txtStatusInfo.Text = "Cập nhật hoàn tất";
                    
                    MessageBox.Show($"Đã cập nhật {contractsExpired} hợp đồng hết hạn!", 
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                txtStatusInfo.Text = "Lỗi cập nhật";
                MessageBox.Show($"Lỗi khi cập nhật trạng thái hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateRoomStatusAfterContract(int roomId, int status)
        {
            try
            {
                // Cập nhật trạng thái phòng trong cơ sở dữ liệu
                _roomRepository.UpdateRoomStatus(roomId, status);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật trạng thái phòng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ Method mới để cập nhật room status dựa trên contract status
        private void UpdateRoomStatusBasedOnContract(RentalContract contract)
        {
            try
            {
                int newRoomStatus;
                
                // Kiểm tra trạng thái hợp đồng để quyết định room status
                switch (contract.ContractStatus)
                {
                    case ContractStatus.Active:
                        newRoomStatus = 1; // Hoạt động
                        // Chỉ cập nhật status, không xóa khách hàng
                        _roomRepository.UpdateRoomStatus(contract.RoomId, newRoomStatus);
                        break;
                    case ContractStatus.Terminated:
                    case ContractStatus.Expired:
                        newRoomStatus = 0; // Phòng trống
                        // ✅ Cập nhật status VÀ xóa khách hàng khỏi phòng
                        _roomRepository.UpdateRoomStatusAndClearCustomers(contract.RoomId, newRoomStatus);
                        break;
                    default:
                        return; // Không thay đổi cho Draft, Pending
                }

                // Thông báo đến các view khác về sự thay đổi
                GlobalEventBus.OnRoomDataChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật trạng thái phòng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
