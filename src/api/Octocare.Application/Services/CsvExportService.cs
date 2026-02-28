using System.Globalization;
using CsvHelper;

namespace Octocare.Application.Services;

public class CsvExportService
{
    public byte[] GenerateCsv<T>(IEnumerable<T> data)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(data);
        }

        return memoryStream.ToArray();
    }
}
