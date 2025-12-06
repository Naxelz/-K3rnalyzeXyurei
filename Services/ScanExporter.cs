using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yurei.Services
{
    public class ScanExporter
    {
        private readonly BrowserHistoryService _browser = new BrowserHistoryService();
        private readonly ExploitsService _exploits = new ExploitsService();
        private readonly FFlagsService _fflags = new FFlagsService();

        public async Task<(string path, object payload)> ExportAsync(string? targetPath = null)
        {
            var searches = _browser.SearchRobloxQueries();
            var history = _browser.SearchRobloxHistory();
            var exploits = await _exploits.SearchExploitsAsync();
            var ff = await _fflags.SearchFFlagsAsync();

            var payload = new
            {
                browser = new
                {
                    searches = searches.Select(s => new { browser = s.Browser, q = s.Q }).ToList(),
                    history = history.Select(h => new { browser = h.Browser, url = h.Url, title = h.Title }).ToList()
                },
                exploits = new
                {
                    urls = exploits.Where(e => e.Type == "PC Exploit" || e.Source.Contains("WEAO", StringComparison.OrdinalIgnoreCase))
                                   .Select(e => new { name = e.Name, url = e.Path, source = e.Source }).ToList()
                },
                fflags = new
                {
                    findings = ff.Select(f => new { key = f.Name, value = f.Value, path = f.Location }).ToList()
                },
                vpn = new
                {
                    active = false,
                    providers = new List<string>()
                },
                system = new
                {
                    os = Environment.OSVersion.VersionString,
                    installDate = "",
                    bootTime = "",
                    country = ""
                }
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            var path = targetPath ?? Path.Combine(AppContext.BaseDirectory, "scan.json");
            try
            {
                File.WriteAllText(path, json);
            }
            catch
            {
                path = Path.Combine(Directory.GetCurrentDirectory(), "scan.json");
                File.WriteAllText(path, json);
            }

            return (path, payload);
        }
    }
}
