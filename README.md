# 🏨 Motel Management System (ThanhDat WPF)

A comprehensive **Motel Management System** built with **WPF (Windows Presentation Foundation)** and **SQL Server LocalDB**. This application helps manage motel operations including room bookings, resident management, billing, and reporting.

## 📋 Features

### 🏠 **Room Management**
- Room information and status tracking
- Room type categorization
- Real-time availability updates
- Room maintenance scheduling

### 👥 **Resident Management**
- Customer registration and profiles
- Resident check-in/check-out
- Personal information management
- History tracking

### 📅 **Booking System**
- Room reservation management
- Booking status tracking
- Advanced booking search and filtering
- Booking statistics and analytics

### 💰 **Billing & Payments**
- Automated monthly bill generation
- Payment tracking and history
- Bill status management
- Excel export functionality

### 📊 **Reports & Analytics**
- Monthly revenue reports
- Occupancy rate statistics
- Customer analytics
- Exportable reports (Excel)

### 🔐 **Security**
- User authentication system
- Role-based access control
- Secure database connections

## 🛠️ Technology Stack

- **Frontend**: WPF (Windows Presentation Foundation)
- **Backend**: C# .NET Framework 4.8
- **Database**: SQL Server LocalDB 2019
- **Architecture**: MVVM Pattern
- **ORM**: Entity Framework / Raw SQL
- **Export**: EPPlus (Excel)
- **Packages**: Newtonsoft.Json, System.Data.SqlClient

## 🏗️ Project Structure

```
├── AnLaNPWPF/                  # Main WPF Application
│   ├── Views/                  # XAML Views (UI)
│   ├── ViewModels/            # MVVM ViewModels
│   ├── Helpers/               # Utility Classes
│   ├── Database/              # Database Files (LocalDB)
│   └── Resources/             # Images, Icons, etc.
├── BusinessObject/            # Data Models & Entities
├── Repository/                # Data Access Layer
└── FUMiniHotelManagement.sql  # Database Schema
```

## ⚙️ Setup & Installation

### Prerequisites
- Windows 10/11
- .NET Framework 4.8
- SQL Server LocalDB 2019
- Visual Studio 2019/2022 (for development)

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/motel-management-system.git
   cd motel-management-system
   ```

2. **Install SQL Server LocalDB 2019**
   - Download: [SqlLocalDB.msi](https://download.microsoft.com/download/7/c/1/7c14e92e-bdcb-4f89-b7cf-93543e7112d1/SqlLocalDB.msi)
   - Install the downloaded file

3. **Setup Database**
   ```sql
   # Run the SQL script to create database schema
   sqlcmd -S "(localdb)\MSSQLLocalDB" -i "FUMiniHotelManagement.sql"
   ```

4. **Build and Run**
   ```bash
   # Open in Visual Studio or build via command line
   msbuild ThanhDat3636Motel.sln /p:Configuration=Release
   ```

5. **Run Application**
   ```
   .\AnLaNPWPF\bin\Release\ThanhDat.exe
   ```

## 🔧 Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FUMiniHotelManagement;Integrated Security=true;AttachDBFilename=|DataDirectory|\\Database\\FUMiniHotelManagement.mdf"
  }
}
```

## 📱 Usage

1. **Login**: Use admin credentials to access the system
2. **Dashboard**: Overview of motel statistics and quick actions
3. **Room Management**: Add, edit, and monitor room status
4. **Bookings**: Handle reservations and check-ins
5. **Billing**: Generate and track monthly bills
6. **Reports**: View analytics and export data

## 🎯 Key Features Highlights

### 💡 **Smart Billing System**
- Automatic monthly bill calculation
- Configurable pricing rules
- Payment status tracking
- Late payment notifications

### 📈 **Business Analytics**
- Revenue tracking
- Occupancy rates
- Customer insights
- Monthly/yearly reports

### 🔄 **Real-time Updates**
- Live room status updates
- Booking notifications
- Payment alerts

## 🚀 Future Enhancements

- [ ] Web-based interface
- [ ] Mobile app support
- [ ] Payment gateway integration
- [ ] SMS/Email notifications
- [ ] Advanced reporting dashboard
- [ ] Multi-language support

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Developer

**ThanhDat** - Software Developer  
📧 Email: [your.email@example.com]  
💼 LinkedIn: [Your LinkedIn Profile]  
🐙 GitHub: [Your GitHub Profile]

## 🙏 Acknowledgments

- Built as part of software engineering coursework
- Thanks to instructors and classmates for feedback
- Special thanks to the open-source community

---

⭐ **Star this repository if it helped you!**
