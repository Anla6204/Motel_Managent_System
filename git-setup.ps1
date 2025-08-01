# Git Setup Script for Motel Management System
# Chạy script này để init Git repository và upload lên GitHub

Write-Host "=== THIẾT LẬP GIT REPOSITORY ===" -ForegroundColor Green

# Kiểm tra Git có được cài đặt không
try {
    $gitVersion = git --version
    Write-Host "✅ Git đã được cài đặt: $gitVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Git chưa được cài đặt. Vui lòng tải từ: https://git-scm.com/" -ForegroundColor Red
    exit 1
}

# Kiểm tra thư mục hiện tại
$currentDir = Get-Location
Write-Host "📁 Thư mục hiện tại: $currentDir" -ForegroundColor Yellow

# Init Git repository
Write-Host "`n🔧 Khởi tạo Git repository..." -ForegroundColor Yellow
git init

# Add all files
Write-Host "📦 Thêm tất cả files vào staging..." -ForegroundColor Yellow
git add .

# First commit
Write-Host "💾 Tạo commit đầu tiên..." -ForegroundColor Yellow
git commit -m "Initial commit: Motel Management System WPF

Features:
- Room management and booking system
- Resident and customer management  
- Automated billing and payment tracking
- Reports and analytics with Excel export
- MVVM architecture with WPF UI
- SQL Server LocalDB integration

Tech Stack: C# WPF, .NET Framework 4.8, SQL Server LocalDB 2019"

Write-Host "✅ Repository đã được thiết lập thành công!" -ForegroundColor Green

Write-Host "`n📋 BƯỚC TIẾP THEO - UPLOAD LÊN GITHUB:" -ForegroundColor Cyan
Write-Host "1. Tạo repository mới trên GitHub:" -ForegroundColor White
Write-Host "   - Đi tới https://github.com/new" -ForegroundColor White
Write-Host "   - Repository name: motel-management-system" -ForegroundColor White
Write-Host "   - Description: WPF Motel Management System with SQL LocalDB" -ForegroundColor White
Write-Host "   - Public repository (để cho vào CV)" -ForegroundColor White
Write-Host "   - KHÔNG tick 'Add README' (vì đã có rồi)" -ForegroundColor White

Write-Host "`n2. Link local repo với GitHub:" -ForegroundColor White
Write-Host "   git branch -M main" -ForegroundColor Gray
Write-Host "   git remote add origin https://github.com/YOURUSERNAME/motel-management-system.git" -ForegroundColor Gray
Write-Host "   git push -u origin main" -ForegroundColor Gray

Write-Host "`n3. Cập nhật README.md:" -ForegroundColor White
Write-Host "   - Sửa [yourusername] thành username GitHub thật" -ForegroundColor White
Write-Host "   - Thêm email và LinkedIn của bạn" -ForegroundColor White
Write-Host "   - Thêm screenshots nếu có" -ForegroundColor White

Write-Host "`n🎯 CHO CV:" -ForegroundColor Green
Write-Host "✅ Clean code structure" -ForegroundColor White
Write-Host "✅ Professional README.md" -ForegroundColor White
Write-Host "✅ MIT License" -ForegroundColor White
Write-Host "✅ .gitignore hoàn chỉnh" -ForegroundColor White
Write-Host "✅ Kích thước nhỏ gọn (0.83MB)" -ForegroundColor White
Write-Host "✅ 103 files, chỉ source code" -ForegroundColor White

Write-Host "`n📊 THỐNG KÊ PROJECT:" -ForegroundColor Yellow
$csharpFiles = (Get-ChildItem -Recurse -Filter "*.cs").Count
$xamlFiles = (Get-ChildItem -Recurse -Filter "*.xaml").Count
$totalLines = (Get-ChildItem -Recurse -Filter "*.cs" | Get-Content | Measure-Object -Line).Lines
Write-Host "📄 C# files: $csharpFiles" -ForegroundColor White
Write-Host "🎨 XAML files: $xamlFiles" -ForegroundColor White
Write-Host "📝 Lines of code: ~$totalLines" -ForegroundColor White

Read-Host "`nNhấn Enter để tiếp tục..."
