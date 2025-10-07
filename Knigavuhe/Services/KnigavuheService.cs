using System.Collections.Concurrent;
using Knigavuhe.Clients;
using Knigavuhe.Models;

namespace Knigavuhe.Services;

public class KnigavuheService(
    KnigavuheClient knigavuheClient,
    CsvService csvService,
    Config config)
{
    public async Task WriteAuthorsLinksToCsv()
    {
        var page = 0;
        var authorsCount = 0;
        while (authorsCount <= config.Csv.MaxAuthors)
        {
            var authorsLinksString = await knigavuheClient.GetAuthorsLinks(page);
            var authorsLinks = authorsLinksString.Select(
                link => new AuthorLink { Link = link }).ToList();

            csvService.WriteCsv(config.Csv.AuthorsLinksCsvFilePath, authorsLinks);

            page++;
            authorsCount += authorsLinks.Count;
            
            Console.WriteLine($"Получено {authorsLinks.Count}({authorsCount}/{config.Csv.MaxAuthors}) ссылок на авторов");
            await Task.Delay(1000);
        }
    }

    public async Task WriteAuthorsToCsv()
    {
        var authors = new List<Author>();
        var authorLinks = csvService.ReadCsv<AuthorLink>(config.Csv.AuthorsLinksCsvFilePath);
        var authorsCount = 0;
        await Parallel.ForEachAsync(authorLinks, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (authorLink, ct) =>
        {
            var author = await knigavuheClient.GetAuthor(authorLink.Link);
            authors.Add(author);

            Interlocked.Increment(ref authorsCount);
            
            Console.WriteLine($"Получено {author.Name} ({authorsCount}/{authorLinks.Count})");
            await Task.Delay(1000, ct);
        });
        
        csvService.WriteCsv(config.Csv.AuthorsCsvFilePath, authors);
        Console.WriteLine($"Сохранено {authors.Count}");
    }

    public async Task WriteTrackToCsv()
    {
        var allTracks = new ConcurrentBag<AudioTrack>();
        var authors = csvService.ReadCsv<Author>(config.Csv.AuthorsCsvFilePath);
        var authorsCount = 0;
        await Parallel.ForEachAsync(authors, new ParallelOptions { MaxDegreeOfParallelism = 15 }, async (author, _) =>
        {
            var tracks = await knigavuheClient.GetTracks(author.BookLink);

            foreach (var track in tracks)
            {
                allTracks.Add(track);
            }
            
            Interlocked.Increment(ref authorsCount);
            
            Console.WriteLine($"Получено {tracks.Count} треков для {author.Name} ({authorsCount}/{authors.Count})");
        });
        
        csvService.WriteCsv(config.Csv.TracksCsvFilePath, allTracks);
    }

    public async Task DownloadMp3()
    {
        var tracks = csvService.ReadCsv<AudioTrack>(config.Csv.TracksCsvFilePath);
        var groupedTracks = tracks.GroupBy(track => track.PlayerData.Title);

        foreach (var group in groupedTracks)
        {
            var tracksDuration = 0.0d;
            var tracksPaths = new List<Mp3Path>();
            
            Console.WriteLine($"Грузим \"{group.First().Title}\"");
            
            foreach (var track in group)
            {
                var path = await knigavuheClient.GetMp3S(track);
                tracksPaths.Add(new Mp3Path { Path = path, Duration = track.Duration });
                
                tracksDuration += track.Duration;
                
                if (tracksDuration >= TimeSpan.FromMinutes(config.Csv.MaxDurationMin).TotalSeconds)
                {
                    break;
                }
            }
            
            csvService.WriteCsv(config.Csv.Mp3Path, tracksPaths);
        }
    }
}