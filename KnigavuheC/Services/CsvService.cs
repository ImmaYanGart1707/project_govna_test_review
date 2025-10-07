using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Knigavuhe.Services;

public class CsvService
{
    public List<T> ReadCsv<T>(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<T>().ToList();
    }

    public void WriteCsv<T>(string filePath, IEnumerable<T> records)
    {
        var fileExists = File.Exists(filePath) && new FileInfo(filePath).Length > 0;
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = !fileExists
        };
        
        using var writer = new StreamWriter(filePath, append: true);
        using var csv = new CsvWriter(writer, config);
        
        csv.WriteRecords(records);
    }
}