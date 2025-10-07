using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Knigavuhe.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Knigavuhe.Services;

public class KnigavuheParser
{
    public List<string> GetAuthorsLinks(string html)
    {
        return GetHrefs(html, "/reader/");
    }
    
    public List<string> GetBooksLinks(string html)
    {
        return GetHrefs(html, "/book/");
    }

    public Author GetAuthor(string html, string rootUrl, string url)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var name = "Empty";
        var nameNode = doc.DocumentNode.SelectSingleNode("//div[@class='author_header_name']");

        var bookLink = GetBooksLinks(html);
        var bl = bookLink.Count > 1 ? bookLink[1] : (bookLink.Count > 0 ? bookLink.First() : "/book/chumaznik/");
        
        return new Author
        {
            Name = name,
            Link = url,
            BookLink = $"{rootUrl}{bl}"
        };
    }
    
    public List<AudioTrack> GetAudioTracks(string input)
    {
        var playlistStart = input.IndexOf("\"playlist\":", StringComparison.InvariantCulture);
        var arrayStart = input.IndexOf('[', playlistStart);

        var bracketCount = 0;
        var arrayEnd = -1;

        for (int i = arrayStart; i < input.Length; i++)
        {
            if (input[i] == '[')
            {
                bracketCount++;
            }
            else if (input[i] == ']')
            {
                bracketCount--;
                if (bracketCount == 0)
                {
                    arrayEnd = i;
                    break;
                }
            }
        }

        if (arrayEnd == -1)
        {
            throw new Exception("Eee chto-to ne tak 1");
        }

        var jsonArray = input.Substring(arrayStart, arrayEnd - arrayStart + 1);

        try
        {
            return JArray.Parse(jsonArray).ToObject<List<AudioTrack>>() 
                   ?? throw new Exception("Eee chto-to ne tak 2");
        }
        catch (Exception e)
        {
            throw new Exception("Eee chto-to ne tak 3", e);
        }
    }

    private List<string> GetHrefs(string html, string contains)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc.DocumentNode
            .SelectNodes($"//a[contains(@href, '{contains}') and (contains(@class, 'authors_author_cover') or contains(@class, 'book_snippet'))]")
            .Select(node => node.GetAttributeValue("href", string.Empty))
            .ToList();
    }
}