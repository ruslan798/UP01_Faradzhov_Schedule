using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница аналитики и диаграмм: интерактивная визуализация расписания через LiveCharts и экспорт в Word, Excel, PDF
/// </summary>
public partial class DiagrammPage : Page
{
    public DiagrammPage()
    {
        InitializeComponent();
        UpdateChart(null, null);
    }

    private void UpdateChart(object? sender, SelectionChangedEventArgs? e)
    {
        if (!IsInitialized) return;

        try
        {
            using var db = new ScheduleDbContext();
            var items = db.ScheduleItems
                .Include(s => s.Teacher)
                .Include(s => s.Group)
                .Include(s => s.Subject)
                .Include(s => s.TimeSlot)
                .AsNoTracking()
                .ToList();

            string metric = (cmbMetric.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Нагрузка преподавателей";
            string chartType = (cmbChartType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Столбчатая";

            var labels = new List<string>();
            var values = new ChartValues<double>();

            if (metric.Contains("преподавателей"))
            {
                var teachers = db.Teachers.AsNoTracking().OrderBy(t => t.FullName).ToList();
                foreach (var t in teachers)
                {
                    labels.Add(t.FullName);
                    values.Add(items.Count(i => i.TeacherId == t.Id));
                }
                AxisY.Title = "Количество пар (шт.)";
                AxisX.Title = "Преподаватели";
            }
            else if (metric.Contains("дням недели"))
            {
                string[] days = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота" };
                foreach (var d in days)
                {
                    labels.Add(d);
                    values.Add(items.Count(i => i.DayOfWeek == d));
                }
                AxisY.Title = "Количество пар (шт.)";
                AxisX.Title = "Дни недели";
            }
            else // Структура занятий по типам
            {
                string[] types = { "Лекция", "Практическое занятие", "Лабораторная работа", "Семинар", "Консультация" };
                foreach (var t in types)
                {
                    labels.Add(t);
                    values.Add(items.Count(i => i.LessonType == t));
                }
                AxisY.Title = "Количество пар (шт.)";
                AxisX.Title = "Типы занятий";
            }

            if (chartType.Contains("Круговая"))
            {
                CartesianChartControl.Visibility = Visibility.Collapsed;
                PieChartControl.Visibility = Visibility.Visible;

                var pieSeries = new SeriesCollection();
                for (int i = 0; i < labels.Count; i++)
                {
                    if (values[i] > 0 || values.All(v => v == 0))
                    {
                        pieSeries.Add(new PieSeries
                        {
                            Title = labels[i],
                            Values = new ChartValues<double> { values[i] },
                            DataLabels = true,
                            LabelPoint = cp => $"{cp.Participation:P0} ({cp.Y})"
                        });
                    }
                }
                PieChartControl.Series = pieSeries;
            }
            else
            {
                PieChartControl.Visibility = Visibility.Collapsed;
                CartesianChartControl.Visibility = Visibility.Visible;

                AxisX.Labels = labels;
                var seriesCol = new SeriesCollection();

                if (chartType.Contains("Линейный"))
                {
                    seriesCol.Add(new LineSeries
                    {
                        Title = metric,
                        Values = values,
                        DataLabels = true
                    });
                }
                else // Столбчатая
                {
                    seriesCol.Add(new ColumnSeries
                    {
                        Title = metric,
                        Values = values,
                        DataLabels = true
                    });
                }

                CartesianChartControl.Series = seriesCol;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка обновления диаграммы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #region Экспорт в Microsoft Word (.docx)

    private void btnExportWord_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new ScheduleDbContext();
            var items = db.ScheduleItems
                .Include(s => s.TimeSlot)
                .Include(s => s.Group)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlot.PairNumber)
                .ToList();

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(dir, "Schedule_Report_Faradzhov.docx");

            using (var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Заголовок документа
                var titlePara = body.AppendChild(new Paragraph());
                var titleRun = titlePara.AppendChild(new Run(new Text("СВОДНОЕ РАСПИСАНИЕ УЧЕБНЫХ ЗАНЯТИЙ")));
                var tp = titleRun.AppendChild(new RunProperties());
                tp.AppendChild(new Bold());
                tp.AppendChild(new FontSize { Val = "28" });
                tp.AppendChild(new Color { Val = "233E76" });

                var subPara = body.AppendChild(new Paragraph());
                var subRun = subPara.AppendChild(new Run(new Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm} | Студент: Фараджов Р.Э. (4ИСИП-523) | Руководитель: Аксёнова Т.Г.")));
                var sp = subRun.AppendChild(new RunProperties());
                sp.AppendChild(new Italic());
                sp.AppendChild(new FontSize { Val = "18" });
                sp.AppendChild(new Color { Val = "5A6578" });

                body.AppendChild(new Paragraph());

                // Таблица
                var table = new Table();
                var tblPr = new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 6 },
                        new BottomBorder { Val = BorderValues.Single, Size = 6 },
                        new LeftBorder { Val = BorderValues.Single, Size = 6 },
                        new RightBorder { Val = BorderValues.Single, Size = 6 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 }
                    )
                );
                table.AppendChild(tblPr);

                string[] headers = { "№", "День недели", "Пара (время)", "Группа", "Дисциплина", "Преподаватель", "Аудитория", "Тип" };
                var headerRow = new TableRow();
                foreach (var h in headers)
                {
                    var cell = new TableCell(new Paragraph(new Run(new Text(h)) { RunProperties = new RunProperties(new Bold(), new Color { Val = "FFFFFF" }) }));
                    cell.AppendChild(new TableCellProperties(new Shading { Val = ShadingPatternValues.Clear, Fill = "0198AF" }));
                    headerRow.AppendChild(cell);
                }
                table.AppendChild(headerRow);

                int idx = 1;
                foreach (var itm in items)
                {
                    var tr = new TableRow();
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(idx++.ToString())))));
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(itm.DayOfWeek)))));
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(itm.TimeSlot.DisplayTime)))));
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(itm.Group.GroupName)))));
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(itm.Subject.Name)))));
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(itm.Teacher.FullName)))));
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(itm.Classroom.RoomNumber)))));
                    tr.AppendChild(new TableCell(new Paragraph(new Run(new Text(itm.LessonType)))));
                    table.AppendChild(tr);
                }

                body.AppendChild(table);
                mainPart.Document.Save();
            }

            var res = MessageBox.Show($"Отчет Word успешно сохранен:\n{filePath}\n\nОткрыть созданный файл?", "Успешный экспорт", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (res == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта в Word: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Экспорт в Microsoft Excel (.xlsx) с формулами =SUM

    private void btnExportExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new ScheduleDbContext();
            var items = db.ScheduleItems
                .Include(s => s.TimeSlot)
                .Include(s => s.Group)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlot.PairNumber)
                .ToList();

            var teachers = db.Teachers.AsNoTracking().OrderBy(t => t.FullName).ToList();

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(dir, "Schedule_Report_Faradzhov.xlsx");

            using (var wb = new XLWorkbook())
            {
                // Лист 1: Сводное расписание
                var ws = wb.Worksheets.Add("Расписание занятий");

                ws.Cell(1, 1).Value = "СВОДНАЯ ВЕДОМОСТЬ РАСПИСАНИЯ ЗАНЯТИЙ";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#233E76");

                ws.Cell(2, 1).Value = $"Дата выгрузки: {DateTime.Now:dd.MM.yyyy HH:mm} | Студент: Фараджов Р.Э. (4ИСИП-523)";
                ws.Cell(2, 1).Style.Font.Italic = true;

                string[] headers = { "№", "День недели", "Пара (время)", "Группа", "Дисциплина", "Преподаватель", "Аудитория", "Тип занятия", "Неделя" };
                for (int c = 0; c < headers.Length; c++)
                {
                    var cell = ws.Cell(4, c + 1);
                    cell.Value = headers[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0198AF");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = 5;
                int idx = 1;
                foreach (var itm in items)
                {
                    ws.Cell(row, 1).Value = idx++;
                    ws.Cell(row, 2).Value = itm.DayOfWeek;
                    ws.Cell(row, 3).Value = itm.TimeSlot.DisplayTime;
                    ws.Cell(row, 4).Value = itm.Group.GroupName;
                    ws.Cell(row, 5).Value = itm.Subject.Name;
                    ws.Cell(row, 6).Value = itm.Teacher.FullName;
                    ws.Cell(row, 7).Value = itm.Classroom.RoomNumber;
                    ws.Cell(row, 8).Value = itm.LessonType;
                    ws.Cell(row, 9).Value = itm.WeekType;
                    row++;
                }

                var tblRange = ws.Range(4, 1, row - 1, headers.Length);
                tblRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                tblRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Columns().AdjustToContents();

                // Лист 2: Педагогическая нагрузка и формулы SUM
                var ws2 = wb.Worksheets.Add("Педагогическая нагрузка");

                ws2.Cell(1, 1).Value = "НАГРУЗКА ПРЕПОДАВАТЕЛЕЙ ПО РАСПИСАНИЮ";
                ws2.Cell(1, 1).Style.Font.Bold = true;
                ws2.Cell(1, 1).Style.Font.FontSize = 14;
                ws2.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#233E76");

                string[] tHeaders = { "№", "ФИО преподавателя", "Ученая степень", "Назначено пар (шт.)", "Академических часов (ч.)" };
                for (int c = 0; c < tHeaders.Length; c++)
                {
                    var cell = ws2.Cell(3, c + 1);
                    cell.Value = tHeaders[c];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#233E76");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                int r2 = 4;
                int tIdx = 1;
                foreach (var t in teachers)
                {
                    int pairs = items.Count(i => i.TeacherId == t.Id);
                    int hours = pairs * 2;

                    ws2.Cell(r2, 1).Value = tIdx++;
                    ws2.Cell(r2, 2).Value = t.FullName;
                    ws2.Cell(r2, 3).Value = t.AcademicDegree;
                    ws2.Cell(r2, 4).Value = pairs;
                    ws2.Cell(r2, 5).Value = hours;
                    r2++;
                }

                // Строка ИТОГО с формулами =SUM(...)
                ws2.Cell(r2, 2).Value = "ИТОГО ВСЕГО:";
                ws2.Cell(r2, 2).Style.Font.Bold = true;
                ws2.Cell(r2, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws2.Cell(r2, 4).FormulaA1 = $"=SUM(D4:D{r2 - 1})";
                ws2.Cell(r2, 4).Style.Font.Bold = true;

                ws2.Cell(r2, 5).FormulaA1 = $"=SUM(E4:E{r2 - 1})";
                ws2.Cell(r2, 5).Style.Font.Bold = true;
                ws2.Cell(r2, 5).Style.Font.FontColor = XLColor.Red;

                var tRange = ws2.Range(3, 1, r2, tHeaders.Length);
                tRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                tRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws2.Columns().AdjustToContents();

                wb.SaveAs(filePath);
            }

            var res = MessageBox.Show($"Отчет Excel успешно сохранен с формулами =SUM:\n{filePath}\n\nОткрыть созданный файл?", "Успешный экспорт", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (res == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Экспорт в Adobe PDF (.pdf)

    private void btnExportPdf_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new ScheduleDbContext();
            var items = db.ScheduleItems
                .Include(s => s.TimeSlot)
                .Include(s => s.Group)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlot.PairNumber)
                .ToList();

            string dir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(dir, "Schedule_Report_Faradzhov.pdf");

            var pdfDoc = new PdfDocument();
            var page = pdfDoc.AddPage();
            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
            var gfx = XGraphics.FromPdfPage(page);

            var titleFont = new XFont("Arial", 16, XFontStyle.Bold);
            var subFont = new XFont("Arial", 10, XFontStyle.Italic);
            var headerFont = new XFont("Arial", 9, XFontStyle.Bold);
            var textFont = new XFont("Arial", 8, XFontStyle.Regular);

            // Шапка
            gfx.DrawString("ОФИЦИАЛЬНОЕ РАСПИСАНИЕ УЧЕБНЫХ ЗАНЯТИЙ", titleFont, XBrushes.Navy, new XPoint(30, 40));
            gfx.DrawString($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm} | Студент: Фараджов Р.Э. (4ИСИП-523) | Руководитель: Аксёнова Т.Г.", subFont, XBrushes.Gray, new XPoint(30, 60));

            // Заголовки колонок таблицы
            double y = 80;
            double[] colWidths = { 30, 90, 110, 80, 170, 150, 75, 75 };
            string[] headers = { "№", "День", "Время", "Группа", "Дисциплина", "Преподаватель", "Каб.", "Тип" };

            double x = 30;
            for (int i = 0; i < headers.Length; i++)
            {
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(1, 152, 175)), x, y, colWidths[i], 22);
                gfx.DrawString(headers[i], headerFont, XBrushes.White, new XRect(x, y + 4, colWidths[i], 18), XStringFormats.TopCenter);
                x += colWidths[i];
            }

            y += 22;
            int idx = 1;
            foreach (var itm in items.Take(20))
            {
                x = 30;
                string[] rowData = {
                    (idx++).ToString(),
                    itm.DayOfWeek,
                    itm.TimeSlot.DisplayTime,
                    itm.Group.GroupName,
                    itm.Subject.Name.Length > 28 ? itm.Subject.Name.Substring(0, 25) + "..." : itm.Subject.Name,
                    itm.Teacher.FullName.Length > 24 ? itm.Teacher.FullName.Substring(0, 21) + "..." : itm.Teacher.FullName,
                    itm.Classroom.RoomNumber,
                    itm.LessonType
                };

                for (int i = 0; i < rowData.Length; i++)
                {
                    gfx.DrawRectangle(XPens.LightGray, x, y, colWidths[i], 18);
                    gfx.DrawString(rowData[i], textFont, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[i] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[i];
                }
                y += 18;
            }

            // Колонтитул внизу
            gfx.DrawString($"Всего записей в расписании: {items.Count} | Страница 1 из 1", subFont, XBrushes.Gray, new XPoint(30, page.Height - 30));

            pdfDoc.Save(filePath);

            var res = MessageBox.Show($"Отчет PDF успешно сохранен:\n{filePath}\n\nОткрыть созданный файл?", "Успешный экспорт", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (res == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка экспорта в PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}
