using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Yurei.Services
{
    public class DiscordWebhookService
    {
        private static readonly HttpClient _client = new HttpClient();

        public static string WebhookUrl { get; set; } = "";

        public async Task SendScanReportAsync(List<Pages.LookupResult> results, int fflagsCount, int exploitsCount, int browsersCount, List<string>? filePaths = null)
        {
            if (string.IsNullOrWhiteSpace(WebhookUrl)) return;

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new MultipartFormDataContent();

                    
                    var fflagsTop = results.Where(r => r.Type == "FFlags").Take(10).Select(r => $"`{r.Information}`").ToList();
                    var exploitsTop = results.Where(r => r.Type == "Exploits").Take(10).Select(r => $"`{r.Information}`").ToList();
                    var browsersTop = results.Where(r => r.Type == "Browsers").Take(10).Select(r => $"`{r.Information}`").ToList();

                    var fieldsList = new List<object>
                    {
                        new { name = "⚙️ FFlags", value = $"Found: **{fflagsCount}**", inline = true },
                        new { name = "💻 Exploits", value = $"Found: **{exploitsCount}**", inline = true },
                        new { name = "🌐 Browsers", value = $"Found: **{browsersCount}**", inline = true }
                    };

                    if (fflagsTop.Any()) fieldsList.Add(new { name = "Top 10 FFlags", value = string.Join("\n", fflagsTop), inline = false });
                    if (exploitsTop.Any()) fieldsList.Add(new { name = "Top 10 Exploits", value = string.Join("\n", exploitsTop), inline = false });
                    if (browsersTop.Any()) fieldsList.Add(new { name = "Top 10 Browser Activity", value = string.Join("\n", browsersTop), inline = false });

                    var embed = new
                    {
                        title = "👻 Yurei Scan Report",
                        description = $"**Total Findings:** {results.Count}\nScan Time: {DateTime.Now:g}\n\n✅ **Detailed logs attached below.**",
                        color = 5814783,
                        fields = fieldsList,
                        footer = new { text = "Ocean Security • Integrity Validation" }
                    };

                    var payload = new
                    {
                        username = "Yurei Scanner",
                        avatar_url = "https://i.imgur.com/4M34hi2.png",
                        embeds = new[] { embed }
                    };

                    var json = JsonSerializer.Serialize(payload);
                    content.Add(new StringContent(json, Encoding.UTF8, "application/json"), "payload_json");

                    
                    if (filePaths != null)
                    {
                        foreach (var path in filePaths)
                        {
                            if (System.IO.File.Exists(path))
                            {
                                var fileBytes = System.IO.File.ReadAllBytes(path);
                                var fileContent = new ByteArrayContent(fileBytes);
                                content.Add(fileContent, $"files[{filePaths.IndexOf(path)}]", System.IO.Path.GetFileName(path));
                            }
                        }
                    }

                    var response = await client.PostAsync(WebhookUrl, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
