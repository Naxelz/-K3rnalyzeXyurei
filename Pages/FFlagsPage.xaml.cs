using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Yurei.Services;

namespace Yurei.Pages
{
    public partial class FFlagsPage : UserControl
    {
        private readonly FFlagsService _fflagsService = new FFlagsService();
        private System.Windows.Threading.DispatcherTimer? _monitorTimer;

        public FFlagsPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al inicializar FFlagsPage: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = false;
                StatusText.Text = "Buscando FFlags...";
                
                try
                {
                    var results = await _fflagsService.SearchFFlagsAsync();
                    ResultsGrid.ItemsSource = results;
                    ResultsCount.Text = $"{results.Count} resultados encontrados";
                    StatusText.Text = $"Búsqueda completada - {results.Count} FFlags encontrados";
                }
                catch (Exception ex)
                {
                    StatusText.Text = $"Error: {ex.Message}";
                    MessageBox.Show($"Error al buscar FFlags: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private void MonitorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (_monitorTimer == null)
                {
                    
                    _monitorTimer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMinutes(5) 
                    };
                    _monitorTimer.Tick += async (s, args) => await PerformSearch();
                    _monitorTimer.Start();
                    
                    btn.Content = "⏸️ Detener Monitoreo";
                    StatusText.Text = "Monitoreo activo (cada 5 minutos)";
                }
                else
                {
                    
                    _monitorTimer.Stop();
                    _monitorTimer = null;
                    
                    btn.Content = "🔄 Monitoreo 24/7";
                    StatusText.Text = "Monitoreo detenido";
                }
            }
        }

        private async System.Threading.Tasks.Task PerformSearch()
        {
            try
            {
                var results = await _fflagsService.SearchFFlagsAsync();
                ResultsGrid.ItemsSource = results;
                StatusText.Text = $"Última búsqueda: {DateTime.Now:HH:mm:ss} - {results.Count} resultados";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error en monitoreo: {ex.Message}";
            }
        }
    }
}

