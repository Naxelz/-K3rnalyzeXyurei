using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Yurei.Pages;

namespace Yurei
{
    public partial class MainWindow : Window
    {
        private string _currentPage = "Home";
        private Dictionary<string, UserControl> _pages = new Dictionary<string, UserControl>();
        
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitializePages();
                NavigateToPage("Home");
                UpdateNavigation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar la ventana: {ex.Message}\n\n{ex.StackTrace}", 
                    "Error de inicialización", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
        
        private void InitializePages()
        {
            try
            {
                _pages["Home"] = new HomePage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar HomePage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            try
            {
                _pages["Exploits"] = new ExploitsPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ExploitsPage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            try
            {
                _pages["FFlags"] = new FFlagsPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar FFlagsPage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            try
            {
                _pages["VPN"] = new VPNPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar VPNPage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            try
            {
                _pages["Settings"] = new Pages.SettingsPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar SettingsPage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            try
            {
                _pages["Lookups"] = new Pages.LookupsPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar LookupsPage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            try
            {
                _pages["Tools"] = new Pages.ToolsPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar ToolsPage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            try
            {
                _pages["Docs"] = new Pages.DocsPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar DocsPage: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void UpdateNavigation()
        {
            
            foreach (var child in GetVisualChildren(this))
            {
                if (child is Button btn && btn.Tag != null)
                {
                    if (btn.Tag.ToString() == _currentPage)
                    {
                        btn.Style = (Style)FindResource("NavItemActiveStyle");
                    }
                    else
                    {
                        btn.Style = (Style)FindResource("NavItemStyle");
                    }
                }
            }
        }
        
        private IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                yield return VisualTreeHelper.GetChild(parent, i);
                foreach (var child in GetVisualChildren(VisualTreeHelper.GetChild(parent, i)))
                {
                    yield return child;
                }
            }
        }
        
        private void NavigateToPage(string pageName)
        {
            try
            {
                if (_pages.ContainsKey(pageName) && _pages[pageName] != null)
                {
                    _currentPage = pageName;
                    
                    
                    if (ContentArea.Content != null && ContentArea.Opacity > 0)
                    {
                        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.15));
                        fadeOut.Completed += (s, e) =>
                        {
                            ContentArea.Content = _pages[pageName];
                            AnimatePageIn();
                        };
                        ContentArea.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                    }
                    else
                    {
                        
                        ContentArea.Content = _pages[pageName];
                        ContentArea.Opacity = 1.0;
                    }
                    
                    UpdateNavigation();
                }
                else
                {
                    MessageBox.Show($"La página '{pageName}' no está disponible.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al navegar a {pageName}: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AnimatePageIn()
        {
            var slideIn = new DoubleAnimation(20, 0, TimeSpan.FromSeconds(0.3));
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
            
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };
            slideIn.EasingFunction = easing;
            fadeIn.EasingFunction = easing;
            
            var transform = ContentArea.RenderTransform as TranslateTransform ?? new TranslateTransform();
            ContentArea.RenderTransform = transform;
            
            transform.BeginAnimation(TranslateTransform.XProperty, slideIn);
            ContentArea.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }
        
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string pageName = btn.Tag.ToString() ?? "Home";
                NavigateToPage(pageName);
            }
        }
        
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                SearchPlaceholder.Visibility = Visibility.Collapsed;
            }
        }
        
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string query = SearchBox.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(query)) return;

                var pages = new[] { "Home", "Exploits", "FFlags", "VPN", "Lookups", "Tools", "Docs", "Settings" };
                var match = pages.FirstOrDefault(p => p.ToLower().Contains(query) || query.Contains(p.ToLower()));

                if (match != null)
                {
                    NavigateToPage(match);
                    SearchBox.Text = ""; 
                    Keyboard.ClearFocus();
                }
                else
                {
                    MessageBox.Show($"No se encontró ninguna página que coincida con '{query}'.", "Búsqueda", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchPlaceholder.Visibility = Visibility.Collapsed;
        }
        
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchPlaceholder.Visibility = Visibility.Visible;
            }
        }
        
        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string link = btn.Tag.ToString() ?? "unknown";
                MessageBox.Show($"Abriendo: {link}", "Enlace", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
        }
        
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            
            var result = MessageBox.Show(
                "¿Estás seguro de que quieres cerrar Yurei?",
                "Cerrar aplicación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        public void ShowScanOverlay()
        {
            try
            {
                ScanOverlay.Visibility = Visibility.Visible;
                ScanProgress.Value = 0;
                ScanStatus.Text = "Validating integrity.. 0%";
            }
            catch { }
        }

        public void UpdateScanProgress(int pct)
        {
            try
            {
                if (pct < 0) pct = 0; if (pct > 100) pct = 100;
                ScanProgress.Value = pct;
                ScanStatus.Text = $"Validating integrity.. {pct}%";
            }
            catch { }
        }

        public async void HideOverlayWithSuccess()
        {
            try
            {
                ScanStatus.Text = "Scan finished";
                await System.Threading.Tasks.Task.Delay(1200);
                ScanOverlay.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
    }
}

