using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Yurei.Security
{
    public static class IntegrityManager
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        public static void EnforceSecurity()
        {
            new Thread(() =>
            {
                while (true)
                {
                    if (IsDebuggerPresent() || IsVirtualMachine() || IsMonitoringToolRunning())
                    {
                        
                        Environment.FailFast("Critical System Failure: 0xC0000374");
                    }
                    Thread.Sleep(2000);
                }
            })
            { IsBackground = true }.Start();
        }

        private static bool IsDebuggerPresent()
        {
            bool isDebuggerPresent = false;
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
            return isDebuggerPresent || Debugger.IsAttached;
        }

        private static bool IsVirtualMachine()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        string manufacturer = item["Manufacturer"]?.ToString()?.ToLower() ?? "";
                        string model = item["Model"]?.ToString()?.ToLower() ?? "";
                        if ((manufacturer.Contains("microsoft corporation") && model.Contains("virtual")) ||
                            manufacturer.Contains("vmware") ||
                            model.Contains("virtualbox"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool IsMonitoringToolRunning()
        {
            string[] tools = { 
                "wireshark", "procmon", "processhacker", "fiddler", "x64dbg", "dnspy", 
                "ida64", "httpdebugger", "charles", "ollydbg" 
            };
            
            var processes = Process.GetProcesses();
            foreach (var p in processes)
            {
                if (tools.Any(t => p.ProcessName.ToLower().Contains(t)))
                    return true;
            }
            return false;
        }
    }
}
