using CeyPASS.Entities.Concrete;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class ExcelHelper
    {
        public static void ExceleDonustur(List<PuantajExportDTO> data, string filePath)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Rapor");
                    worksheet.Cells["A1"].Value = data.Count + 1;

                    string[] headers = new[]
                    {
                "Sicil Numarası", "Ad", "Soyad", "Firma", "Bölüm", "İşyeri",
                "Normal(Saat)", "Hafta Tatili(Gün)", "Resmi Tatil(Gün)", "Ücretli İzin(Gün)",
                "Ücretsiz İzin(Saat)", "RAPOR(Gün)", "Fazla Mesai (1)(Saat)", "Fazla Çalışma(Saat)",
                "Fazla Mesai %125(Saat)", "RESMİ TATİL(Saat)", "SSK Eksik Çalışma Nedeni",
                "İşkur Eksik Çalışma Nedeni"
            };

                    for (int i = 0; i < headers.Length; i++)
                        worksheet.Cells[1, i + 2].Value = headers[i];

                    var templateRow = new object[]
                    {
                "900001", "900002", "900003", "910001", "910002", "910003",
                "010013", "010022", "010032", "010042", "010063", "010072",
                "020013", "020023", "020083", "020073", "900008", "900009"
                    };

                    for (int i = 0; i < templateRow.Length; i++)
                        worksheet.Cells[2, i + 2].Value = templateRow[i];

                    int rowIndex = 3;
                    foreach (var dto in data)
                    {
                        worksheet.Cells[rowIndex, 2].Value = dto.SicilNo;
                        worksheet.Cells[rowIndex, 3].Value = dto.Ad;
                        worksheet.Cells[rowIndex, 4].Value = dto.Soyad;
                        worksheet.Cells[rowIndex, 5].Value = dto.Firma;
                        worksheet.Cells[rowIndex, 6].Value = dto.Bolum;
                        worksheet.Cells[rowIndex, 7].Value = dto.Isyeri;
                        worksheet.Cells[rowIndex, 8].Value = dto.NormalSaat;
                        worksheet.Cells[rowIndex, 9].Value = dto.HaftaTatiliGun;
                        worksheet.Cells[rowIndex, 10].Value = dto.ResmiTatilGun;
                        worksheet.Cells[rowIndex, 11].Value = dto.UcretliIzinGun;
                        worksheet.Cells[rowIndex, 12].Value = dto.UcretsizIzinSaat;
                        worksheet.Cells[rowIndex, 13].Value = dto.RaporGun;
                        worksheet.Cells[rowIndex, 14].Value = dto.FazlaMesaiSaat;
                        worksheet.Cells[rowIndex, 15].Value = dto.FazlaCalismaSaat;
                        worksheet.Cells[rowIndex, 16].Value = dto.FazlaMesai125Saat;
                        worksheet.Cells[rowIndex, 17].Value = dto.ResmiTatilSaat;
                        worksheet.Cells[rowIndex, 18].Value = dto.SskEksikNedeni;
                        worksheet.Cells[rowIndex, 19].Value = dto.IskurEksikNedeni;
                        rowIndex++;
                    }

                    for (int col = 8; col <= 19; col++)
                    {
                        var headerCell = worksheet.Cells[1, col];
                        string headerText = headerCell.Text;
                        bool gunMu = headerText.Contains("(Gün)");

                        for (int row = 3; row < rowIndex; row++)
                        {
                            var cell = worksheet.Cells[row, col];

                            if (cell.Value != null && double.TryParse(cell.Value.ToString(), out double numericValue))
                            {
                                if (gunMu)
                                {
                                    cell.Value = (int)numericValue;
                                    cell.Style.Numberformat.Format = "0";
                                }
                                else
                                {
                                    cell.Value = numericValue;
                                    cell.Style.Numberformat.Format = "#,##0.00";
                                }
                            }
                        }
                    }
                    package.SaveAs(new FileInfo(filePath));
                }
                LogHelper.Info("AylikPuantaj", "ExportWrite", "Excel dosyası yazıldı", detayJson: $"{{\"Dosya\":\"{filePath.Replace("\\", "\\\\")}\",\"Satir\":{data.Count}}}");
            }
            catch (Exception ex)
            {
                LogHelper.Error("AylikPuantaj", "ExportWrite", "Excel yazma hatası", ex, detayJson: $"{{\"Dosya\":\"{filePath.Replace("\\", "\\\\")}\"}}");
                throw;
            }
        }
    }
}
