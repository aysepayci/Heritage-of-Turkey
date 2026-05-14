using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Heritage_of_Turkey.Data;
using Heritage_of_Turkey.Models;
using Microsoft.EntityFrameworkCore;

namespace Heritage_of_Turkey.Controllers.Admin
{
    public class CsvImportResult
    {
        public int RowNumber { get; set; }
        public string Status { get; set; } // "Success", "Warning", "Error"
        public string ItemName { get; set; }
        public string City { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }

    public class CsvImportService
    {
        private readonly ApplicationDbContext _context;

        public CsvImportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Museum CSV Import
        public async Task<List<CsvImportResult>> ValidateMuseumCsvAsync(Stream csvStream)
        {
            var results = new List<CsvImportResult>();
            var lines = new List<string>();
            var processedKeys = new HashSet<string>(); // CSV içi duplicate check için

            try
            {
                using (var reader = new StreamReader(csvStream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lines.Add(line);
                    }
                }

                if (lines.Count < 2)
                {
                    results.Add(new CsvImportResult
                    {
                        Status = "Error",
                        Message = "CSV dosyası başlık satırı ve en az 1 veri satırı içermelidir"
                    });
                    return results;
                }

                // Header validasyonu
                var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
                var requiredHeaders = new[] { "MuseumName", "City", "Address", "Description", "CategoryName" };
                var missingHeaders = requiredHeaders.Where(h => !headers.Contains(h)).ToList();

                if (missingHeaders.Any())
                {
                    results.Add(new CsvImportResult
                    {
                        Status = "Error",
                        Message = $"CSV'de gerekli kolonlar eksik: {string.Join(", ", missingHeaders)}"
                    });
                    return results;
                }

                // Veri satırlarını işle
                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = ParseCsvLine(lines[i]);
                    var data = MapCsvLineToDict(headers, parts);

                    if (string.IsNullOrWhiteSpace(data.GetValueOrDefault("MuseumName")))
                        continue; // Boş satırı atla

                    var result = await ValidateMuseumRowAsync(data, i + 1, processedKeys);
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                results.Add(new CsvImportResult
                {
                    Status = "Error",
                    Message = $"CSV dosyası okuma hatası: {ex.Message}"
                });
            }

            return results;
        }

        // Ruin CSV Import
        public async Task<List<CsvImportResult>> ValidateRuinCsvAsync(Stream csvStream)
        {
            var results = new List<CsvImportResult>();
            var lines = new List<string>();
            var processedKeys = new HashSet<string>();

            try
            {
                using (var reader = new StreamReader(csvStream))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lines.Add(line);
                    }
                }

                if (lines.Count < 2)
                {
                    results.Add(new CsvImportResult
                    {
                        Status = "Error",
                        Message = "CSV dosyası başlık satırı ve en az 1 veri satırı içermelidir"
                    });
                    return results;
                }

                var headers = lines[0].Split(',').Select(h => h.Trim()).ToList();
                var requiredHeaders = new[] { "RuinName", "City", "Address", "Description", "CategoryName" };
                var missingHeaders = requiredHeaders.Where(h => !headers.Contains(h)).ToList();

                if (missingHeaders.Any())
                {
                    results.Add(new CsvImportResult
                    {
                        Status = "Error",
                        Message = $"CSV'de gerekli kolonlar eksik: {string.Join(", ", missingHeaders)}"
                    });
                    return results;
                }

                for (int i = 1; i < lines.Count; i++)
                {
                    var parts = ParseCsvLine(lines[i]);
                    var data = MapCsvLineToDict(headers, parts);

                    if (string.IsNullOrWhiteSpace(data.GetValueOrDefault("RuinName")))
                        continue;

                    var result = await ValidateRuinRowAsync(data, i + 1, processedKeys);
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                results.Add(new CsvImportResult
                {
                    Status = "Error",
                    Message = $"CSV dosyası okuma hatası: {ex.Message}"
                });
            }

            return results;
        }

        private async Task<CsvImportResult> ValidateMuseumRowAsync(Dictionary<string, string> data, int rowNumber, HashSet<string> processedKeys)
        {
            var result = new CsvImportResult
            {
                RowNumber = rowNumber,
                Data = data,
                ItemName = data.GetValueOrDefault("MuseumName", "")
            };

            // 1. CSV içi duplicate check
            var duplicateKey = $"{data.GetValueOrDefault("MuseumName")}|{data.GetValueOrDefault("City")}";
            if (processedKeys.Contains(duplicateKey))
            {
                result.Status = "Warning";
                result.Message = "⚠️ Bu satır CSV'nin içinde tekrar geliyor, sadece ilki kullanılacak";
                return result;
            }
            processedKeys.Add(duplicateKey);

            // 2. Temel validasyonlar
            var museumName = data.GetValueOrDefault("MuseumName", "").Trim();
            var city = data.GetValueOrDefault("City", "").Trim();
            var address = data.GetValueOrDefault("Address", "").Trim();
            var description = data.GetValueOrDefault("Description", "").Trim();
            var categoryName = data.GetValueOrDefault("CategoryName", "").Trim();

            result.City = city;

            if (string.IsNullOrWhiteSpace(museumName) || museumName.Length > 150)
            {
                result.Status = "Error";
                result.Message = "❌ Museum adı boş veya 150 karakteri aşıyor";
                return result;
            }

            if (string.IsNullOrWhiteSpace(city) || city.Length > 50)
            {
                result.Status = "Error";
                result.Message = "❌ Şehir boş veya 50 karakteri aşıyor";
                return result;
            }

            if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
            {
                result.Status = "Error";
                result.Message = "❌ Adres boş veya 300 karakteri aşıyor";
                return result;
            }

            if (string.IsNullOrWhiteSpace(description) || description.Length > 2000)
            {
                result.Status = "Error";
                result.Message = "❌ Açıklama boş veya 2000 karakteri aşıyor";
                return result;
            }

            // 3. Category Matching
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == categoryName && c.IsActive);

            if (category == null)
            {
                result.Status = "Warning";
                result.Message = $"⚠️ Kategori '{categoryName}' bulunamadı. Mevcut kategoriler: Archaeology Museum, Art Museum, Ethnography Museum, Ancient City, Ancient Theater, Temple Ruins";
                return result;
            }

            // 4. URL validasyonları
            var imageUrl = data.GetValueOrDefault("ImageUrl", "").Trim();
            if (!string.IsNullOrWhiteSpace(imageUrl) && !IsValidUrl(imageUrl))
            {
                result.Status = "Warning";
                result.Message = "⚠️ ImageUrl geçersiz format, null olarak ayarlanacak";
                imageUrl = null;
            }

            var googleMapsUrl = data.GetValueOrDefault("GoogleMapsUrl", "").Trim();
            if (!string.IsNullOrWhiteSpace(googleMapsUrl) && !IsValidUrl(googleMapsUrl))
            {
                result.Status = "Warning";
                result.Message = "⚠️ GoogleMapsUrl geçersiz format, null olarak ayarlanacak";
                googleMapsUrl = null;
            }

            // 5. Database Duplicate Check (MuseumName + City)
            var existingMuseum = await _context.Museums
                .FirstOrDefaultAsync(m => m.MuseumName == museumName && m.City == city);

            if (existingMuseum != null)
            {
                result.Status = "Warning";
                result.Message = $"⚠️ Veritabanında zaten var: '{museumName} - {city}'";
                return result;
            }

            result.Status = "Success";
            result.Message = "✅ Hazır";
            return result;
        }

        private async Task<CsvImportResult> ValidateRuinRowAsync(Dictionary<string, string> data, int rowNumber, HashSet<string> processedKeys)
        {
            var result = new CsvImportResult
            {
                RowNumber = rowNumber,
                Data = data,
                ItemName = data.GetValueOrDefault("RuinName", "")
            };

            // 1. CSV içi duplicate check
            var historicalPeriod = data.GetValueOrDefault("HistoricalPeriod", "").Trim();
            var duplicateKey = $"{data.GetValueOrDefault("RuinName")}|{data.GetValueOrDefault("City")}|{historicalPeriod}";
            if (processedKeys.Contains(duplicateKey))
            {
                result.Status = "Warning";
                result.Message = "⚠️ Bu satır CSV'nin içinde tekrar geliyor, sadece ilki kullanılacak";
                return result;
            }
            processedKeys.Add(duplicateKey);

            // 2. Temel validasyonlar
            var ruinName = data.GetValueOrDefault("RuinName", "").Trim();
            var city = data.GetValueOrDefault("City", "").Trim();
            var address = data.GetValueOrDefault("Address", "").Trim();
            var description = data.GetValueOrDefault("Description", "").Trim();
            var categoryName = data.GetValueOrDefault("CategoryName", "").Trim();

            result.City = city;

            if (string.IsNullOrWhiteSpace(ruinName) || ruinName.Length > 150)
            {
                result.Status = "Error";
                result.Message = "❌ Ruin adı boş veya 150 karakteri aşıyor";
                return result;
            }

            if (string.IsNullOrWhiteSpace(city) || city.Length > 50)
            {
                result.Status = "Error";
                result.Message = "❌ Şehir boş veya 50 karakteri aşıyor";
                return result;
            }

            if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
            {
                result.Status = "Error";
                result.Message = "❌ Adres boş veya 300 karakteri aşıyor";
                return result;
            }

            if (string.IsNullOrWhiteSpace(description) || description.Length > 2000)
            {
                result.Status = "Error";
                result.Message = "❌ Açıklama boş veya 2000 karakteri aşıyor";
                return result;
            }

            // 3. Category Matching
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryName == categoryName && c.IsActive);

            if (category == null)
            {
                result.Status = "Warning";
                result.Message = $"⚠️ Kategori '{categoryName}' bulunamadı. Mevcut kategoriler: Archaeology Museum, Art Museum, Ethnography Museum, Ancient City, Ancient Theater, Temple Ruins";
                return result;
            }

            // 4. URL validasyonları
            var imageUrl = data.GetValueOrDefault("ImageUrl", "").Trim();
            if (!string.IsNullOrWhiteSpace(imageUrl) && !IsValidUrl(imageUrl))
            {
                result.Status = "Warning";
                result.Message = "⚠️ ImageUrl geçersiz format, null olarak ayarlanacak";
                imageUrl = null;
            }

            var googleMapsUrl = data.GetValueOrDefault("GoogleMapsUrl", "").Trim();
            if (!string.IsNullOrWhiteSpace(googleMapsUrl) && !IsValidUrl(googleMapsUrl))
            {
                result.Status = "Warning";
                result.Message = "⚠️ GoogleMapsUrl geçersiz format, null olarak ayarlanacak";
                googleMapsUrl = null;
            }

            // 5. Database Duplicate Check (RuinName + City)
            var existingRuin = await _context.Ruins
                .FirstOrDefaultAsync(r => r.RuinName == ruinName && r.City == city);

            if (existingRuin != null)
            {
                result.Status = "Warning";
                result.Message = $"⚠️ Veritabanında zaten var: '{ruinName} - {city}'";
                return result;
            }

            result.Status = "Success";
            result.Message = "✅ Hazır";
            return result;
        }

        // Toplu Museum Import
        public async Task<(int successCount, int skipCount, List<CsvImportResult> results)> ImportMuseumsAsync(List<CsvImportResult> validResults)
        {
            int successCount = 0;
            int skipCount = 0;
            var results = new List<CsvImportResult>();

            foreach (var validResult in validResults.Where(r => r.Status == "Success"))
            {
                try
                {
                    var data = validResult.Data;
                    var categoryName = data.GetValueOrDefault("CategoryName", "").Trim();
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryName == categoryName && c.IsActive);

                    if (category == null)
                    {
                        skipCount++;
                        continue;
                    }

                    var museum = new Museum
                    {
                        MuseumName = data.GetValueOrDefault("MuseumName", "").Trim(),
                        City = data.GetValueOrDefault("City", "").Trim(),
                        District = data.GetValueOrDefault("District", "").Trim(),
                        Address = data.GetValueOrDefault("Address", "").Trim(),
                        Description = data.GetValueOrDefault("Description", "").Trim(),
                        ImageUrl = data.GetValueOrDefault("ImageUrl", "").Trim(),
                        TicketPrice = TryParseDecimal(data.GetValueOrDefault("TicketPrice", "")),
                        OpeningHours = data.GetValueOrDefault("OpeningHours", "").Trim(),
                        PhoneNumber = data.GetValueOrDefault("PhoneNumber", "").Trim(),
                        Email = data.GetValueOrDefault("Email", "").Trim(),
                        Website = data.GetValueOrDefault("Website", "").Trim(),
                        GoogleMapsUrl = data.GetValueOrDefault("GoogleMapsUrl", "").Trim(),
                        IsFeatured = TryParseBool(data.GetValueOrDefault("IsFeatured", "false")),
                        IsActive = TryParseBool(data.GetValueOrDefault("IsActive", "true")),
                        CategoryId = category.CategoryId,
                        CreatedDate = DateTime.Now
                    };

                    _context.Museums.Add(museum);
                    successCount++;
                }
                catch (Exception ex)
                {
                    validResult.Status = "Error";
                    validResult.Message = $"Veritabanı hatası: {ex.Message}";
                    results.Add(validResult);
                }
            }

            if (successCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return (successCount, skipCount, results);
        }

        // Toplu Ruin Import
        public async Task<(int successCount, int skipCount, List<CsvImportResult> results)> ImportRuinsAsync(List<CsvImportResult> validResults)
        {
            int successCount = 0;
            int skipCount = 0;
            var results = new List<CsvImportResult>();

            foreach (var validResult in validResults.Where(r => r.Status == "Success"))
            {
                try
                {
                    var data = validResult.Data;
                    var categoryName = data.GetValueOrDefault("CategoryName", "").Trim();
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryName == categoryName && c.IsActive);

                    if (category == null)
                    {
                        skipCount++;
                        continue;
                    }

                    var ruin = new Ruin
                    {
                        RuinName = data.GetValueOrDefault("RuinName", "").Trim(),
                        City = data.GetValueOrDefault("City", "").Trim(),
                        District = data.GetValueOrDefault("District", "").Trim(),
                        Address = data.GetValueOrDefault("Address", "").Trim(),
                        Description = data.GetValueOrDefault("Description", "").Trim(),
                        ImageUrl = data.GetValueOrDefault("ImageUrl", "").Trim(),
                        TicketPrice = TryParseDecimal(data.GetValueOrDefault("TicketPrice", "")),
                        OpeningHours = data.GetValueOrDefault("OpeningHours", "").Trim(),
                        HistoricalPeriod = data.GetValueOrDefault("HistoricalPeriod", "").Trim(),
                        GoogleMapsUrl = data.GetValueOrDefault("GoogleMapsUrl", "").Trim(),
                        IsFeatured = TryParseBool(data.GetValueOrDefault("IsFeatured", "false")),
                        IsActive = TryParseBool(data.GetValueOrDefault("IsActive", "true")),
                        CategoryId = category.CategoryId,
                        CreatedDate = DateTime.Now
                    };

                    _context.Ruins.Add(ruin);
                    successCount++;
                }
                catch (Exception ex)
                {
                    validResult.Status = "Error";
                    validResult.Message = $"Veritabanı hatası: {ex.Message}";
                    results.Add(validResult);
                }
            }

            if (successCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return (successCount, skipCount, results);
        }

        // Helper Methods
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var current = new System.Text.StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim().Trim('"'));
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim().Trim('"'));
            return result;
        }

        private Dictionary<string, string> MapCsvLineToDict(List<string> headers, List<string> values)
        {
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < headers.Count && i < values.Count; i++)
            {
                var cleanHeader = headers[i].Trim().Trim('"');
                var cleanValue = values[i].Trim().Trim('"');
                dict[cleanHeader] = cleanValue;
            }
            return dict;
        }

        private bool IsValidUrl(string url)
        {
            try
            {
                new Uri(url);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private decimal? TryParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        private bool TryParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}