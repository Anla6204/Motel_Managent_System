using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace AnLaNPWPF.Views
{
    /// <summary>
    /// Interaction logic for ImageViewerWindow.xaml
    /// </summary>
    public partial class ImageViewerWindow : Window
    {
        private string _imagePath;
        private double _currentZoom = 1.0;

        public ImageViewerWindow(string imagePath, string title)
        {
            InitializeComponent();
            _imagePath = imagePath;
            txtImageTitle.Text = title;
            LoadImage();
        }

        private void LoadImage()
        {
            try
            {
                if (string.IsNullOrEmpty(_imagePath) || !File.Exists(_imagePath))
                {
                    MessageBox.Show("Không tìm thấy file ảnh!", "Lỗi", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                MainImage.Source = bitmap;
                _currentZoom = 1.0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải ảnh: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.F11)
            {
                // Toggle fullscreen
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else
                {
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.None;
                }
            }
        }

        private void MainImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // Double click to fit window
            {
                FitToWindow_Click(sender, e);
            }
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentZoom *= 1.2;
                if (_currentZoom <= 5.0) // Max zoom 500%
                {
                    ApplyZoom();
                }
                else
                {
                    _currentZoom /= 1.2; // Revert
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi phóng to: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentZoom /= 1.2;
                if (_currentZoom >= 0.1) // Min zoom 10%
                {
                    ApplyZoom();
                }
                else
                {
                    _currentZoom *= 1.2; // Revert
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thu nhỏ: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FitToWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentZoom = 1.0;
                ApplyZoom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi fit cửa sổ: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyZoom()
        {
            try
            {
                MainImage.LayoutTransform = new System.Windows.Media.ScaleTransform(_currentZoom, _currentZoom);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi áp dụng zoom: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Title = "Lưu ảnh",
                    Filter = "JPEG Image|*.jpg|PNG Image|*.png|All files|*.*",
                    DefaultExt = "jpg",
                    FileName = $"CCCD_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.Copy(_imagePath, saveDialog.FileName, true);
                    MessageBox.Show("Đã lưu ảnh thành công!", "Thành công", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu ảnh: {ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
