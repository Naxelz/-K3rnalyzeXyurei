using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace Yurei.Services
{
    public class FFlagsService
    {
        private readonly BrowserHistoryService _historyService = new BrowserHistoryService();
        
        private readonly List<string> _knownFFlagPaths = new List<string>
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Roblox",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Roblox",
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Roblox",
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Roblox",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Local\Roblox",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Roblox",
            @"C:\Program Files\Roblox",
            @"C:\Program Files (x86)\Roblox",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Desktop",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents"
        };

        private readonly List<string> _knownLaunchers = new List<string>
        {
            "voidtrap", "bloxtrap", "fishtrap", "roblox", "robloxplayerbeta",
            "robloxplayer", "robloxstudio", "robloxstudiobeta", "robloxapp"
        };

        private readonly List<string> _fflagKeywords = new List<string>
        {
            "FFlag", "FString", "FInt", "FBool", "FastFlag", "FeatureFlag",
            "ClientSettings", "appsettings", "GlobalSettings", "ClientAppSettings"
        };

        public async Task<List<FFlagResult>> SearchFFlagsAsync()
        {
            return await Task.Run(() =>
            {
                var results = new List<FFlagResult>();

                
                results.AddRange(SearchRegistryExhaustive());

                
                results.AddRange(SearchFilesExhaustive());

                
                results.AddRange(SearchProcesses());

                
                results.AddRange(SearchBrowserHistory());

                
                results.AddRange(SearchEnvironmentVariables());

                
                results.AddRange(SearchSystemConfigFiles());

                
                results.AddRange(SearchLogs());

                
                results.AddRange(SearchProcessMemory());

                return results.Distinct().ToList();
            });
        }

        private List<FFlagResult> SearchRegistryExhaustive()
        {
            var results = new List<FFlagResult>();
            var registryPaths = new[]
            {
                @"Software\Roblox",
                @"Software\Classes\roblox-player",
                @"Software\Classes\roblox",
                @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\Roblox",
                @"SOFTWARE\Classes\roblox-player",
                @"SOFTWARE\WOW6432Node\Roblox"
            };

            foreach (var regPath in registryPaths)
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(regPath))
                    {
                        if (key != null)
                        {
                            SearchRegistryKey(key, results, $"HKCU\\{regPath}");
                            SearchRegistrySubKeys(key, results, $"HKCU\\{regPath}");
                        }
                    }

                    using (var key = Registry.LocalMachine.OpenSubKey(regPath))
                    {
                        if (key != null)
                        {
                            SearchRegistryKey(key, results, $"HKLM\\{regPath}");
                            SearchRegistrySubKeys(key, results, $"HKLM\\{regPath}");
                        }
                    }
                }
                catch { }
            }

            return results;
        }

        private void SearchRegistryKey(RegistryKey key, List<FFlagResult> results, string location)
        {
            try
            {
                foreach (var valueName in key.GetValueNames())
                {
                    if (_fflagKeywords.Any(k => valueName.Contains(k, StringComparison.OrdinalIgnoreCase)) ||
                        valueName.Contains("roblox", StringComparison.OrdinalIgnoreCase))
                    {
                        var value = key.GetValue(valueName)?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(value))
                        {
                            results.Add(new FFlagResult
                            {
                                Name = valueName,
                                Value = value,
                                Location = location,
                                Type = "Registry Value"
                            });
                        }
                    }
                }
            }
            catch { }
        }

        private void SearchRegistrySubKeys(RegistryKey parentKey, List<FFlagResult> results, string baseLocation)
        {
            try
            {
                foreach (var subKeyName in parentKey.GetSubKeyNames())
                {
                    using (var subKey = parentKey.OpenSubKey(subKeyName))
                    {
                        if (subKey != null)
                        {
                            SearchRegistryKey(subKey, results, $"{baseLocation}\\{subKeyName}");
                            SearchRegistrySubKeys(subKey, results, $"{baseLocation}\\{subKeyName}");
                        }
                    }
                }
            }
            catch { }
        }

        private List<FFlagResult> SearchFilesExhaustive()
        {
            var results = new List<FFlagResult>();

            foreach (var path in _knownFFlagPaths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        SearchDirectoryRecursive(path, results, 5); 
                    }
                }
                catch { }
            }

            
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
            foreach (var drive in drives.Take(1)) 
            {
                try
                {
                    var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    if (drive.RootDirectory.FullName.StartsWith(userProfile.Substring(0, 3)))
                    {
                        SearchDirectoryRecursive(userProfile, results, 3);
                    }
                }
                catch { }
            }

            return results;
        }

        private void SearchDirectoryRecursive(string directory, List<FFlagResult> results, int maxDepth, int currentDepth = 0)
        {
            if (currentDepth >= maxDepth) return;

            try
            {
                var files = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly)
                    .Where(f => _fflagKeywords.Any(k => Path.GetFileName(f).Contains(k, StringComparison.OrdinalIgnoreCase)) ||
                               f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".config", StringComparison.OrdinalIgnoreCase) ||
                               f.EndsWith(".ini", StringComparison.OrdinalIgnoreCase) ||
                               f.Contains("roblox", StringComparison.OrdinalIgnoreCase))
                    .Take(50);

                foreach (var file in files)
                {
                    try
                    {
                        var content = File.ReadAllText(file);
                        if (_fflagKeywords.Any(k => content.Contains(k, StringComparison.OrdinalIgnoreCase)) ||
                            content.Contains("roblox", StringComparison.OrdinalIgnoreCase))
                        {
                            var matches = Regex.Matches(content, @"(FFlag|FString|FInt|FBool)[A-Za-z0-9_]+", RegexOptions.IgnoreCase);
                            foreach (Match match in matches)
                            {
                                results.Add(new FFlagResult
                                {
                                    Name = match.Value,
                                    Value = file,
                                    Location = Path.GetDirectoryName(file) ?? "",
                                    Type = "File Content"
                                });
                            }

                            results.Add(new FFlagResult
                            {
                                Name = Path.GetFileName(file),
                                Value = file,
                                Location = Path.GetDirectoryName(file) ?? "",
                                Type = "Config File"
                            });
                        }
                    }
                    catch { }
                }

                var dirs = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly)
                    .Where(d => !d.Contains("Windows") && !d.Contains("Program Files") && 
                               !d.Contains("$Recycle.Bin") && !d.Contains("System Volume Information"))
                    .Take(20);

                foreach (var dir in dirs)
                {
                    SearchDirectoryRecursive(dir, results, maxDepth, currentDepth + 1);
                }
            }
            catch { }
        }

        private List<FFlagResult> SearchProcesses()
        {
            var results = new List<FFlagResult>();

            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => _knownLaunchers.Any(launcher => 
                        p.ProcessName.Contains(launcher, StringComparison.OrdinalIgnoreCase)));

                foreach (var process in processes)
                {
                    try
                    {
                        results.Add(new FFlagResult
                        {
                            Name = process.ProcessName,
                            Value = process.MainModule?.FileName ?? "",
                            Location = "Running Process",
                            Type = "Process"
                        });

                        var cmdLine = GetCommandLine(process.Id);
                        if (!string.IsNullOrEmpty(cmdLine) && 
                            _fflagKeywords.Any(k => cmdLine.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        {
                            results.Add(new FFlagResult
                            {
                                Name = $"{process.ProcessName} - Command Line",
                                Value = cmdLine,
                                Location = "Process Arguments",
                                Type = "Process Args"
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return results;
        }

        private string GetCommandLine(int processId)
        {
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        return obj["CommandLine"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

        private List<FFlagResult> SearchBrowserHistory()
        {
            var results = new List<FFlagResult>();

            try
            {
                var history = _historyService.SearchRobloxHistory();
                foreach (var item in history.Where(h => 
                    h.Url.Contains("fflag", StringComparison.OrdinalIgnoreCase) ||
                    h.Url.Contains("fastflag", StringComparison.OrdinalIgnoreCase) ||
                    h.Title.Contains("fflag", StringComparison.OrdinalIgnoreCase)))
                {
                    results.Add(new FFlagResult
                    {
                        Name = item.Title,
                        Value = item.Url,
                        Location = $"Browser History ({item.Browser})",
                        Type = "History"
                    });
                }
            }
            catch { }

            return results;
        }

        private List<FFlagResult> SearchEnvironmentVariables()
        {
            var results = new List<FFlagResult>();

            try
            {
                foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
                {
                    var keyObj = entry.Key;
                    var valueObj = entry.Value;
                    
                    var key = keyObj != null ? keyObj.ToString() ?? "" : "";
                    var value = valueObj != null ? valueObj.ToString() ?? "" : "";
                    
                    if (!string.IsNullOrEmpty(key) && 
                        (_fflagKeywords.Any(k => key.Contains(k, StringComparison.OrdinalIgnoreCase)) ||
                        _fflagKeywords.Any(k => value.Contains(k, StringComparison.OrdinalIgnoreCase)) ||
                        key.Contains("roblox", StringComparison.OrdinalIgnoreCase)))
                    {
                        results.Add(new FFlagResult
                        {
                            Name = key,
                            Value = value,
                            Location = "Environment Variable",
                            Type = "Env Var"
                        });
                    }
                }
            }
            catch { }

            return results;
        }

        private List<FFlagResult> SearchSystemConfigFiles()
        {
            var results = new List<FFlagResult>();
            var configPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\System32\config",
                Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SysWOW64\config"
            };

            foreach (var path in configPaths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                            .Where(f => f.Contains("roblox", StringComparison.OrdinalIgnoreCase) ||
                                       f.Contains("fflag", StringComparison.OrdinalIgnoreCase))
                            .Take(10);

                        foreach (var file in files)
                        {
                            results.Add(new FFlagResult
                            {
                                Name = Path.GetFileName(file),
                                Value = file,
                                Location = path,
                                Type = "System Config"
                            });
                        }
                    }
                }
                catch { }
            }

            return results;
        }

        private List<FFlagResult> SearchLogs()
        {
            var results = new List<FFlagResult>();
            var logPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Roblox\Logs",
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Roblox\Logs"
            };

            foreach (var path in logPaths)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        var files = Directory.GetFiles(path, "*.log", SearchOption.AllDirectories)
                            .Take(20);

                        foreach (var file in files)
                        {
                            try
                            {
                                var content = File.ReadAllText(file);
                                if (_fflagKeywords.Any(k => content.Contains(k, StringComparison.OrdinalIgnoreCase)))
                                {
                                    results.Add(new FFlagResult
                                    {
                                        Name = Path.GetFileName(file),
                                        Value = file,
                                        Location = path,
                                        Type = "Log File"
                                    });
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            return results;
        }

        private List<FFlagResult> SearchProcessMemory()
        {
            var results = new List<FFlagResult>();

            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => _knownLaunchers.Any(l => p.ProcessName.Contains(l, StringComparison.OrdinalIgnoreCase)));

                foreach (var process in processes)
                {
                    try
                    {
                        
                        foreach (ProcessModule module in process.Modules)
                        {
                            if (module.FileName.Contains("roblox", StringComparison.OrdinalIgnoreCase) ||
                                module.ModuleName.Contains("fflag", StringComparison.OrdinalIgnoreCase))
                            {
                                results.Add(new FFlagResult
                                {
                                    Name = $"{process.ProcessName} - {module.ModuleName}",
                                    Value = module.FileName,
                                    Location = "Process Memory",
                                    Type = "Loaded Module"
                                });
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            return results;
        }
    }

    public class FFlagResult
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
        public string Location { get; set; } = "";
        public string Type { get; set; } = "";
        
        public string TypeIcon
        {
            get
            {
                return Type switch
                {
                    "FastFlag" or "FFlag" => "⚙️",
                    "Registry Value" or "Registry" => "🔑",
                    "File" or "File Content" or "Config File" => "📄",
                    "Process" or "Process Args" => "🖥️",
                    "History" => "🌐",
                    "Env Var" => "🔧",
                    "System Config" => "⚙️",
                    "Log File" => "📋",
                    "Loaded Module" => "📦",
                    _ => "❓"
                };
            }
        }
        
        public string Description
        {
            get
            {
                if (Type == "FastFlag" || Type == "FFlag")
                    return $"FastFlag de Roblox encontrado en {Location}. Valor: {Value}";
                else if (Type == "Registry Value" || Type == "Registry")
                    return $"Valor en Registry de Windows. Puede afectar el comportamiento de Roblox.";
                else if (Type == "File" || Type == "File Content")
                    return $"Archivo de configuración que contiene FFlags. Ruta: {Value}";
                else if (Type == "Process")
                    return $"Proceso de Roblox en ejecución. Puede contener FFlags en memoria.";
                else if (Type == "History")
                    return $"URL relacionada con FFlags visitada en el navegador.";
                else if (Type == "Env Var")
                    return $"Variable de entorno del sistema relacionada con Roblox/FFlags.";
                else
                    return $"Elemento encontrado: {Type}";
            }
        }
    }
}
