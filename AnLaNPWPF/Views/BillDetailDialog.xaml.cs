using System;
using System.Windows;
using BusinessObject;
using Repository;
using AnLaNPWPF.Helpers;

namespace AnLaNPWPF.Views
{
    public partial class BillDetailDialog : Window
    {
        private MonthlyBill _bill;
        private MonthlyBillRepository _billRepository;

        public BillDetailDialog(MonthlyBill bill)
        {
            InitializeComponent();
            
            _bill = bill;
            _billRepository = new MonthlyBillRepository();
            
            LoadBillDetails();
        }

        private void LoadBillDetails()
        {
            try
            {
                // Header
                txtBillTitle.Text = $"Hóa đơn #{_bill.BillId} - {_bill.BillPeriod}";
                
                // Contract Info
                txtContractNumber.Text = _bill.Contract?.ContractNumber ?? "N/A";
                txtCustomerName.Text = _bill.Contract?.CustomerName ?? "N/A";
                txtRoomNumber.Text = _bill.Contract?.RoomNumber ?? "N/A";
                
                // Bill Period
                txtBillPeriod.Text = _bill.BillPeriod;
                
                // Cost Breakdown
                txtRoomRent.Text = _bill.FormattedRoomRent;
                
                txtElectricityUsage.Text = $"{_bill.ElectricityUsage:N1} kWh × {_bill.ElectricityRate:N0} = ";
                txtElectricityCost.Text = _bill.FormattedElectricityCost;
                
                txtWaterUsage.Text = $"{_bill.WaterUsage:N1} m³ × {_bill.WaterRate:N0} = ";
                txtWaterCost.Text = _bill.FormattedWaterCost;
                
                txtOtherCharges.Text = _bill.FormattedOtherCharges;
                txtTotalAmount.Text = _bill.FormattedTotal;
                
                // Payment Status
                txtBillStatus.Text = _bill.StatusText;
                txtDueDate.Text = _bill.FormattedDueDate;
                
                // Show paid info if applicable
                if (_bill.BillStatus == BillStatus.Paid || _bill.BillStatus == BillStatus.Partial)
                {
                    pnlPaidInfo.Visibility = Visibility.Visible;
                    txtPaidDate.Text = _bill.PaidDate?.ToString("dd/MM/yyyy") ?? "N/A";
                    txtPaidAmount.Text = _bill.PaidAmount?.ToString("C0") ?? "N/A";
                }
                else
                {
                    pnlPaidInfo.Visibility = Visibility.Collapsed;
                }
                
                // Timestamps
                txtCreatedAt.Text = _bill.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
                txtUpdatedAt.Text = _bill.UpdatedAt.ToString("dd/MM/yyyy HH:mm:ss");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải chi tiết hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BillPrintHelper.PrintBill(_bill);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in hóa đơn: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var statusDialog = new BillStatusUpdateDialog(_bill);
                if (statusDialog.ShowDialog() == true && statusDialog.IsUpdated)
                {
                    if (_billRepository.Update(statusDialog.Bill))
                    {
                        _bill = statusDialog.Bill; // Update local reference
                        LoadBillDetails(); // Refresh display
                        MessageBox.Show("Cập nhật trạng thái hóa đơn thành công!", "Thông báo", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể cập nhật trạng thái hóa đơn!", "Lỗi", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
