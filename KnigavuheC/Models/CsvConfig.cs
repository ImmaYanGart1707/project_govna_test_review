namespace Knigavuhe.Models;

public class CsvConfig
{
    public int MaxAuthors { get; set; } = 1000;
    public string AuthorsLinksCsvFilePath { get; set; } = null!;
    public string AuthorsCsvFilePath { get; set; } = null!;
    public string TracksCsvFilePath { get; set; } = null!;
    public string TracksInfoCsvFilePath { get; set; } = null!;
    public string Mp3Path { get; set; } = null!;
    public int MaxDurationMin { get; set; } = 30;
}