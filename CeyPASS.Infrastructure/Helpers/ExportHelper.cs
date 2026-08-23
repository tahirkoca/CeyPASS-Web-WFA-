using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using OfficeOpenXml;
using PdfSharp.Fonts;
using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using Colors = MigraDoc.DocumentObjectModel.Colors;
using Orientation = MigraDoc.DocumentObjectModel.Orientation;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class ExportHelper
    {
        public static void ConfigurePdfFonts()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        }

        public static void ExportToExcel(DataTable dt, string filePath)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                LogHelper.Info("Raporlar", "Excel", "Başladı", $"{{\"path\":\"{filePath}\",\"rows\":{dt?.Rows?.Count ?? 0}}}", cid);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Rapor");
                    worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        int lastRow = dt.Rows.Count + 1;
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            var t = Nullable.GetUnderlyingType(dt.Columns[c].DataType) ?? dt.Columns[c].DataType;
                            string fmt = null;

                            if (t == typeof(DateTime) || t == typeof(DateTimeOffset))
                            {
                                bool hasTime = false;
                                foreach (DataRow r in dt.Rows)
                                {
                                    if (r[c] == DBNull.Value || r[c] == null)
                                        continue;
                                    var dto = r[c] is DateTimeOffset off ? off.DateTime : Convert.ToDateTime(r[c]);
                                    if (dto.TimeOfDay != TimeSpan.Zero)
                                    {
                                        hasTime = true;
                                        break;
                                    }
                                }
                                fmt = hasTime ? "dd.MM.yyyy HH:mm:ss" : "dd.MM.yyyy";
                            }
                            else if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort)
                                  || t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong))
                            {
                                fmt = "0";
                            }
                            else if (t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                            {
                                fmt = "#,##0.##";
                            }

                            if (fmt != null)
                                worksheet.Cells[2, c + 1, lastRow, c + 1].Style.Numberformat.Format = fmt;
                        }
                    }

                    package.SaveAs(new FileInfo(filePath));
                }
                LogHelper.Info("Raporlar", "Excel", "Tamamlandı", null, cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Raporlar", "Excel", "Hata", ex, null, cid);
                throw;
            }
        }
        public static void ExportToPdf(DataTable dt, string filePath, string reportTitle = "Rapor")
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                LogHelper.Info("Raporlar", "PDF", "Başladı", $"{{\"path\":\"{filePath}\",\"rows\":{dt?.Rows?.Count ?? 0}}}", cid);
                var doc = new Document();

                doc.Info.Title = reportTitle ?? "Rapor";
                var normal = doc.Styles["Normal"];
                normal.Font.Name = "Arial";
                normal.Font.Size = 10;

                var sec = doc.AddSection();
                sec.PageSetup.Orientation = Orientation.Landscape;
                sec.PageSetup.LeftMargin = "1.4cm";
                sec.PageSetup.RightMargin = "1.4cm";
                sec.PageSetup.TopMargin = "1.4cm";
                sec.PageSetup.BottomMargin = "1.4cm";

                var p = sec.AddParagraph(doc.Info.Title);
                p.Format.Font.Size = 14;
                p.Format.Font.Bold = true;
                p.Format.Alignment = ParagraphAlignment.Center;
                p.Format.SpaceAfter = "0.5cm";

                var table = sec.AddTable();
                table.Borders.Width = 0.75;
                table.Borders.Color = Colors.Gray;

                double GetWidth(string name)
                {
                    name = name.ToLower();
                    if (name.Contains("ad") || name.Contains("soyad")) return 6.0;
                    if (name.Contains("tarih")) return 4.0;
                    if (name.Contains("saat")) return 3.0;
                    return 4.0;
                }

                foreach (DataColumn col in dt.Columns)
                {
                    var c = table.AddColumn(Unit.FromCentimeter(GetWidth(col.ColumnName)));
                    c.Format.Alignment = ParagraphAlignment.Left;
                }

                var header = table.AddRow();
                header.HeadingFormat = true;
                header.Format.Font.Bold = true;
                header.Format.Alignment = ParagraphAlignment.Center;

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var cell = header.Cells[i];
                    cell.AddParagraph(dt.Columns[i].ColumnName);
                    cell.Format.Alignment = ParagraphAlignment.Left;
                }

                foreach (DataRow r in dt.Rows)
                {
                    var row = table.AddRow();
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string text = r[i]?.ToString() ?? "";

                        var colName = dt.Columns[i].ColumnName.ToLower();
                        if (colName.Contains("tarih") && DateTime.TryParse(text, out var d))
                            text = d.ToString("dd.MM.yyyy");

                        row.Cells[i].AddParagraph(text);
                        row.Cells[i].Format.Alignment = ParagraphAlignment.Left;
                    }
                }

                var renderer = new PdfDocumentRenderer();
                renderer.Document = doc;
                renderer.RenderDocument();
                renderer.PdfDocument.Save(filePath);

                LogHelper.Info("Raporlar", "PDF", "Tamamlandı", null, cid);
            }
            catch (Exception ex)
            {
                LogHelper.Error("Raporlar", "PDF", "Hata", ex, null, cid);
                throw;
            }
        }
    }
}
