using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using wpf_projekt.Models;
using wpf_projekt.models;

namespace wpf_projekt.Services
{
    public class CsvImportResult
    {
        public List<Transaction> Imported { get; init; } = new();
        public List<string> Errors { get; init; } = new();
        public int Duplicates { get; set; }
    }

    public class CsvImportService
    {
        //  Odczyt pliku CSV  słowniki wierszy 

        public static (string[] Headers, List<Dictionary<string, string>> Rows) ParseFile(string path)
        {
            string[] lines = File.ReadAllLines(path);

            if (lines.Length < 2)
                throw new InvalidOperationException("Plik jest pusty lub nie zawiera wierszy z danymi.");

            char delimiter = lines[0].Contains(';') ? ';' : ',';
            string[] headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();

            if (headers.Length < 2 || string.IsNullOrWhiteSpace(headers[0]))
                throw new InvalidOperationException("Plik nie zawiera poprawnych nagłówków kolumn.");

            var rows = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                // Prosta obsługa cudzysłowów
                var values = SplitCsvLine(lines[i], delimiter);

                if (values.Length < headers.Length - 1) continue;

                var row = new Dictionary<string, string>();
                for (int j = 0; j < headers.Length; j++)
                {
                    if (string.IsNullOrWhiteSpace(headers[j])) continue;
                    row[headers[j]] = j < values.Length ? values[j].Trim() : string.Empty;
                }
                if (row.Count > 0) rows.Add(row);
            }

            if (rows.Count == 0)
                throw new InvalidOperationException("Nie znaleziono żadnych danych do zaimportowania.");

            return (headers, rows);
        }

        //  Konwersja wierszy  modele Transaction 

        public static CsvImportResult MapToTransactions(
            List<Dictionary<string, string>> rows,
            CsvMappingProfile profile,
            TransactionType defaultCategory,
            int? personalAccountId,
            int? sharedAccountId,
            IEnumerable<Transaction> existingTransactions,
            IEnumerable<TransactionType>? allCategories = null)
        {
            var result = new CsvImportResult();
            var existing = existingTransactions.ToList();
            var categories = allCategories?.ToList() ?? new List<TransactionType>();
            int rowNumber = 1;

            foreach (var row in rows)
            {
                rowNumber++;

                //  Data
                if (!row.TryGetValue(profile.ColumnDate!, out var rawDate) ||
                    !TryParseDate(rawDate, profile.DateFormat, out DateTime date))
                {
                    result.Errors.Add($"Wiersz {rowNumber}: Nie można sparsować daty '{rawDate}'.");
                    continue;
                }

                //  Kwota
                if (!row.TryGetValue(profile.ColumnAmount!, out var rawAmount) ||
                    !TryParseAmount(rawAmount, out decimal amount))
                {
                    result.Errors.Add($"Wiersz {rowNumber}: Nie można sparsować kwoty '{rawAmount}'.");
                    continue;
                }

                //  Kierunek transakcji
                bool isPositive;
                if (!profile.AmountSignDeterminesDirection
                    && !string.IsNullOrWhiteSpace(profile.ColumnIsPositive)
                    && row.TryGetValue(profile.ColumnIsPositive!, out var rawType)
                    && !string.IsNullOrWhiteSpace(rawType))
                {
                    isPositive = IsPositiveType(rawType);
                }
                else
                {
                    isPositive = amount >= 0;
                }
                amount = Math.Abs(amount);

                if (amount == 0)
                {
                    result.Errors.Add($"Wiersz {rowNumber}: Kwota wynosi 0 – pominięto.");
                    continue;
                }

                //  Opis
                string description = string.Empty;
                if (!string.IsNullOrWhiteSpace(profile.ColumnDescription) &&
                    row.TryGetValue(profile.ColumnDescription, out var rawDesc))
                    description = rawDesc.Trim();

                //  Kategoria
                TransactionType resolvedCategory = defaultCategory;
                if (!string.IsNullOrWhiteSpace(profile.ColumnCategory)
                    && row.TryGetValue(profile.ColumnCategory!, out var rawCategory)
                    && !string.IsNullOrWhiteSpace(rawCategory)
                    && categories.Count > 0)
                {
                    var match = categories.FirstOrDefault(c =>
                        c.Name.Equals(rawCategory.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (match != null) resolvedCategory = match;
                }

                //  Deduplikacja
                bool isDuplicate = existing.Any(t =>
                    t.Date.Date == date.Date &&
                    t.Amount == amount &&
                    t.IsPositive == isPositive &&
                    t.Description == description);

                if (isDuplicate)
                {
                    result.Duplicates++;
                    continue;
                }

                //  Budowanie modelu
                var tx = new Transaction
                {
                    Amount = amount,
                    IsPositive = isPositive,
                    Date = date,
                    Description = description,
                    TransactionTypeId = resolvedCategory.Id,
                    PersonalAccountId = personalAccountId,
                    SharedAccountId = sharedAccountId
                };

                result.Imported.Add(tx);
            }

            return result;
        }

        private static readonly string[] PositiveTypeKeywords =
            { "przychód", "przychod", "uznanie", "wpływ", "wplyw", "przychodzący", "przychodzacy", "credit", "income" };

        private static readonly string[] NegativeTypeKeywords =
            { "wydatek", "obciążenie", "obciazenie", "wypłata", "wyplata", "wychodzący", "wychodzacy", "debit", "expense", "zakup" };

        private static bool IsPositiveType(string raw)
        {
            var normalized = raw.Trim();
            foreach (var kw in PositiveTypeKeywords)
                if (normalized.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return true;
            foreach (var kw in NegativeTypeKeywords)
                if (normalized.Contains(kw, StringComparison.OrdinalIgnoreCase))
                    return false;
            return normalized.Contains('+');
        }

        //  Zapis i odczyt profilu mapowania 

        private static readonly string ProfilesDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "FinanceManager", "CsvProfiles");

        public static void SaveProfile(CsvMappingProfile profile)
        {
            Directory.CreateDirectory(ProfilesDir);
            string file = Path.Combine(ProfilesDir, $"{Sanitize(profile.ProfileName)}.json");
            File.WriteAllText(file, JsonSerializer.Serialize(profile,
                new JsonSerializerOptions { WriteIndented = true }));
        }

        public static List<CsvMappingProfile> LoadSavedProfiles()
        {
            if (!Directory.Exists(ProfilesDir)) return new();
            var list = new List<CsvMappingProfile>();
            foreach (var f in Directory.GetFiles(ProfilesDir, "*.json"))
            {
                try
                {
                    var p = JsonSerializer.Deserialize<CsvMappingProfile>(File.ReadAllText(f));
                    if (p != null) list.Add(p);
                }
                catch { /* uszkodzony plik – pomijamy */ }
            }
            return list;
        }

        //  Helpery 

        private static bool TryParseDate(string? raw, string format, out DateTime result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            // Próbuj podany format, a potem kilka popularnych
            var formats = new[] { format, "dd.MM.yyyy", "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "d.M.yyyy" };
            return DateTime.TryParseExact(raw.Trim(), formats,
                       CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
                   DateTime.TryParse(raw.Trim(), CultureInfo.CurrentCulture,
                       DateTimeStyles.None, out result);
        }

        private static bool TryParseAmount(string? raw, out decimal result)
        {
            result = 0;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            // Normalizuj: usuń spacje, zamień przecinek dziesiętny na kropkę
            string normalized = raw.Trim()
                                   .Replace("\u00a0", "")   // nbsp, ai to dodał
                                   .Replace(" ", "");

            // Jeśli jest i przecinek i kropka, ten ostatni jest separatorem dziesiętnym
            bool hasDot = normalized.Contains('.');
            bool hasComma = normalized.Contains(',');

            if (hasDot && hasComma)
            {
                // Np. "1.234,56" (PL) lub "1,234.56" (EN)
                if (normalized.IndexOf('.') < normalized.IndexOf(','))
                    normalized = normalized.Replace(".", "").Replace(",", ".");
                else
                    normalized = normalized.Replace(",", "");
            }
            else if (hasComma)
            {
                normalized = normalized.Replace(",", ".");
            }

            return decimal.TryParse(normalized, NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out result);
        }

        private static string[] SplitCsvLine(string line, char delimiter)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();

            foreach (char c in line)
            {
                if (c == '"') { inQuotes = !inQuotes; }
                else if (c == delimiter && !inQuotes) { fields.Add(current.ToString()); current.Clear(); }
                else { current.Append(c); }
            }
            fields.Add(current.ToString());
            return fields.ToArray();
        }

        private static string Sanitize(string name) =>
            string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    }
}
