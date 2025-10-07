namespace Knigavuhe.Models;

public class Config(IConfiguration configuration)
{
    public CsvConfig Csv { get; set; } = configuration.GetSection("CsvConfig").Get<CsvConfig>() ?? new CsvConfig();
}