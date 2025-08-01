using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BusinessObject
{
    public class RentalContract : INotifyPropertyChanged
    {
        private int _contractId;
        private int _customerId;
        private int _roomId;
        private string _contractNumber;
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _monthlyRent;
        private decimal _securityDeposit;
        private ContractStatus _contractStatus;
        private string _contractTerms;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int ContractId
        {
            get => _contractId;
            set { _contractId = value; OnPropertyChanged(nameof(ContractId)); }
        }

        public int CustomerId
        {
            get => _customerId;
            set { _customerId = value; OnPropertyChanged(nameof(CustomerId)); }
        }

        public int RoomId
        {
            get => _roomId;
            set { _roomId = value; OnPropertyChanged(nameof(RoomId)); }
        }

        public string ContractNumber
        {
            get => _contractNumber;
            set { _contractNumber = value; OnPropertyChanged(nameof(ContractNumber)); }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(nameof(StartDate)); }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
        }

        public decimal MonthlyRent
        {
            get => _monthlyRent;
            set { _monthlyRent = value; OnPropertyChanged(nameof(MonthlyRent)); }
        }

        public decimal SecurityDeposit
        {
            get => _securityDeposit;
            set { _securityDeposit = value; OnPropertyChanged(nameof(SecurityDeposit)); }
        }

        public ContractStatus ContractStatus
        {
            get => _contractStatus;
            set { _contractStatus = value; OnPropertyChanged(nameof(ContractStatus)); OnPropertyChanged(nameof(StatusText)); }
        }

        public string ContractTerms
        {
            get => _contractTerms;
            set { _contractTerms = value; OnPropertyChanged(nameof(ContractTerms)); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(nameof(CreatedAt)); }
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set { _updatedAt = value; OnPropertyChanged(nameof(UpdatedAt)); }
        }

        // Navigation properties
        public CustomerModel Customer { get; set; }
        public RoomInformationModel Room { get; set; }
        public List<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();

        // Computed properties
        public string StatusText
        {
            get
            {
                if (ContractStatus == ContractStatus.Active)
                    return "🟢 Đang hiệu lực";
                else if (ContractStatus == ContractStatus.Expired)
                    return "🔴 Hết hạn";
                else if (ContractStatus == ContractStatus.Terminated)
                    return "⚫ Đã chấm dứt";
                else if (ContractStatus == ContractStatus.Pending)
                    return "🟡 Chờ xử lý";
                else
                    return "❓ Không xác định";
            }
        }

        public bool IsExpired => DateTime.Now > EndDate && ContractStatus == ContractStatus.Active;
        
        public bool IsExpiringSoon => ContractStatus == ContractStatus.Active && 
            (EndDate - DateTime.Now).TotalDays <= 30 && (EndDate - DateTime.Now).TotalDays > 0;
        
        public int DaysRemaining => Math.Max(0, (int)(EndDate - DateTime.Now).TotalDays);

        public string CustomerName => Customer?.CustomerFullName ?? "Chưa có";
        public string RoomNumber => Room?.RoomNumber ?? "Chưa có";

        public int ContractDurationMonths => (int)((EndDate - StartDate).TotalDays / 30.44);

        public string FormattedMonthlyRent => MonthlyRent.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        public string FormattedSecurityDeposit => SecurityDeposit.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));

        // Constructor
        public RentalContract()
        {
            ContractNumber = $"HD{DateTime.Now:yyyyMMddHHmmss}";
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddMonths(6); // Default 6 months
            ContractStatus = ContractStatus.Pending;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            SecurityDeposit = 0;
            MonthlyRent = 0;
        }

        // Auto-update methods
        public void UpdateStatus()
        {
            if (ContractStatus == ContractStatus.Active && DateTime.Now > EndDate)
            {
                ContractStatus = ContractStatus.Expired;
                UpdatedAt = DateTime.Now;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ContractStatus
    {
        Pending = 0,    // Chờ xử lý
        Active = 1,     // Đang hiệu lực
        Expired = 2,    // Hết hạn
        Terminated = 3  // Đã chấm dứt
    }
}
