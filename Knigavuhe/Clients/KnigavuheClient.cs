using System.Net;
using Knigavuhe.Models;
using Knigavuhe.Services;
using Polly;
using Polly.Extensions.Http;

namespace Knigavuhe.Clients;

public class KnigavuheClient
{
    private readonly KnigavuheParser _knigavuheParser;
    private readonly Config _config;
    private readonly List<HttpClient> _httpClients;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private int _currentProxyIndex;

    public KnigavuheClient(KnigavuheParser knigavuheParser, Config config)
    {
        _knigavuheParser = knigavuheParser;
        _config = config;
        _httpClients = CreateHttpClients();
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task<List<string>> GetAuthorsLinks(int page)
    {
        var url = $"https://knigavuhe.org/readers/{page}/?sort=popularity&period=alltime&asc=0";
        
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var client = GetNextHttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Add("cookie", "new_design=1;");
            var result = await client.SendAsync(message);
            result.EnsureSuccessStatusCode();
            return result;
        });

        var responseString = await response.Content.ReadAsStringAsync();
        return _knigavuheParser.GetAuthorsLinks(responseString);
    }
    
    public async Task<Author> GetAuthor(string author)
    {
        var rootUrl = "https://knigavuhe.org";
        var url = $"{rootUrl}{author}";
        
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var client = GetNextHttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Add("cookie", "new_design=1;");
            var result = await client.SendAsync(message);
            result.EnsureSuccessStatusCode();
            return result;
        });

        var responseString = await response.Content.ReadAsStringAsync();
        return _knigavuheParser.GetAuthor(responseString, rootUrl, url);
    }

    public async Task<List<AudioTrack>> GetTracks(string bookLink)
    {
        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var client = GetNextHttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, bookLink);
            message.Headers.Add("cookie", "new_design=1;");
            var result = await client.SendAsync(message);
            result.EnsureSuccessStatusCode();
            return result;
        });

        var responseString = await response.Content.ReadAsStringAsync();
        return _knigavuheParser.GetAudioTracks(responseString);
    }

    public async Task<string> GetMp3S(AudioTrack audioTrack)
    {
        var path = $"{_config.Csv.Mp3Path}/" +
                   $"{audioTrack.PlayerData.Readers}_" +
                   $"{audioTrack.Title}({audioTrack.Duration}).mp3";
        
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var client = GetNextHttpClient();
            var response = await client.GetAsync(audioTrack.Url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
       
            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var totalBytesRead = 0L;
            var buffer = new byte[8192];
            
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(
                path, 
                FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 8192, useAsync: true);
       
            int bytesRead;
            var bytesOffset = 0;
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
                bytesOffset += bytesRead;
           
                if (totalBytes > 0 && bytesOffset > 1000000)
                {
                    bytesOffset = 0;
                    var progressPercentage = (double)totalBytesRead / totalBytes * 100;
                    Console.WriteLine($"Скачано: {totalBytesRead:N0} / {totalBytes:N0} байт ({progressPercentage:F1}%)");
                }
            }
       
            return response;
        });

        return path;
    }
    
    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (_, timespan, retryCount, _) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
                });
    }
    
    private List<HttpClient> CreateHttpClients()
    {
        var clients = new List<HttpClient>();
        for (int port = 9001; port <= 9050; port++)
        {
            var proxy = new WebProxy($"192.168.4.2:{port}");
            
            var handler = new SocketsHttpHandler
            {
                Proxy = proxy,
                UseProxy = true
            };

            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(30);
            
            clients.Add(client);
        }
        
        return clients;
    }
    
    private HttpClient GetNextHttpClient()
    {
        var client = _httpClients[_currentProxyIndex];
        _currentProxyIndex = (_currentProxyIndex + 1) % _httpClients.Count;
        return client;
    }
}