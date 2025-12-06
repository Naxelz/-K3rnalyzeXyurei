using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Yurei.Services
{
    public class BrowserHistoryService
    {
        public List<HistoryResult> SearchRobloxHistory()
        {
            var results = new List<HistoryResult>();

            
            results.AddRange(SearchChromeHistory());
            
            
            results.AddRange(SearchEdgeHistory());
            
            
            results.AddRange(SearchFirefoxHistory());
            
            results.AddRange(SearchOperaHistory());

            results.AddRange(SearchOperaGXHistory());

            results.AddRange(SearchYandexHistory());
            
            results.AddRange(SearchBraveHistory());
            
            results.AddRange(SearchVivaldiHistory());
            
            results.AddRange(SearchChromiumHistory());

            results.AddRange(SearchGenericHistory());

            return results;
        }

        public List<SearchQuery> SearchRobloxQueries()
        {
            var queries = new List<SearchQuery>();

            void addFrom(List<HistoryResult> hist)
            {
                foreach (var h in hist)
                {
                    var q = ExtractSearchQuery(h.Url);
                    if (!string.IsNullOrWhiteSpace(q) && IsRelevant(q))
                    {
                        queries.Add(new SearchQuery { Browser = h.Browser, Q = q });
                    }
                }
            }

            addFrom(SearchChromeHistory());
            addFrom(SearchEdgeHistory());
            addFrom(SearchFirefoxHistory());
            addFrom(SearchOperaHistory());
            addFrom(SearchOperaGXHistory());
            addFrom(SearchYandexHistory());
            addFrom(SearchBraveHistory());
            addFrom(SearchVivaldiHistory());
            addFrom(SearchChromiumHistory());
            addFrom(SearchGenericHistory());

            return queries;
        }

        private static bool IsRelevant(string term)
        {
            term = term.ToLowerInvariant();
            return term.Contains("roblox") || term.Contains("exploit") || term.Contains("fflag");
        }

        private static string ExtractSearchQuery(string url)
        {
            try
            {
                var u = new Uri(url);
                var host = u.Host.ToLowerInvariant();
                var query = u.Query;
                var dict = System.Web.HttpUtility.ParseQueryString(query);
                string[] keys = new[] { "q", "query", "text", "search", "p" };
                foreach (var k in keys)
                {
                    var v = dict.Get(k);
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
                
                if (host.Contains("youtube.com"))
                {
                    var v = dict.Get("search_query");
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
                
                var m = Regex.Match(url, "[?&](q|query|text|search|p)=([^&#]+)");
                if (m.Success) return Uri.UnescapeDataString(m.Groups[2].Value);
                return string.Empty;
            }
            catch { return string.Empty; }
        }

        private List<HistoryResult> SearchChromeHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\History");

            if (File.Exists(historyPath))
            {
                try
                {
                    File.Copy(historyPath, historyPath + ".temp", true);
                    using (var connection = new SQLiteConnection($"Data Source={historyPath}.temp;Version=3;"))
                    {
                        connection.Open();
                        var query = @"SELECT url, title, visit_count, last_visit_time FROM urls 
                                     WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                     OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                     ORDER BY last_visit_time DESC LIMIT 1000";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var urlObj = reader["url"];
                                    var titleObj = reader["title"];
                                    var visitCountObj = reader["visit_count"];
                                    var lastVisitObj = reader["last_visit_time"];
                                    
                                    results.Add(new HistoryResult
                                    {
                                        Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                        Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                        Browser = "Chrome",
                                        VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                        LastVisit = lastVisitObj != null && lastVisitObj != DBNull.Value ? DateTime.FromFileTime(Convert.ToInt64(lastVisitObj)) : DateTime.MinValue
                                    });
                                }
                            }
                        }
                    }
                    File.Delete(historyPath + ".temp");
                }
                catch { }
            }

            return results;
        }

        private List<HistoryResult> SearchEdgeHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\User Data\Default\History");

            if (File.Exists(historyPath))
            {
                try
                {
                    File.Copy(historyPath, historyPath + ".temp", true);
                    using (var connection = new SQLiteConnection($"Data Source={historyPath}.temp;Version=3;"))
                    {
                        connection.Open();
                        var query = @"SELECT url, title, visit_count, last_visit_time FROM urls 
                                     WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                     OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                     ORDER BY last_visit_time DESC LIMIT 1000";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var urlObj = reader["url"];
                                    var titleObj = reader["title"];
                                    var visitCountObj = reader["visit_count"];
                                    var lastVisitObj = reader["last_visit_time"];
                                    
                                    results.Add(new HistoryResult
                                    {
                                        Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                        Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                        Browser = "Edge",
                                        VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                        LastVisit = lastVisitObj != null && lastVisitObj != DBNull.Value ? DateTime.FromFileTime(Convert.ToInt64(lastVisitObj)) : DateTime.MinValue
                                    });
                                }
                            }
                        }
                    }
                    File.Delete(historyPath + ".temp");
                }
                catch { }
            }

            return results;
        }

        private List<HistoryResult> SearchFirefoxHistory()
        {
            var results = new List<HistoryResult>();
            var profilesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"Mozilla\Firefox\Profiles");

            if (Directory.Exists(profilesPath))
            {
                try
                {
                    var profileDirs = Directory.GetDirectories(profilesPath);
                    foreach (var profileDir in profileDirs)
                    {
                        var placesPath = Path.Combine(profileDir, "places.sqlite");
                        if (File.Exists(placesPath))
                        {
                            File.Copy(placesPath, placesPath + ".temp", true);
                            using (var connection = new SQLiteConnection($"Data Source={placesPath}.temp;Version=3;"))
                            {
                                connection.Open();
                                var query = @"SELECT url, title, visit_count, last_visit_date FROM moz_places 
                                             WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                             OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                             ORDER BY last_visit_date DESC LIMIT 1000";
                                
                                using (var command = new SQLiteCommand(query, connection))
                                {
                                    using (var reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            var lastVisit = reader["last_visit_date"] != DBNull.Value
                                                ? DateTime.FromFileTime(Convert.ToInt64(reader["last_visit_date"]) / 1000)
                                                : DateTime.MinValue;

                                            var urlObj = reader["url"];
                                            var titleObj = reader["title"];
                                            var visitCountObj = reader["visit_count"];
                                            
                                            results.Add(new HistoryResult
                                            {
                                                Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                                Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                                Browser = "Firefox",
                                                VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                                LastVisit = lastVisit
                                            });
                                        }
                                    }
                                }
                            }
                            File.Delete(placesPath + ".temp");
                        }
                    }
                }
                catch { }
            }

            return results;
        }

        private List<HistoryResult> SearchOperaHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Opera Software\Opera Stable\History");

            if (File.Exists(historyPath))
            {
                try
                {
                    File.Copy(historyPath, historyPath + ".temp", true);
                    using (var connection = new SQLiteConnection($"Data Source={historyPath}.temp;Version=3;"))
                    {
                        connection.Open();
                        var query = @"SELECT url, title, visit_count, last_visit_time FROM urls 
                                     WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                     OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                     ORDER BY last_visit_time DESC LIMIT 1000";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var urlObj = reader["url"];
                                    var titleObj = reader["title"];
                                    var visitCountObj = reader["visit_count"];
                                    var lastVisitObj = reader["last_visit_time"];
                                    
                                    results.Add(new HistoryResult
                                    {
                                        Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                        Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                        Browser = "Opera",
                                        VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                        LastVisit = lastVisitObj != null && lastVisitObj != DBNull.Value ? DateTime.FromFileTime(Convert.ToInt64(lastVisitObj)) : DateTime.MinValue
                                    });
                                }
                            }
                        }
                    }
                    File.Delete(historyPath + ".temp");
                }
                catch { }
            }

            return results;
        }

        private List<HistoryResult> SearchBraveHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"BraveSoftware\Brave-Browser\User Data\Default\History");

            if (File.Exists(historyPath))
            {
                try
                {
                    File.Copy(historyPath, historyPath + ".temp", true);
                    using (var connection = new SQLiteConnection($"Data Source={historyPath}.temp;Version=3;"))
                    {
                        connection.Open();
                        var query = @"SELECT url, title, visit_count, last_visit_time FROM urls 
                                     WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                     OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                     ORDER BY last_visit_time DESC LIMIT 1000";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var urlObj = reader["url"];
                                    var titleObj = reader["title"];
                                    var visitCountObj = reader["visit_count"];
                                    var lastVisitObj = reader["last_visit_time"];
                                    
                                    results.Add(new HistoryResult
                                    {
                                        Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                        Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                        Browser = "Brave",
                                        VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                        LastVisit = lastVisitObj != null && lastVisitObj != DBNull.Value ? DateTime.FromFileTime(Convert.ToInt64(lastVisitObj)) : DateTime.MinValue
                                    });
                                }
                            }
                        }
                    }
                    File.Delete(historyPath + ".temp");
                }
                catch { }
            }

            return results;
        }

        private List<HistoryResult> SearchVivaldiHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Vivaldi\User Data\Default\History");

            if (File.Exists(historyPath))
            {
                try
                {
                    File.Copy(historyPath, historyPath + ".temp", true);
                    using (var connection = new SQLiteConnection($"Data Source={historyPath}.temp;Version=3;"))
                    {
                        connection.Open();
                        var query = @"SELECT url, title, visit_count, last_visit_time FROM urls 
                                     WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                     OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                     ORDER BY last_visit_time DESC LIMIT 1000";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var urlObj = reader["url"];
                                    var titleObj = reader["title"];
                                    var visitCountObj = reader["visit_count"];
                                    var lastVisitObj = reader["last_visit_time"];
                                    
                                    results.Add(new HistoryResult
                                    {
                                        Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                        Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                        Browser = "Vivaldi",
                                        VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                        LastVisit = lastVisitObj != null && lastVisitObj != DBNull.Value ? DateTime.FromFileTime(Convert.ToInt64(lastVisitObj)) : DateTime.MinValue
                                    });
                                }
                            }
                        }
                    }
                    File.Delete(historyPath + ".temp");
                }
                catch { }
            }

            return results;
        }

        private List<HistoryResult> SearchChromiumHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Chromium\User Data\Default\History");

            if (File.Exists(historyPath))
            {
                try
                {
                    File.Copy(historyPath, historyPath + ".temp", true);
                    using (var connection = new SQLiteConnection($"Data Source={historyPath}.temp;Version=3;"))
                    {
                        connection.Open();
                        var query = @"SELECT url, title, visit_count, last_visit_time FROM urls 
                                     WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                     OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                     ORDER BY last_visit_time DESC LIMIT 1000";
                        
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var urlObj = reader["url"];
                                    var titleObj = reader["title"];
                                    var visitCountObj = reader["visit_count"];
                                    var lastVisitObj = reader["last_visit_time"];
                                    
                                    results.Add(new HistoryResult
                                    {
                                        Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                        Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                        Browser = "Chromium",
                                        VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                        LastVisit = lastVisitObj != null && lastVisitObj != DBNull.Value ? DateTime.FromFileTime(Convert.ToInt64(lastVisitObj)) : DateTime.MinValue
                                    });
                                }
                            }
                        }
                    }
                    File.Delete(historyPath + ".temp");
                }
                catch { }
            }

            return results;
        }

        private List<HistoryResult> SearchOperaGXHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Opera Software\Opera GX Stable\History");
            if (File.Exists(historyPath)) results.AddRange(SearchSqliteHistory(historyPath, "Opera GX"));
            return results;
        }

        private List<HistoryResult> SearchYandexHistory()
        {
            var results = new List<HistoryResult>();
            var historyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Yandex\YandexBrowser\User Data\Default\History");
            if (File.Exists(historyPath)) results.AddRange(SearchSqliteHistory(historyPath, "Yandex"));
            return results;
        }

        private List<HistoryResult> SearchGenericHistory()
        {
            var results = new List<HistoryResult>();
            return results;
        }

        private List<HistoryResult> SearchSqliteHistory(string dbPath, string browserName)
        {
             var results = new List<HistoryResult>();
             try
             {
                 string tempPath = dbPath + ".temp" + Guid.NewGuid();
                 File.Copy(dbPath, tempPath, true);
                 using (var connection = new SQLiteConnection($"Data Source={tempPath};Version=3;"))
                 {
                     connection.Open();
                     var query = @"SELECT url, title, visit_count, last_visit_time FROM urls 
                                  WHERE url LIKE '%roblox%' OR url LIKE '%exploit%' OR url LIKE '%fflag%' 
                                  OR title LIKE '%roblox%' OR title LIKE '%exploit%' OR title LIKE '%fflag%'
                                  ORDER BY last_visit_time DESC LIMIT 1000";
                     
                     using (var command = new SQLiteCommand(query, connection))
                     {
                         using (var reader = command.ExecuteReader())
                         {
                             while (reader.Read())
                             {
                                 var urlObj = reader["url"];
                                 var titleObj = reader["title"];
                                 var visitCountObj = reader["visit_count"];
                                 var lastVisitObj = reader["last_visit_time"];
                                 
                                 results.Add(new HistoryResult
                                 {
                                     Url = urlObj != null && urlObj != DBNull.Value ? urlObj.ToString() ?? "" : "",
                                     Title = titleObj != null && titleObj != DBNull.Value ? titleObj.ToString() ?? "" : "",
                                     Browser = browserName,
                                     VisitCount = visitCountObj != null && visitCountObj != DBNull.Value ? Convert.ToInt32(visitCountObj) : 0,
                                     LastVisit = lastVisitObj != null && lastVisitObj != DBNull.Value ? DateTime.FromFileTime(Convert.ToInt64(lastVisitObj)) : DateTime.MinValue
                                 });
                             }
                         }
                     }
                 }
                 File.Delete(tempPath);
             }
             catch { }
             return results;
        }
    }

    public class HistoryResult
    {
        public string Url { get; set; } = "";
        public string Title { get; set; } = "";
        public string Browser { get; set; } = "";
        public int VisitCount { get; set; }
        public DateTime LastVisit { get; set; }
    }

    public class SearchQuery
    {
        public string Browser { get; set; } = "";
        public string Q { get; set; } = "";
    }
}

