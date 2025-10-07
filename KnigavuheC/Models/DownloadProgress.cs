namespace Knigavuhe.Models;

public class DownloadProgress(long bytesDownloaded, long totalBytes)
{
    public long BytesDownloaded { get; } = bytesDownloaded;
    public long TotalBytes { get; } = totalBytes;

    public double? ProgressPercentage => TotalBytes > 0 ? 
        (double)BytesDownloaded / TotalBytes * 100 : null;
}