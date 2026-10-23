using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RenewOne.Application.DTOs.Request.Reports;
using RenewOne.Application.DTOs.Response.Reports;
using RenewOne.Application.Interfaces.Services.Reports;
using RenewOne.Domain.Enums;

namespace RenewOne.Infrastructure.Services.Reports
{
    public sealed class ReportExportService : IReportExportService
    {
        static ReportExportService()
        {
            // QuestPDF community license initialization
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public ReportExportResult Export(EnquiryReportResult reportResult, ReportExportFormat format, string? customTitle = null)
        {
            ArgumentNullException.ThrowIfNull(reportResult);

            var title = string.IsNullOrWhiteSpace(customTitle)
                ? GetDefaultReportTitle(reportResult.ReportType)
                : customTitle;

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            var sanitizedTitle = title.Replace(" ", "_").Replace("/", "_");

            return format switch
            {
                ReportExportFormat.Csv => new ReportExportResult
                {
                    FileBytes = GenerateCsv(reportResult, title),
                    ContentType = "text/csv; charset=utf-8",
                    FileName = $"{sanitizedTitle}_{timestamp}.csv"
                },
                ReportExportFormat.Excel => new ReportExportResult
                {
                    FileBytes = GenerateExcel(reportResult, title),
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName = $"{sanitizedTitle}_{timestamp}.xlsx"
                },
                ReportExportFormat.Pdf => new ReportExportResult
                {
                    FileBytes = GeneratePdf(reportResult, title),
                    ContentType = "application/pdf",
                    FileName = $"{sanitizedTitle}_{timestamp}.pdf"
                },
                _ => throw new NotSupportedException($"Export format '{format}' is not supported.")
            };
        }

        #region CSV Export

        private static byte[] GenerateCsv(EnquiryReportResult reportResult, string title)
        {
            var sb = new StringBuilder();

            // Header info
            sb.AppendLine($"\"{EscapeCsv(title)}\"");
            sb.AppendLine($"\"Generated On:\",\"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\"");
            sb.AppendLine($"\"Total Records:\",\"{reportResult.Data.TotalRecords}\"");
            sb.AppendLine();

            // Metrics summary
            if (reportResult.Metrics is { Count: > 0 })
            {
                sb.AppendLine("\"Summary Metrics:\"");
                foreach (var metric in reportResult.Metrics)
                {
                    sb.AppendLine($"\"{EscapeCsv(metric.Label)}\",\"{EscapeCsv(FormatMetricValue(metric))}\"");
                }
                sb.AppendLine();
            }

            // Table Columns
            var colHeaders = reportResult.Columns.Select(c => $"\"{EscapeCsv(c.Label)}\"");
            sb.AppendLine(string.Join(",", colHeaders));

            // Data Rows
            foreach (var row in reportResult.Data.Items)
            {
                var cells = reportResult.Columns.Select(col =>
                {
                    if (row.TryGetValue(col.Key, out var value) && value != null)
                    {
                        if (value is DateTime dt)
                            return $"\"{dt:yyyy-MM-dd HH:mm}\"";
                        if (value is DateOnly d)
                            return $"\"{d:yyyy-MM-dd}\"";
                        return $"\"{EscapeCsv(value.ToString()!)}\"";
                    }
                    return "\"\"";
                });
                sb.AppendLine(string.Join(",", cells));
            }

            // Return UTF-8 encoded bytes with BOM for Excel compatibility
            var preamble = Encoding.UTF8.GetPreamble();
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return [.. preamble, .. contentBytes];
        }

        #endregion

        #region Excel Export

        private static byte[] GenerateExcel(EnquiryReportResult reportResult, string title)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            var columnCount = Math.Max(reportResult.Columns.Count, 4);

            // Title Banner
            worksheet.Cell(1, 1).Value = title.ToUpperInvariant();
            var titleRange = worksheet.Range(1, 1, 1, columnCount);
            titleRange.Merge();
            titleRange.Style
                .Font.SetBold()
                .Font.SetFontSize(15)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1E3A8A"))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            worksheet.Row(1).Height = 35;

            // Metadata row
            var metaText = $"Generated On: {DateTime.Now:dd MMM yyyy, HH:mm}   |   Total Records: {reportResult.Data.TotalRecords}";
            worksheet.Cell(2, 1).Value = metaText;
            var metaRange = worksheet.Range(2, 1, 2, columnCount);
            metaRange.Merge();
            metaRange.Style
                .Font.SetItalic()
                .Font.SetFontSize(9)
                .Font.SetFontColor(XLColor.FromHtml("#475569"))
                .Fill.SetBackgroundColor(XLColor.FromHtml("#F1F5F9"))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Row(2).Height = 20;

            var currentRow = 4;

            // Summary Metrics Box
            if (reportResult.Metrics is { Count: > 0 })
            {
                worksheet.Cell(currentRow, 1).Value = "SUMMARY METRICS";
                worksheet.Cell(currentRow, 1).Style
                    .Font.SetBold()
                    .Font.SetFontSize(11)
                    .Font.SetFontColor(XLColor.FromHtml("#1E3A8A"));
                currentRow++;

                var metricStartRow = currentRow;
                for (var i = 0; i < reportResult.Metrics.Count; i++)
                {
                    var metric = reportResult.Metrics[i];
                    var colIndex = (i % 4) + 1;
                    if (i > 0 && i % 4 == 0)
                    {
                        currentRow += 2;
                    }

                    worksheet.Cell(currentRow, colIndex).Value = metric.Label;
                    worksheet.Cell(currentRow, colIndex).Style
                        .Font.SetFontSize(9)
                        .Font.SetFontColor(XLColor.FromHtml("#64748B"))
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#F8FAFC"));

                    worksheet.Cell(currentRow + 1, colIndex).Value = FormatMetricValue(metric);
                    worksheet.Cell(currentRow + 1, colIndex).Style
                        .Font.SetBold()
                        .Font.SetFontSize(12)
                        .Font.SetFontColor(XLColor.FromHtml("#0F172A"))
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#F8FAFC"));
                }
                currentRow += 3;
            }

            // Data Table Headers
            var tableHeaderRow = currentRow;
            for (var c = 0; c < reportResult.Columns.Count; c++)
            {
                var cell = worksheet.Cell(tableHeaderRow, c + 1);
                cell.Value = reportResult.Columns[c].Label;
                cell.Style
                    .Font.SetBold()
                    .Font.SetFontSize(10)
                    .Font.SetFontColor(XLColor.White)
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#2563EB"))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                    .Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            }
            worksheet.Row(tableHeaderRow).Height = 26;
            currentRow++;

            // Data Rows
            var dataStartRow = currentRow;
            foreach (var item in reportResult.Data.Items)
            {
                var isEven = (currentRow - dataStartRow) % 2 == 1;
                var rowBgColor = isEven ? XLColor.FromHtml("#F8FAFC") : XLColor.White;

                for (var c = 0; c < reportResult.Columns.Count; c++)
                {
                    var colDef = reportResult.Columns[c];
                    var cell = worksheet.Cell(currentRow, c + 1);

                    if (item.TryGetValue(colDef.Key, out var val) && val != null)
                    {
                        if (val is int intVal)
                        {
                            cell.Value = intVal;
                            cell.Style.NumberFormat.Format = "#,##0";
                            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        }
                        else if (val is long longVal)
                        {
                            cell.Value = longVal;
                            cell.Style.NumberFormat.Format = "#,##0";
                            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        }
                        else if (val is decimal decVal)
                        {
                            cell.Value = decVal;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        }
                        else if (val is double dblVal)
                        {
                            cell.Value = dblVal;
                            cell.Style.NumberFormat.Format = "#,##0.00";
                            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                        }
                        else if (val is DateTime dtVal)
                        {
                            cell.Value = dtVal;
                            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        }
                        else if (val is DateOnly doVal)
                        {
                            cell.Value = doVal.ToDateTime(TimeOnly.MinValue);
                            cell.Style.DateFormat.Format = "yyyy-MM-dd";
                            cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        }
                        else
                        {
                            cell.Value = val.ToString();
                        }
                    }
                    else
                    {
                        cell.Value = "-";
                        cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    }

                    cell.Style.Fill.SetBackgroundColor(rowBgColor);
                    cell.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
                    cell.Style.Border.SetBottomBorderColor(XLColor.FromHtml("#E2E8F0"));
                }

                worksheet.Row(currentRow).Height = 20;
                currentRow++;
            }

            // Adjust column widths automatically
            worksheet.Columns(1, reportResult.Columns.Count).AdjustToContents(10, 50);
            worksheet.ShowGridLines = true;

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return memoryStream.ToArray();
        }

        #endregion

        #region PDF Export

        private static byte[] GeneratePdf(EnquiryReportResult reportResult, string title)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(titleCol =>
                            {
                                titleCol.Item().Text("RENEWONE").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                                titleCol.Item().Text(title).FontSize(13).SemiBold().FontColor(Colors.Grey.Darken3);
                            });

                            row.ConstantItem(220).AlignRight().Column(metaCol =>
                            {
                                metaCol.Item().Text($"Date: {DateTime.Now:dd MMM yyyy, HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                                metaCol.Item().Text($"Total Records: {reportResult.Data.TotalRecords}").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                        });

                        col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Metrics cards banner
                        if (reportResult.Metrics is { Count: > 0 })
                        {
                            col.Item().PaddingBottom(8).Row(metricRow =>
                            {
                                foreach (var metric in reportResult.Metrics)
                                {
                                    metricRow.RelativeItem().PaddingHorizontal(3).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(5).Column(mBox =>
                                    {
                                        mBox.Item().Text(metric.Label).FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                                        mBox.Item().Text(FormatMetricValue(metric)).FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
                                    });
                                }
                            });
                        }
                    });

                    // Body: Data Table
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in reportResult.Columns)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        // Header
                        table.Header(header =>
                        {
                            foreach (var col in reportResult.Columns)
                            {
                                header.Cell()
                                    .Background(Colors.Blue.Darken2)
                                    .Padding(5)
                                    .Text(col.Label)
                                    .FontSize(8)
                                    .Bold()
                                    .FontColor(Colors.White);
                            }
                        });

                        // Rows
                        var isEven = false;
                        foreach (var item in reportResult.Data.Items)
                        {
                            isEven = !isEven;
                            var rowBg = isEven ? Colors.Grey.Lighten5 : Colors.White;

                            foreach (var col in reportResult.Columns)
                            {
                                var textVal = "-";
                                if (item.TryGetValue(col.Key, out var val) && val != null)
                                {
                                    textVal = val switch
                                    {
                                        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
                                        DateOnly d => d.ToString("yyyy-MM-dd"),
                                        decimal dec => dec.ToString("#,##0.00"),
                                        double dbl => dbl.ToString("#,##0.00"),
                                        _ => val.ToString() ?? "-"
                                    };
                                }

                                table.Cell()
                                    .Background(rowBg)
                                    .BorderBottom(0.5f)
                                    .BorderColor(Colors.Grey.Lighten3)
                                    .Padding(4)
                                    .Text(textVal)
                                    .FontSize(7.5f);
                            }
                        }
                    });

                    // Footer
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text(x =>
                        {
                            x.Span("RenewOne Admin Reports  •  Confidential").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                            x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        #endregion

        #region Helpers

        private static string FormatMetricValue(ReportResultMetric metric)
        {
            if (metric.Value == null)
                return "0";

            return metric.Format?.ToLowerInvariant() switch
            {
                "currency" => metric.Value switch
                {
                    decimal d => string.Format(CultureInfo.InvariantCulture, "₹ {0:N2}", d),
                    double db => string.Format(CultureInfo.InvariantCulture, "₹ {0:N2}", db),
                    float f => string.Format(CultureInfo.InvariantCulture, "₹ {0:N2}", f),
                    int i => string.Format(CultureInfo.InvariantCulture, "₹ {0:N0}", i),
                    long l => string.Format(CultureInfo.InvariantCulture, "₹ {0:N0}", l),
                    _ => $"₹ {metric.Value}"
                },
                "percentage" => metric.Value switch
                {
                    decimal d => $"{d:F1}%",
                    double db => $"{db:F1}%",
                    float f => $"{f:F1}%",
                    _ => $"{metric.Value}%"
                },
                "number" => metric.Value switch
                {
                    decimal d => string.Format(CultureInfo.InvariantCulture, "{0:N0}", d),
                    double db => string.Format(CultureInfo.InvariantCulture, "{0:N0}", db),
                    int i => string.Format(CultureInfo.InvariantCulture, "{0:N0}", i),
                    long l => string.Format(CultureInfo.InvariantCulture, "{0:N0}", l),
                    _ => metric.Value.ToString() ?? "0"
                },
                _ => metric.Value.ToString() ?? "-"
            };
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\"", "\"\"");
        }

        private static string GetDefaultReportTitle(EnquiryReportType reportType)
        {
            return reportType switch
            {
                EnquiryReportType.Status => "Enquiry Status Report",
                EnquiryReportType.Cancelled => "Cancelled Enquiries Report",
                EnquiryReportType.Pending => "Pending Enquiries Report",
                EnquiryReportType.VehicleType => "Vehicle Type Analysis Report",
                EnquiryReportType.InsuranceType => "Insurance Type Performance Report",
                EnquiryReportType.ConvertedPolicies => "Converted Policies Report",
                EnquiryReportType.EmployeePerformance => "Employee Performance Report",
                EnquiryReportType.AgencyPerformance => "Agency Performance Report",
                EnquiryReportType.InsuranceCompany => "Insurance Company Business Report",
                EnquiryReportType.DailyMonthly => "Daily / Monthly Enquiries Report",
                EnquiryReportType.ProfitAnalysis => "Profit & Commission Analysis Report",
                EnquiryReportType.DocumentPending => "Document Pending Report",
                _ => "Enquiry Report"
            };
        }

        #endregion
    }
}
