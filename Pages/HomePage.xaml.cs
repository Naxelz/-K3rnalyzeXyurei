using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading.Tasks;
using Yurei.Services;

namespace Yurei.Pages
{
    public partial class HomePage : UserControl
    {
        private DispatcherTimer _monitorTimer;
        private readonly FFlagsService _fflags = new FFlagsService();
        private readonly ExploitsService _exploits = new ExploitsService();
        private readonly BrowserHistoryService _browser = new BrowserHistoryService();

        public HomePage()
        {
            InitializeComponent();
            InitializeFeatureCards();
            StartRealtimeMonitoring();
        }

        private void StartRealtimeMonitoring()
        {
            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromSeconds(10); 
            _monitorTimer.Tick += async (s, e) => await UpdateStats();
            _monitorTimer.Start();
            
            
            _ = UpdateStats();
        }

        private async Task UpdateStats()
        {
            try
            {
                var ffTask = Task.Run(async () => (await _fflags.SearchFFlagsAsync()).Count);
                var exTask = Task.Run(async () => (await _exploits.SearchExploitsAsync()).Count);
                var brTask = Task.Run(() => _browser.SearchRobloxQueries().Count + _browser.SearchRobloxHistory().Count);

                await Task.WhenAll(ffTask, exTask, brTask);

                FFlagsCount.Text = $"{ffTask.Result} encontrados";
                ExploitsCount.Text = $"{exTask.Result} encontrados";
                BrowsersCount.Text = $"{brTask.Result} encontrados";
            }
            catch { }
        }

        public class FeatureCard
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Icon { get; set; } = "";
            public System.Windows.Media.Brush IconBackground { get; set; }
        }

        private void InitializeFeatureCards()
        {
            var cards = new System.Collections.Generic.List<FeatureCard>
            {
                new FeatureCard 
                { 
                    Title = "Escáner de Exploits", 
                    Description = "Detecta inyecciones, ejecutables y procesos sospechosos en tiempo real.",
                    Icon = "⚡",
                    IconBackground = (System.Windows.Media.Brush)FindResource("NeonPinkBrush")
                },
                new FeatureCard 
                { 
                    Title = "Análisis de FFlags", 
                    Description = "Verifica configuraciones modificadas y archivos JSON de Roblox.",
                    Icon = "⚙️",
                    IconBackground = (System.Windows.Media.Brush)FindResource("NeonCyanBrush")
                },
                new FeatureCard 
                { 
                    Title = "Rastreo de Red", 
                    Description = "Identifica uso de VPNs, Proxies y conexiones no autorizadas.",
                    Icon = "🔒",
                    IconBackground = (System.Windows.Media.Brush)FindResource("NeonBlueBrush")
                }
            };
            
            FeatureCards.ItemsSource = cards;
        }

        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderThickness = new Thickness(2);
                border.Opacity = 1.0;
            }
        }

        private void Card_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderThickness = new Thickness(1);
                border.Opacity = 0.9;
            }
        }
    }
}
