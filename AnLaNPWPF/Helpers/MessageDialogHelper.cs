using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AnLaNPWPF.Helpers
{
    public static class MessageDialogHelper
    {
        /// <summary>
        /// Hi?n th? h?p tho?i tùy ch?nh thay th? cho MessageBox
        /// </summary>
        /// <param name="message">N?i dung thông báo</param>
        /// <param name="title">Tiêu ?? h?p tho?i</param>
        /// <param name="messageType">Lo?i thông báo: Success, Error, Warning, Info</param>
        /// <returns>MessageBoxResult</returns>
        public static MessageBoxResult ShowMessageDialog(string message, string title, MessageType messageType)
        {
            // T?o c?a s? m?i
            Window dialogWindow = new Window
            {
                Title = title,
                Width = 380,
                Height = 200,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent
            };

            // T?o border cho c?a s?
            Border mainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 320,
                    ShadowDepth = 5,
                    Opacity = 0.3,
                    BlurRadius = 8
                }
            };

            // Grid chính
            Grid mainGrid = new Grid();
            mainBorder.Child = mainGrid;

            // ??nh ngh?a hàng cho grid
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            // Title bar
            Border titleBar = new Border
            {
                Background = GetHeaderBackgroundByType(messageType),
                CornerRadius = new CornerRadius(8, 8, 0, 0)
            };

            Grid titleGrid = new Grid();
            titleBar.Child = titleGrid;

            // Icon và Text cho title
            StackPanel titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            TextBlock titleText = new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };

            // Thêm icon phù h?p v?i lo?i thông báo
            Image icon = new Image
            {
                Width = 20,
                Height = 20,
                Source = GetIconByType(messageType),
                VerticalAlignment = VerticalAlignment.Center
            };

            titlePanel.Children.Add(icon);
            titlePanel.Children.Add(titleText);
            titleGrid.Children.Add(titlePanel);

            // Close button
            Button closeButton = new Button
            {
                Content = "?",
                Foreground = Brushes.White,
                FontSize = 14,
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 0)
            };

            closeButton.Click += (s, e) => { dialogWindow.DialogResult = false; };
            titleGrid.Children.Add(closeButton);

            // Thêm title bar vào grid
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Content panel
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(20, 15, 20, 10)
            };

            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            scrollViewer.Content = messageTextBlock;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // Buttons panel
            Border buttonsBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                CornerRadius = new CornerRadius(0, 0, 8, 8)
            };

            StackPanel buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // OK button style
            Style buttonStyle = new Style(typeof(Button));
            buttonStyle.Setters.Add(new Setter(Button.BackgroundProperty, GetHeaderBackgroundByType(messageType)));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            buttonStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.SemiBold));
            buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            buttonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(20, 8, 20, 8)));
            buttonStyle.Setters.Add(new Setter(Button.MarginProperty, new Thickness(5)));
            buttonStyle.Setters.Add(new Setter(Button.CursorProperty, System.Windows.Input.Cursors.Hand));
            
            ControlTemplate buttonTemplate = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            
            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            borderFactory.AppendChild(contentPresenterFactory);
            buttonTemplate.VisualTree = borderFactory;
            
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));
            
            // OK button
            Button okButton = new Button
            {
                Content = "OK",
                Style = buttonStyle,
                Width = 100
            };

            okButton.Click += (s, e) => { dialogWindow.DialogResult = true; };
            buttonsPanel.Children.Add(okButton);

            buttonsBorder.Child = buttonsPanel;
            Grid.SetRow(buttonsBorder, 2);
            mainGrid.Children.Add(buttonsBorder);

            // Thêm main border vào c?a s?
            dialogWindow.Content = mainBorder;

            // Animation khi hi?n th?
            dialogWindow.Loaded += (s, e) =>
            {
                DoubleAnimation fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                dialogWindow.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            // Hi?n th? h?p tho?i và tr? v? k?t qu?
            return dialogWindow.ShowDialog() == true ? MessageBoxResult.OK : MessageBoxResult.Cancel;
        }

        /// <summary>
        /// Phiên b?n ??n gi?n h?n ch? hi?n th? thông báo
        /// </summary>
        public static void ShowMessage(string message, string title = "Thông báo", MessageType messageType = MessageType.Info)
        {
            ShowMessageDialog(message, title, messageType);
        }

        /// <summary>
        /// Hàm xác nh?n Yes/No
        /// </summary>
        public static MessageBoxResult ShowConfirmation(string message, string title = "Xác nh?n")
        {
            // T?o c?a s? m?i
            Window dialogWindow = new Window
            {
                Title = title,
                Width = 380,
                Height = 200,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent
            };

            // T?o border cho c?a s?
            Border mainBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 320,
                    ShadowDepth = 5,
                    Opacity = 0.3,
                    BlurRadius = 8
                }
            };

            // Grid chính
            Grid mainGrid = new Grid();
            mainBorder.Child = mainGrid;

            // ??nh ngh?a hàng cho grid
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

            // Title bar
            Border titleBar = new Border
            {
                Background = GetHeaderBackgroundByType(MessageType.Warning),
                CornerRadius = new CornerRadius(8, 8, 0, 0)
            };

            Grid titleGrid = new Grid();
            titleBar.Child = titleGrid;

            // Icon và Text cho title
            StackPanel titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            TextBlock titleText = new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };

            // Thêm icon
            Image icon = new Image
            {
                Width = 20,
                Height = 20,
                Source = GetIconByType(MessageType.Warning),
                VerticalAlignment = VerticalAlignment.Center
            };

            titlePanel.Children.Add(icon);
            titlePanel.Children.Add(titleText);
            titleGrid.Children.Add(titlePanel);

            // Close button
            Button closeButton = new Button
            {
                Content = "?",
                Foreground = Brushes.White,
                FontSize = 14,
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 0)
            };

            closeButton.Click += (s, e) => { dialogWindow.DialogResult = false; };
            titleGrid.Children.Add(closeButton);

            // Thêm title bar vào grid
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // Content panel
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(20, 15, 20, 10)
            };

            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            scrollViewer.Content = messageTextBlock;
            Grid.SetRow(scrollViewer, 1);
            mainGrid.Children.Add(scrollViewer);

            // Buttons panel
            Border buttonsBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                CornerRadius = new CornerRadius(0, 0, 8, 8)
            };

            StackPanel buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Button style
            Style buttonStyle = new Style(typeof(Button));
            buttonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            buttonStyle.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.SemiBold));
            buttonStyle.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            buttonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(20, 8, 20, 8)));
            buttonStyle.Setters.Add(new Setter(Button.MarginProperty, new Thickness(5)));
            buttonStyle.Setters.Add(new Setter(Button.CursorProperty, System.Windows.Input.Cursors.Hand));
            
            ControlTemplate buttonTemplate = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            
            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            
            borderFactory.AppendChild(contentPresenterFactory);
            buttonTemplate.VisualTree = borderFactory;
            
            buttonStyle.Setters.Add(new Setter(Button.TemplateProperty, buttonTemplate));

            // Yes button
            Button yesButton = new Button
            {
                Content = "??ng ý",
                Style = buttonStyle,
                Width = 100,
                Background = new SolidColorBrush(Color.FromRgb(40, 167, 69))
            };

            yesButton.Click += (s, e) => { dialogWindow.DialogResult = true; };
            buttonsPanel.Children.Add(yesButton);

            // No button
            Button noButton = new Button
            {
                Content = "H?y",
                Style = buttonStyle,
                Width = 100,
                Background = new SolidColorBrush(Color.FromRgb(108, 117, 125))
            };

            noButton.Click += (s, e) => { dialogWindow.DialogResult = false; };
            buttonsPanel.Children.Add(noButton);

            buttonsBorder.Child = buttonsPanel;
            Grid.SetRow(buttonsBorder, 2);
            mainGrid.Children.Add(buttonsBorder);

            // Thêm main border vào c?a s?
            dialogWindow.Content = mainBorder;

            // Animation khi hi?n th?
            dialogWindow.Loaded += (s, e) =>
            {
                DoubleAnimation fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                dialogWindow.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            // Hi?n th? h?p tho?i và tr? v? k?t qu?
            return dialogWindow.ShowDialog() == true ? MessageBoxResult.Yes : MessageBoxResult.No;
        }

        // L?y màu n?n cho header tùy theo lo?i thông báo
        private static SolidColorBrush GetHeaderBackgroundByType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Success:
                    return new SolidColorBrush(Color.FromRgb(40, 167, 69));
                case MessageType.Error:
                    return new SolidColorBrush(Color.FromRgb(220, 53, 69));
                case MessageType.Warning:
                    return new SolidColorBrush(Color.FromRgb(255, 193, 7));
                case MessageType.Info:
                default:
                    return new SolidColorBrush(Color.FromRgb(23, 162, 184));
            }
        }

        // L?y icon tùy theo lo?i thông báo (s? d?ng hình ?nh t? mã Base64)
        private static BitmapImage GetIconByType(MessageType messageType)
        {
            string iconBase64;

            switch (messageType)
            {
                case MessageType.Success:
                    iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADVSURBVEhL7ZVNCsIwEEZzLkEQehBBEAQP4AHcuXHnpVzpQhAXggfQheDv+0omEJukE7sRfPAWaWb6JplkUhwwxlRo5g5ZJ09oavOFkC60QJfkHY1A5g10Dm3kh3tDL9DgmjmENsiFKbQJjeEMXUETh9nD2eZ3CQc0kx7yYgw1Q3/CQaNkTh4ItUIbmjHSjxD/hMQlJJrbpFTCuSK5En5DpITnBXFUwtGrO1TCZakk8lLnD5RwHfXX5x9UwrHuiSTU0S1dRmgPcdDUpsUWzWx+GKL4APKaUCl3XOXbAAAAAElFTkSuQmCC";
                    break;
                case MessageType.Error:
                    iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFHSURBVEhL7ZU7TsNAEIb3PBwpEQ1CAqEUoKOhpqGioaKkoKehoeIGFBQUCEoqJCoKhHgIhAQJr++fZC3vgzXrpMgvfUo8s/7H9s7Oump5OVBXyJFykCe0Yft2wXOQr5AKxhP6Qe6RU2QfWbSjZsFIlYsgc8Qgp8gFsoTIfY3x6bwiyFqIz+QaWUGmkRyuEV+faiBpIa7x+Yk3ZB1ZQ0aIT+QG2e10OvtUPdAIgV/LJaLxtT/IDqJdHLUQN4Kw+xz1fwzRe3TfT5Rdkgk+Ezkl+0j2zPc1PZs5JYrglOSU5BpR/p+RB2QD6SPZ5hFyXhFkHfHNbxFNjWZ1hOi4n6OZ4FPxnb1TxCBLiHZxgLijGfWwjWTCs86nyBaSLVB8OGLPrZ+GGsS9ILQ9u0Q0rhJmDnHnR5Nt1SxoVYH0OudCQXr4oar+AJHxGvGPUDYJAAAAAElFTkSuQmCC";
                    break;
                case MessageType.Warning:
                    iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFYSURBVEhL7ZU9TsNAEIX3aBwpERVCQqIDOhoaGq6QI3AGbkCBAIkCUVIgfghBiBAJhwTi5/1jdq31JnaclCnySU+KZ/y+tXdnx1nLy4F6RI6UI+QeLdp9t+A5yIdIBeMJ/SK3yB5yiKza1jLBSMTFyBzRyAlyjiwhcu8xPp1XGlkJ8ZHcIKvINFLCNeJz52ogaSFu4/OEd8gGsoHUiG/kFtmZTCYHVD3QCIZPS+SIxtf+IruIdnHSQtwIxu5L1P/RRK/RfT9Rdkku+Eqk6zytmr0kn4qUMzXfj7RMmq+4TeSUqPsLRB9cQ+SZlkkLkXzOTxE1rGd1jKi4n6OZ4FPxnb1TRCNLiHZxhLitGfVwiGTC89anSGxVlEBLRjmXRFZJ2ZtGMGLPrZ+GGsS9IBz69D1EdZUws4l7f7TYTs2CVhVIr3MuVKRXNK3qD2sLFgwcJJ9tAAAAAElFTkSuQmCC";
                    break;
                case MessageType.Info:
                default:
                    iconBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFHSURBVEhL7ZU9TsNAEIX9c6REVAgJiQ7o6GhoOAJHoOMIFDQcggNU0CBRUiAoKRBCICQSfr/vzWzWG2ft2EkHT3rKzng/e3Znd5xWeHlcPwt0hBzqe9ht8HTJt5EKxg36Ti5QF7VROzY1E4xEXIzKiEZOUQ91UbP3GJ/OKo16IT6RC5ShKeqgIq4Rn9tXDaQX4l18nvCCLNEyrdIC3aJ9XdeBqtYaIShakXOk8bU/qEM6qJM24kaQdveD+n9q6Dk67odUbJwJvhI55kn2bJ9lT0VOiWb/jOjD02AeUXHMEP86P0M0sHrXR1TUz1FP8Kn4zt45opEZdVCHtLdDGvWwjdQKzz4/RbSLvIBq+6go5PwoLdEn4o7mfBpqEPeCsOvTd4i6SpgmqPR/VNiqJkGvFkivs2IqMixqVn+gWw1wfJhJ6wAAAABJRU5ErkJggg==";
                    break;
            }

            byte[] bytes = Convert.FromBase64String(iconBase64);
            BitmapImage image = new BitmapImage();
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }
    }

    // ??nh ngh?a các lo?i thông báo
    public enum MessageType
    {
        Success,
        Error,
        Warning,
        Info
    }
}