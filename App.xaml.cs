using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Yurei
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            
            Yurei.Security.IntegrityManager.EnforceSecurity();

            base.OnStartup(e);
            
            
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMsg = $"Error no controlado:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}";
            
            
            try
            {
                File.AppendAllText("error.log", $"[{DateTime.Now}] {errorMsg}\n\n");
            }
            catch { }
            
            MessageBox.Show(errorMsg, "Error crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; 
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string errorMsg = $"Error fatal: {e.ExceptionObject}";
            
            try
            {
                File.AppendAllText("error.log", $"[{DateTime.Now}] FATAL: {errorMsg}\n\n");
            }
            catch { }
        }
    }
}

