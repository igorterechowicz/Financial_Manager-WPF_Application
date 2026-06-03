using System.Text.Json.Serialization;

namespace wpf_projekt.Import
{
    /// <summary>
    /// Schemat mapowania kolumn CSV → pola modelu Transaction.
    /// Można serializować do JSON i zapisać na dysku.
    /// </summary>
    public class CsvMappingProfile
    {
        public string ProfileName { get; set; } = "Mój bank";

        // Separator używany w pliku CSV
        public char Delimiter { get; set; } = ';';

        // Nazwy kolumn w nagłówku pliku CSV przypisane do pól modelu
        public string? ColumnDate        { get; set; }
        public string? ColumnAmount      { get; set; }
        public string? ColumnDescription { get; set; }
        public string? ColumnIsPositive  { get; set; }   // opcjonalna – wykrywana automatycznie lub ze znaku kwoty

        // Jeśli bank nie ma kolumny typ (Przychód/Wydatek), kierunek wynika ze znaku kwoty
        public bool AmountSignDeterminesDirection { get; set; } = true;

        // Format daty w pliku, np. "dd.MM.yyyy" lub "yyyy-MM-dd"
        public string DateFormat { get; set; } = "dd.MM.yyyy";

        // Opcjonalna kolumna z nazwą kategorii w pliku CSV
        public string? ColumnCategory { get; set; }

        // Domyślna kategoria dla importowanych transakcji (gdy brak kolumny lub brak dopasowania)
        public string DefaultCategoryName { get; set; } = "Import";

        [JsonIgnore]
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(ColumnDate) &&
            !string.IsNullOrWhiteSpace(ColumnAmount);
    }
}
