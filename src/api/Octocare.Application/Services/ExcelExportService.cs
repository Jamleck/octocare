using System.Reflection;
using ClosedXML.Excel;

namespace Octocare.Application.Services;

public class ExcelExportService
{
    public byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Write headers
        for (var i = 0; i < properties.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = FormatHeaderName(properties[i].Name);
            cell.Style.Font.Bold = true;
        }

        // Write data rows
        var rowIndex = 2;
        foreach (var item in data)
        {
            for (var i = 0; i < properties.Length; i++)
            {
                var value = properties[i].GetValue(item);
                var cell = worksheet.Cell(rowIndex, i + 1);

                if (value is null)
                {
                    cell.Value = string.Empty;
                }
                else if (value is decimal d)
                {
                    cell.Value = d;
                    cell.Style.NumberFormat.Format = "#,##0.00";
                }
                else if (value is int intVal)
                {
                    cell.Value = intVal;
                }
                else if (value is bool b)
                {
                    cell.Value = b ? "Yes" : "No";
                }
                else if (value is DateTime dt)
                {
                    cell.Value = dt;
                    cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
                }
                else if (value is DateOnly dateOnly)
                {
                    cell.Value = dateOnly.ToString("yyyy-MM-dd");
                }
                else
                {
                    cell.Value = value.ToString();
                }
            }
            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    private static string FormatHeaderName(string propertyName)
    {
        // Insert spaces before uppercase letters: "ParticipantName" -> "Participant Name"
        var chars = new List<char>();
        for (var i = 0; i < propertyName.Length; i++)
        {
            if (i > 0 && char.IsUpper(propertyName[i]) && !char.IsUpper(propertyName[i - 1]))
            {
                chars.Add(' ');
            }
            chars.Add(propertyName[i]);
        }
        return new string(chars.ToArray());
    }
}
