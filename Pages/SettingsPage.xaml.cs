using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Yurei.Services;

namespace Yurei.Pages
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
            WebhookBox.Text = DiscordWebhookService.WebhookUrl;
            WebhookBox.TextChanged += (s, e) => DiscordWebhookService.WebhookUrl = WebhookBox.Text;
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.SelectedIndex == 0) 
            {
                ApplyTheme("#1E1E2E", "#181825", "#313244", "#CDD6F4", "#A6ADC8");
            }
            else 
            {
                ApplyTheme("#EFF1F5", "#E6E9EF", "#CCD0DA", "#4C4F69", "#6C6F85");
            }
        }

        private void ApplyTheme(string baseCol, string surfaceCol, string cardCol, string textCol, string subTextCol)
        {
            var dict = Application.Current.Resources;
            
            
            void SetColor(string key, string hex)
            {
                try { dict[key] = (Color)ColorConverter.ConvertFromString(hex); } catch { }
            }
            
            
            void SetBrush(string key, string hex)
            {
                try { dict[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)); } catch { }
            }

            
            SetBrush("BackgroundBrush", baseCol);
            SetBrush("SurfaceBrush", surfaceCol);
            SetBrush("CardBrush", cardCol);
            
            
            SetBrush("TextPrimaryBrush", textCol);
            SetBrush("TextSecondaryBrush", subTextCol);
        }
    }
}
