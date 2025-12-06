using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Text;
using Yurei.Services;

namespace Yurei.Pages
{
    public partial class LookupsPage : UserControl
    {
        private List<LookupResult> _allResults = new List<LookupResult>();
        private readonly FFlagsService _fflags = new FFlagsService();
        private readonly ExploitsService _exploits = new ExploitsService();
        private readonly BrowserHistoryService _browser = new BrowserHistoryService();

        public LookupsPage()
        {
            InitializeComponent();
        }

        private async void RunSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Ejecutando escaneo forense...";
                _allResults.Clear();
                ResultsGrid.ItemsSource = null;
                ChartCanvas.Children.Clear();

                var tasks = new List<Task>();
                int countFF = 0, countEx = 0, countBr = 0;

                if (OptFFlags.IsChecked == true)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var ff = await _fflags.SearchFFlagsAsync();
                        countFF = ff.Count;
                        foreach (var f in ff.Take(200))
                            _allResults.Add(new LookupResult { 
                                Type = "FFlags", 
                                Information = f.Description, 
                                Source = f.Location,
                                Date = DateTime.Now.ToString("g"),
                                Details = f.Value
                            });
                    }));
                }

                if (OptExploits.IsChecked == true)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var ex = await _exploits.SearchExploitsAsync();
                        countEx = ex.Count;
                        foreach (var e in ex.Take(200))
                            _allResults.Add(new LookupResult { 
                                Type = "Exploits", 
                                Information = e.Description, 
                                Source = e.Source,
                                Date = DateTime.Now.ToString("g"),
                                Details = e.Status
                            });
                    }));
                }

                if (OptBrowser.IsChecked == true)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        var searches = _browser.SearchRobloxQueries();
                        var history = _browser.SearchRobloxHistory();
                        countBr = searches.Count + history.Count;
                        foreach (var s in searches.Take(200))
                            _allResults.Add(new LookupResult { 
                                Type = "Browsers", 
                                Information = $"Search: {s.Q}", 
                                Source = s.Browser,
                                Date = "Reciente",
                                Details = "Query de búsqueda"
                            });
                        foreach (var h in history.Take(200))
                            _allResults.Add(new LookupResult { 
                                Type = "Browsers", 
                                Information = $"URL: {h.Url}", 
                                Source = h.Browser,
                                Date = h.LastVisit != DateTime.MinValue ? h.LastVisit.ToString("g") : "N/A",
                                Details = $"Visitas: {h.VisitCount}"
                            });
                    }));
                }

                await Task.WhenAll(tasks);

                ResultsGrid.ItemsSource = _allResults;
                DrawChart();
                DrawComparisonChart(countFF, countEx, countBr);
                StatusText.Text = $"Análisis completado. Hallazgos: {countFF + countEx + countBr}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private async void SendToDiscordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_allResults.Count == 0)
            {
                MessageBox.Show("No hay resultados para reportar. Ejecuta un escaneo primero.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(DiscordWebhookService.WebhookUrl))
            {
                MessageBox.Show("Configura la Webhook URL en Ajustes primero.", "Configuración requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusText.Text = "Generando archivos de reporte...";

            var fflags = _allResults.Where(r => r.Type == "FFlags").ToList();
            var exploits = _allResults.Where(r => r.Type == "Exploits").ToList();
            var browsers = _allResults.Where(r => r.Type == "Browsers").ToList();

            var filePaths = new List<string>();
            var tempPath = System.IO.Path.GetTempPath();

            
            string CreateLogFile(string name, List<LookupResult> data)
            {
                if (data.Count == 0) return null;
                var path = System.IO.Path.Combine(tempPath, $"Yurei_{name}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                var sb = new StringBuilder();
                sb.AppendLine($"--- Yurei Scan Report: {name} ---");
                sb.AppendLine($"Date: {DateTime.Now:G}");
                sb.AppendLine($"Total Items: {data.Count}");
                sb.AppendLine(new string('-', 50));
                
                foreach (var item in data)
                {
                    sb.AppendLine($"[{item.Date}] {item.Information}");
                    sb.AppendLine($"    Source: {item.Source}");
                    sb.AppendLine($"    Details: {item.Details}");
                    sb.AppendLine();
                }
                
                System.IO.File.WriteAllText(path, sb.ToString());
                return path;
            }

            try
            {
                var fPath = CreateLogFile("FFlags", fflags);
                if (fPath != null) filePaths.Add(fPath);

                var ePath = CreateLogFile("Exploits", exploits);
                if (ePath != null) filePaths.Add(ePath);

                var bPath = CreateLogFile("Browsers", browsers);
                if (bPath != null) filePaths.Add(bPath);

                StatusText.Text = "Enviando reporte a Discord...";
                
                var service = new DiscordWebhookService();
                await service.SendScanReportAsync(_allResults, fflags.Count, exploits.Count, browsers.Count, filePaths);
                
                StatusText.Text = "Reporte enviado exitosamente.";
                MessageBox.Show("Reporte enviado a Discord con logs adjuntos.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar reporte: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                
                foreach (var p in filePaths)
                {
                    try { if (System.IO.File.Exists(p)) System.IO.File.Delete(p); } catch { }
                }
            }
        }

        private async void CleanAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Estás seguro de eliminar todos los rastros encontrados? Esto puede cerrar navegadores y eliminar configuraciones.", 
                "Confirmar Limpieza", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            StatusText.Text = "Limpiando rastros...";
            var cleaner = new CleanupService();
            
            var cleanFF = OptFFlags.IsChecked == true;
            var cleanEx = OptExploits.IsChecked == true;
            var cleanBr = OptBrowser.IsChecked == true;

            int removed = await cleaner.CleanTracesAsync(cleanBr, cleanEx, cleanFF);

            StatusText.Text = $"Limpieza completada. {removed} elementos eliminados.";
            MessageBox.Show($"Limpieza completada.\nSe eliminaron {removed} rastros/archivos.", "Limpieza", MessageBoxButton.OK, MessageBoxImage.Information);
            
            
            _allResults.Clear();
            ResultsGrid.ItemsSource = null;
            DrawChart();
        }

        private void UpdateResults(List<LookupResult> results)
        {
            
            
            _allResults.AddRange(results);
            ResultsGrid.ItemsSource = null; 
            ResultsGrid.ItemsSource = _allResults;
            DrawChart();
        }

        private void DrawChart()
        {
            ChartCanvas.Children.Clear();

            if (_allResults.Count == 0)
            {
                var text = new TextBlock
                {
                    Text = "No hay datos para mostrar",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8a8aa8")),
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Canvas.SetLeft(text, ChartCanvas.ActualWidth / 2 - 100);
                Canvas.SetTop(text, ChartCanvas.ActualHeight / 2);
                ChartCanvas.Children.Add(text);
                return;
            }

            
            var grouped = _allResults.GroupBy(r => r.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            if (grouped.Count == 0) return;

            var maxCount = grouped.Max(x => x.Count);
            var barWidth = 60;
            var maxBarHeight = 200;
            var spacing = 20;
            var startX = 30;
            var startY = 250;

            
            for (int i = 0; i < grouped.Count && i < 5; i++)
            {
                var item = grouped[i];
                var barHeight = maxCount > 0 ? (item.Count / (double)maxCount) * maxBarHeight : 0;
                var x = startX + i * (barWidth + spacing);

                
                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = GetColorForIndex(i),
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, startY - barHeight);
                ChartCanvas.Children.Add(bar);

                
                var valueText = new TextBlock
                {
                    Text = item.Count.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(valueText, x + barWidth / 2 - 10);
                Canvas.SetTop(valueText, startY - barHeight - 20);
                ChartCanvas.Children.Add(valueText);

                
                var typeText = new TextBlock
                {
                    Text = item.Type,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b8b8d4")),
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Canvas.SetLeft(typeText, x + barWidth / 2 - 20);
                Canvas.SetTop(typeText, startY + 5);
                ChartCanvas.Children.Add(typeText);
            }

            
            var mostFound = grouped.FirstOrDefault();
            ChartStats.Text = $"Total: {_allResults.Count} | Más encontrado: {mostFound?.Type ?? "N/A"} ({mostFound?.Count ?? 0})";
        }

        private void DrawComparisonChart(int countFF, int countEx, int countBr)
        {
            ChartCanvas.Children.Clear();
            var categories = new[]
            {
                new { Name = "FFlags", Value = countFF, Color = (Color)ColorConverter.ConvertFromString("#89B4FA") },
                new { Name = "Exploits", Value = countEx, Color = (Color)ColorConverter.ConvertFromString("#F38BA8") },
                new { Name = "Browsers", Value = countBr, Color = (Color)ColorConverter.ConvertFromString("#74C7EC") }
            };

            var max = Math.Max(1, categories.Max(c => c.Value));
            var barWidth = 80; var spacing = 40; var maxBarHeight = 220; var startX = 40; var startY = 260;

            for (int i = 0; i < categories.Length; i++)
            {
                var c = categories[i];
                var h = (c.Value / (double)max) * maxBarHeight;
                var x = startX + i * (barWidth + spacing);

                var rect = new Rectangle { Width = barWidth, Height = h, Fill = new SolidColorBrush(c.Color), RadiusX = 6, RadiusY = 6 };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, startY - h);
                ChartCanvas.Children.Add(rect);

                var title = new TextBlock { Text = c.Name, Foreground = Brushes.White, FontSize = 12 };
                Canvas.SetLeft(title, x + barWidth/2 - 24); Canvas.SetTop(title, startY + 6);
                ChartCanvas.Children.Add(title);

                var val = new TextBlock { Text = c.Value.ToString(), Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12 };
                Canvas.SetLeft(val, x + barWidth/2 - 12); Canvas.SetTop(val, startY - h - 22);
                ChartCanvas.Children.Add(val);
            }

            ChartStats.Text = $"Total: {categories.Sum(c=>c.Value)} | Más encontrado: {categories.OrderByDescending(c=>c.Value).First().Name}";
        }

        private Brush GetColorForIndex(int index)
        {
            var colors = new[]
            {
                (Color)ColorConverter.ConvertFromString("#00d4ff"), 
                (Color)ColorConverter.ConvertFromString("#8338ec"), 
                (Color)ColorConverter.ConvertFromString("#ff006e"), 
                (Color)ColorConverter.ConvertFromString("#3a86ff"), 
                (Color)ColorConverter.ConvertFromString("#06ffa5")  
            };
            return new SolidColorBrush(colors[index % colors.Length]);
        }
    }

    public class LookupResult
    {
        public string Type { get; set; } = "";
        public string Information { get; set; } = "";
        public string Source { get; set; } = "";
        public string Date { get; set; } = "";
        public string Details { get; set; } = "";
    }
}

