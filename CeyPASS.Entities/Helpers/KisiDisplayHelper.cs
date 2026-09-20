using System;

namespace CeyPASS.Entities.Helpers
{
    /// <summary>Personel detay ekranında boş alanlar için ortak gösterim metinleri.</summary>
    public static class KisiDisplayHelper
    {
        public const string Missing = "Belirtilmemiş";
        public const string ComboPlaceholder = "-- Seçiniz --";

        /// <returns>Boşsa <see cref="Missing"/>.</returns>
        public static string TextOrMissing(string? value) =>
            string.IsNullOrWhiteSpace(value) ? Missing : value.Trim();

        /// <returns>Boşsa <see cref="Missing"/>.</returns>
        public static string DateOrMissing(DateTime? value) =>
            value?.ToString("dd.MM.yyyy") ?? Missing;

        /// <returns>Boşsa <see cref="Missing"/>.</returns>
        public static string NumberOrMissing(int? value) =>
            value.HasValue ? value.Value.ToString() : Missing;
    }
}
