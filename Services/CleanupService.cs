using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Yurei.Services
{
    public class CleanupService
    {
        public async Task<int> CleanTracesAsync(bool cleanBrowsers, bool cleanExploits, bool cleanFFlags)
        {
            int itemsRemoved = 0;

            if (cleanExploits)
            {
                itemsRemoved += await Task.Run(() => CleanExploitTraces());
            }

            if (cleanFFlags)
            {
                itemsRemoved += await Task.Run(() => CleanFFlags());
            }

            if (cleanBrowsers)
            {
                itemsRemoved += await Task.Run(() => CleanBrowserHistoryFiles());
            }

            return itemsRemoved;
        }

        private int CleanExploitTraces()
        {
            int count = 0;
            var processes = Process.GetProcesses();
            var exploits = new[] { "krnl", "fluxus", "synapse", "oxygen", "electron", "comet" };

            
            foreach (var p in processes)
            {
                try
                {
                    if (IsExploitProcess(p.ProcessName, exploits))
                    {
                        p.Kill();
                        count++;
                    }
                }
                catch { }
            }

            
            var paths = new[] 
            { 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Krnl"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fluxus"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Synapse X")
            };

            foreach (var p in paths)
            {
                if (Directory.Exists(p))
                {
                    try { Directory.Delete(p, true); count++; } catch { }
                }
            }

            return count;
        }

        private bool IsExploitProcess(string name, string[] list)
        {
            foreach (var item in list)
                if (name.Contains(item, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private int CleanFFlags()
        {
            int count = 0;
            var robloxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Roblox\Versions");
            if (Directory.Exists(robloxPath))
            {
                foreach (var dir in Directory.GetDirectories(robloxPath))
                {
                    var settings = Path.Combine(dir, "ClientSettings", "ClientAppSettings.json");
                    if (File.Exists(settings))
                    {
                        try { File.Delete(settings); count++; } catch { }
                    }
                }
            }
            return count;
        }

        private int CleanBrowserHistoryFiles()
        {
            int count = 0;
            KillBrowsers();

            var historyPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\User Data\Default\History"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\User Data\Default\History"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"BraveSoftware\Brave-Browser\User Data\Default\History"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Opera Software\Opera GX Stable\History")
            };

            foreach (var p in historyPaths)
            {
                if (File.Exists(p))
                {
                    try { File.Delete(p); count++; } catch { }
                }
            }

            return count;
        }

        private void KillBrowsers()
        {
            var browsers = new[] { "chrome", "msedge", "brave", "opera", "firefox" };
            foreach (var p in Process.GetProcesses())
            {
                foreach (var b in browsers)
                {
                    if (p.ProcessName.ToLower().Contains(b))
                    {
                        try { p.Kill(); } catch { }
                    }
                }
            }
        }
    }
}
