using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObject;
using Repository;
using AnLaNPWPF.Helpers;

namespace AnLaNPWPF.Views
{
    public partial class ContractBillViewDialog : UserControl
    {
        private MonthlyBillRepository _billRepository;
        private RentalContract _contract;
        private List<MonthlyBill> _bills;
        private Window _parentWindow;

        public ContractBillViewDialog(RentalContract contract, Window parentWindow = null)
        {
            InitializeComponent();
            _contract = contract;
            _parentWindow = parentWindow;
            _billRepository = new MonthlyBillRepository();
            
            LoadContractInfo();
            LoadBills();
        }

        private void LoadContractInfo()
        {
            try
            {
                if (_contract != null)
                {
                    txtContractInfo.Text = $"Hợp đồng: {_contract.ContractNumber} - " +
                                         $"Khách hàng: {_contract.CustomerName} - " +
                                         $"Phòng: {_contract.RoomNumber}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải thông tin hợp đồng: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBills()
        {
            try
            {
                txtStatusInfo.Text = "Đang tải danh sách hóa đơn...";
                
                if (_contract != null)
                {
                    // Lấy tất cả hóa đơn của hợp đồng này
                    _bills = _billRepository.GetBillsByContract(_contract.ContractId);
                    
                    if (_bills != null)
                    {
                        dgBills.ItemsSource = _bills;
                        txtBillCount.Text = $"Tổng: {_bills.Count} hóa đơn";
                        txtStatusInfo.Text = "Tải danh sách hóa đơn thành công";
                    }
                    else
                    {
                        dgBills.ItemsSource = new List<MonthlyBill>();
                        txtBillCount.Text = "Tổng: 0 hóa đơn";
                        txtStatusInfo.Text = "Không có hóa đơn nào";
                    }
                }
                else
                {
                    dgBills.ItemsSource = new List<MonthlyBill>();
                    txtBillCount.Text = "Tổng: 0 hóa đơn";
                    txtStatusInfo.Text = "Thông tin hợp đồng không hợp lệ";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi tải dữ liệu";
                txtBillCount.Text = "Tổng: 0 hóa đơn";
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtStatusInfo.Text = "Đang xuất danh sách hóa đơn...";
                
                if (_bills != null && _bills.Count > 0)
                {
                    // Sử dụng ExcelExporter để xuất hóa đơn
                    ExcelExporter.ExportBillsToExcel(_bills, $"HoaDon_HopDong_{_contract?.ContractNumber}");
                    txtStatusInfo.Text = "Xuất Excel thành công";
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu hóa đơn để xuất!", "Thông báo", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtStatusInfo.Text = "Không có dữ liệu";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xuất Excel: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatusInfo.Text = "Lỗi xuất Excel";
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadBills();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Đóng window chứa dialog này
                if (_parentWindow != null)
                {
                    _parentWindow.Close();
                }
                else
                {
                    // Tìm window cha và đóng
                    Window window = Window.GetWindow(this);
                    if (window != null)
                    {
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đóng cửa sổ: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
