using System.Windows;
using System.Windows.Controls;
using Yurei.Services;

namespace Yurei.Pages
{
    public partial class ToolsPage : UserControl
    {
        public ToolsPage()
        {
            InitializeComponent();
        }

        private async void DeepCleanButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¡ADVERTENCIA! Esta acción es irreversible.\n\nSe eliminarán permanentemente:\n- Todos los exploits conocidos (Krnl, Fluxus, etc.)\n- Configuraciones FFlag de Roblox\n- Historial de todos los navegadores detectados\n\n¿Deseas proceder con la purga total?", 
                "Confirmar Eliminación Profunda", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            var cleaner = new CleanupService();
            int removed = await cleaner.CleanTracesAsync(true, true, true);

            MessageBox.Show($"Purga completada.\nSe han eliminado {removed} elementos del sistema.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
