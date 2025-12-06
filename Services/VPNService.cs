using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using Yurei.Services;

namespace Yurei.Services
{
    public class VPNService
    {
        private readonly BrowserHistoryService _historyService = new BrowserHistoryService();
        
        private readonly List<string> _knownVPNs = new List<string>
        {
            "nordvpn", "expressvpn", "surfshark", "cyberghost", "protonvpn",
            "windscribe", "tunnelbear", "private internet access", "clumsy",
            "proxifier", "sockscap", "shadowsocks", "openvpn", "wireguard",
            "vpn", "proxy", "tor", "torbrowser", "fiddler", "wireshark",
            "charles", "burp", "mitmproxy"
        };

        public async Task<List<VPNResult>> SearchVPNAsync()
        {
            return await Task.Run(() =>
            {
                var results = new List<VPNResult>();

                
                results.AddRange(SearchVPNProcesses());
                results.AddRange(SearchNetworkAdapters());
                results.AddRange(SearchRegistry());
                results.AddRange(SearchSuspiciousPorts());
                results.AddRange(SearchBrowserHistory());
                results.AddRange(SearchWindowsServices());
                results.AddRange(SearchNetworkInterfaces());
                results.AddRange(SearchFirewallRules());
                results.AddRange(SearchRoutingTable());
                results.AddRange(SearchDNS());

                return results.Distinct().ToList();
            });
        }

        private List<VPNResult> SearchVPNProcesses()
        {
            var results = new List<VPNResult>();

            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => _knownVPNs.Any(vpn => 
                        p.ProcessName.Contains(vpn, StringComparison.OrdinalIgnoreCase)));

                foreach (var process in processes)
                {
                    try
                    {
                        var cmdLine = GetCommandLine(process.Id);
                        results.Add(new VPNResult
                        {
                            Name = process.ProcessName,
                            Type = "VPN Process",
                            Status = "Running",
                            Path = !string.IsNullOrEmpty(cmdLine) ? cmdLine : (process.MainModule?.FileName ?? ""),
                            Location = "Process"
                        });
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
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return obj["CommandLine"]?.ToString() ?? "";
                    }
                }
            }
            catch { }
            return "";
        }

        private List<VPNResult> SearchNetworkAdapters()
        {
            var results = new List<VPNResult>();

            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => _knownVPNs.Any(vpn => 
                        ni.Description.Contains(vpn, StringComparison.OrdinalIgnoreCase)) ||
                        ni.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase) ||
                        ni.Description.Contains("TAP", StringComparison.OrdinalIgnoreCase) ||
                        ni.Description.Contains("TUN", StringComparison.OrdinalIgnoreCase) ||
                        ni.Description.Contains("OpenVPN", StringComparison.OrdinalIgnoreCase) ||
                        ni.Description.Contains("WireGuard", StringComparison.OrdinalIgnoreCase) ||
                        ni.Description.Contains("ProtonVPN", StringComparison.OrdinalIgnoreCase) ||
                        ni.Description.Contains("NordLynx", StringComparison.OrdinalIgnoreCase));

                foreach (var adapter in adapters)
                {
                    results.Add(new VPNResult
                    {
                        Name = adapter.Name,
                        Type = "Network Adapter",
                        Status = adapter.OperationalStatus.ToString(),
                        Path = adapter.Description,
                        Location = "Network Interface"
                    });
                }
            }
            catch { }

            return results;
        }

        private List<VPNResult> SearchRegistry()
        {
            var results = new List<VPNResult>();
            var registryPaths = new[]
            {
                @"SYSTEM\CurrentControlSet\Services",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var regPath in registryPaths)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(regPath))
                    {
                        if (key != null)
                        {
                            foreach (var subKeyName in key.GetSubKeyNames())
                            {
                                foreach (var vpn in _knownVPNs)
                                {
                                    if (subKeyName.Contains(vpn, StringComparison.OrdinalIgnoreCase))
                                    {
                                        using (var subKey = key.OpenSubKey(subKeyName))
                                        {
                                            var displayName = subKey?.GetValue("DisplayName")?.ToString() ?? subKeyName;
                                            var installLocation = subKey?.GetValue("InstallLocation")?.ToString() ?? "";

                                            results.Add(new VPNResult
                                            {
                                                Name = displayName,
                                                Type = regPath.Contains("Services") ? "System Service" : "Installed Software",
                                                Status = "Installed",
                                                Path = !string.IsNullOrEmpty(installLocation) ? installLocation : $@"HKLM\{regPath}\{subKeyName}",
                                                Location = "Registry"
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return results;
        }

        private List<VPNResult> SearchSuspiciousPorts()
        {
            var results = new List<VPNResult>();

            try
            {
                var connections = IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpConnections()
                    .Where(c => IsSuspiciousPort(c.LocalEndPoint.Port) || 
                               IsSuspiciousPort(c.RemoteEndPoint.Port));

                foreach (var conn in connections)
                {
                    results.Add(new VPNResult
                    {
                        Name = $"Port {conn.LocalEndPoint.Port}",
                        Type = "Network Connection",
                        Status = conn.State.ToString(),
                        Path = $"{conn.LocalEndPoint} -> {conn.RemoteEndPoint}",
                        Location = "TCP Connection"
                    });
                }

                var listeners = IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Where(l => IsSuspiciousPort(l.Port));

                foreach (var listener in listeners)
                {
                    results.Add(new VPNResult
                    {
                        Name = $"Listening Port {listener.Port}",
                        Type = "Network Listener",
                        Status = "Listening",
                        Path = listener.ToString(),
                        Location = "TCP Listener"
                    });
                }
            }
            catch { }

            return results;
        }

        private bool IsSuspiciousPort(int port)
        {
            
            var suspiciousPorts = new[] 
            { 
                1080, 3128, 8080, 8118, 8123, 8888, 9050, 1194, 1723, 4500, 500,
                9051, 9150, 9151, 4444, 5555, 6666, 7777, 9999, 10808, 10809,
                8081, 8082, 8889, 9998, 4145, 1080, 3128, 8080
            };
            return suspiciousPorts.Contains(port);
        }

        private List<VPNResult> SearchBrowserHistory()
        {
            var results = new List<VPNResult>();

            try
            {
                var history = _historyService.SearchRobloxHistory();
                foreach (var item in history.Where(h => 
                    _knownVPNs.Any(vpn => 
                        h.Url.Contains(vpn, StringComparison.OrdinalIgnoreCase) ||
                        h.Title.Contains(vpn, StringComparison.OrdinalIgnoreCase))))
                {
                    results.Add(new VPNResult
                    {
                        Name = item.Title,
                        Type = "Browser History",
                        Status = $"Visited {item.VisitCount} times",
                        Path = item.Url,
                        Location = item.Browser
                    });
                }
            }
            catch { }

            return results;
        }

        private List<VPNResult> SearchWindowsServices()
        {
            var results = new List<VPNResult>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service"))
                {
                    foreach (ManagementObject service in searcher.Get())
                    {
                        var name = service["Name"]?.ToString() ?? "";
                        var displayName = service["DisplayName"]?.ToString() ?? "";

                        if (_knownVPNs.Any(vpn => 
                            name.Contains(vpn, StringComparison.OrdinalIgnoreCase) ||
                            displayName.Contains(vpn, StringComparison.OrdinalIgnoreCase)))
                        {
                            results.Add(new VPNResult
                            {
                                Name = displayName,
                                Type = "Windows Service",
                                Status = service["State"]?.ToString() ?? "Unknown",
                                Path = service["PathName"]?.ToString() ?? "",
                                Location = "Services"
                            });
                        }
                    }
                }
            }
            catch { }

            return results;
        }

        private List<VPNResult> SearchNetworkInterfaces()
        {
            var results = new List<VPNResult>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter"))
                {
                    foreach (ManagementObject adapter in searcher.Get())
                    {
                        var name = adapter["Name"]?.ToString() ?? "";
                        var description = adapter["Description"]?.ToString() ?? "";

                        if (_knownVPNs.Any(vpn => 
                            name.Contains(vpn, StringComparison.OrdinalIgnoreCase) ||
                            description.Contains(vpn, StringComparison.OrdinalIgnoreCase)))
                        {
                            results.Add(new VPNResult
                            {
                                Name = name,
                                Type = "Network Adapter (WMI)",
                                Status = adapter["NetConnectionStatus"]?.ToString() ?? "Unknown",
                                Path = description,
                                Location = "WMI"
                            });
                        }
                    }
                }
            }
            catch { }

            return results;
        }

        private List<VPNResult> SearchFirewallRules()
        {
            var results = new List<VPNResult>();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_FirewallProduct"))
                {
                    foreach (ManagementObject firewall in searcher.Get())
                    {
                        var name = firewall["Name"]?.ToString() ?? "";
                        if (_knownVPNs.Any(vpn => name.Contains(vpn, StringComparison.OrdinalIgnoreCase)))
                        {
                            results.Add(new VPNResult
                            {
                                Name = name,
                                Type = "Firewall",
                                Status = "Active",
                                Path = firewall["Path"]?.ToString() ?? "",
                                Location = "Firewall"
                            });
                        }
                    }
                }
            }
            catch { }

            return results;
        }

        private List<VPNResult> SearchRoutingTable()
        {
            var results = new List<VPNResult>();

            try
            {
                var routes = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
                
                foreach (var route in routes.Where(r => 
                    r.RemoteEndPoint.Address.ToString().StartsWith("10.") ||
                    r.RemoteEndPoint.Address.ToString().StartsWith("172.16.") ||
                    r.RemoteEndPoint.Address.ToString().StartsWith("192.168.")))
                {
                    results.Add(new VPNResult
                    {
                        Name = $"Route to {route.RemoteEndPoint}",
                        Type = "Routing",
                        Status = route.State.ToString(),
                        Path = route.LocalEndPoint.ToString(),
                        Location = "Routing Table"
                    });
                }
            }
            catch { }

            return results;
        }

        private List<VPNResult> SearchDNS()
        {
            var results = new List<VPNResult>();

            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var adapter in adapters)
                {
                    var properties = adapter.GetIPProperties();
                    foreach (var dns in properties.DnsAddresses)
                    {
                        
                        if (dns.ToString().StartsWith("1.1.1.1") || 
                            dns.ToString().StartsWith("8.8.8.8") || 
                            dns.ToString().StartsWith("208.67.222.222")) 
                        {
                            results.Add(new VPNResult
                            {
                                Name = $"DNS Server: {dns}",
                                Type = "DNS Configuration",
                                Status = "Configured",
                                Path = adapter.Name,
                                Location = "DNS"
                            });
                        }
                    }
                }
            }
            catch { }

            return results;
        }
    }

    public class VPNResult
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
        public string Path { get; set; } = "";
        public string Location { get; set; } = "";
        
        public string TypeIcon
        {
            get
            {
                return Type switch
                {
                    "VPN Process" => "🔒",
                    "Network Adapter" => "🌐",
                    "System Service" or "Windows Service" => "⚙️",
                    "Network Connection" or "TCP Connection" => "🔌",
                    "Network Listener" or "TCP Listener" => "👂",
                    "Browser History" => "🌐",
                    "Installed Software" => "💾",
                    "Firewall" => "🔥",
                    "Routing" => "🛣️",
                    "DNS Configuration" or "DNS" => "🔍",
                    _ => "❓"
                };
            }
        }
        
        public string Description
        {
            get
            {
                if (Type == "VPN Process")
                    return $"⚠️ PROCESO VPN ACTIVO: {Name} está ejecutándose. Esto puede afectar la conexión a Roblox. Ruta: {Path}";
                else if (Type == "Network Adapter")
                    return $"🌐 Adaptador de red VPN detectado: {Name}. Estado: {Status}. Descripción: {Path}";
                else if (Type == "System Service" || Type == "Windows Service")
                    return $"⚙️ Servicio de VPN instalado: {Name}. Estado: {Status}. Ubicación: {Path}";
                else if (Type == "Network Connection" || Type == "TCP Connection")
                    return $"🔌 Conexión de red sospechosa detectada en puerto. {Path}. Estado: {Status}";
                else if (Type == "Network Listener" || Type == "TCP Listener")
                    return $"👂 Puerto escuchando conexiones: {Name}. Ruta: {Path}";
                else if (Type == "Browser History")
                    return $"🌐 Historial del navegador: Se visitó {Status}. URL: {Path}";
                else if (Type == "Installed Software")
                    return $"💾 Software VPN instalado: {Name}. Ubicación: {Path}";
                else if (Type == "Firewall")
                    return $"🔥 Regla de firewall relacionada con VPN: {Name}";
                else if (Type == "Routing")
                    return $"🛣️ Ruta de red sospechosa: {Name}. Estado: {Status}";
                else if (Type == "DNS Configuration" || Type == "DNS")
                    return $"🔍 Configuración DNS sospechosa: {Name}. Servidor: {Path}";
                else
                    return $"Elemento VPN/Proxy encontrado: {Type}. Estado: {Status}";
            }
        }
    }
}
