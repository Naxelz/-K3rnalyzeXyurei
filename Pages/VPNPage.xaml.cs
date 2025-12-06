using System;
using System.Windows;
using System.Windows.Controls;
using Yurei.Services;

namespace Yurei.Pages
{
    public partial class VPNPage : UserControl
    {
        private readonly VPNService _vpnService = new VPNService();

        public VPNPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al inicializar VPNPage: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = false;
                StatusText.Text = "Buscando VPN/Proxy...";
                
                try
                {
                    var results = await _vpnService.SearchVPNAsync();
                    ResultsGrid.ItemsSource = results;
                    ResultsCount.Text = $"{results.Count} resultados encontrados";
                    StatusText.Text = $"Búsqueda completada - {results.Count} VPN/Proxy encontrados";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Error: {ex.Message}";
                    MessageBox.Show($"Error al buscar VPN/Proxy: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private async void AnalyzePortsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = false;
                StatusText.Text = "Analizando puertos sospechosos...";
                
                try
                {
                    var results = await _vpnService.SearchVPNAsync();
                    ResultsGrid.ItemsSource = results;
                    ResultsCount.Text = $"{results.Count} resultados encontrados";
                    StatusText.Text = $"Análisis completado - {results.Count} conexiones encontradas";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Error: {ex.Message}";
                    MessageBox.Show($"Error al analizar puertos: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            }
        }
    }
}

