using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusinessObject
{
    public class ResidentModel : INotifyPropertyChanged
    {
        private int _residentId;
        private string _fullName;
        private string _gender;
        private string _identityCard;
        private DateTime _birthYear;
        private string _phoneNumber;
        private string _email;
        private string _address;
        private int _roomId;
        private string _roomNumber;
        private DateTime _checkInDate;
        private DateTime? _checkOutDate;
        private int _status; // 1: Đang ở, 2: Đã chuyển đi
        private string _notes;

        public int ResidentId
        {
            get => _residentId;
            set { _residentId = value; OnPropertyChanged(); }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(); }
        }

        public string IdentityCard
        {
            get => _identityCard;
            set { _identityCard = value; OnPropertyChanged(); }
        }

        public DateTime BirthYear
        {
            get => _birthYear;
            set { _birthYear = value; OnPropertyChanged(); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public int RoomId
        {
            get => _roomId;
            set { _roomId = value; OnPropertyChanged(); }
        }

        public string RoomNumber
        {
            get => _roomNumber;
            set { _roomNumber = value; OnPropertyChanged(); }
        }

        public DateTime CheckInDate
        {
            get => _checkInDate;
            set { _checkInDate = value; OnPropertyChanged(); }
        }

        public DateTime? CheckOutDate
        {
            get => _checkOutDate;
            set { _checkOutDate = value; OnPropertyChanged(); }
        }

        public int Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get
            {
                return _status switch
                {
                    1 => "Đang ở",
                    2 => "Đã chuyển đi",
                    _ => "Không xác định"
                };
            }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        // Navigation properties
        public RoomInformationModel Room { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
