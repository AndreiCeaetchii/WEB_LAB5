using HtmlAgilityPack;
using System.Web;

namespace WEB_LAB5;

public class SearchEngine
{
    private readonly RawHttpClient _httpClient;

    public SearchEngine(RawHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public List<SearchResult> Search(string query)
    {
        var encoded = HttpUtility.UrlEncode(query);
        var url = $"https://html.duckduckgo.com/html/?q={encoded}";
        var response = _httpClient.Get(url);

        return ParseDuckDuckGoResults(response.Body);
    }

    private static List<SearchResult> ParseDuckDuckGoResults(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var results = new List<SearchResult>();

        // DuckDuckGo HTML results are in <a class="result__a"> tags
        var links = doc.DocumentNode.SelectNodes("//a[contains(@class, 'result__a')]");
        if (links == null) return results;

        foreach (var link in links)
        {
            var href = link.GetAttributeValue("href", "");
            var title = HttpUtility.HtmlDecode(link.InnerText?.Trim() ?? "");

            // DuckDuckGo sometimes wraps URLs in a redirect
            if (href.Contains("uddg="))
            {
                var match = System.Text.RegularExpressions.Regex.Match(href, @"uddg=([^&]+)");
                if (match.Success)
                    href = HttpUtility.UrlDecode(match.Groups[1].Value);
            }

            if (!string.IsNullOrEmpty(title) && href.StartsWith("http"))
            {
                results.Add(new SearchResult { Title = title, Url = href });
            }

            if (results.Count >= 10) break;
        }

        return results;
    }

    public static void DisplayResults(List<SearchResult> results)
    {
        if (results.Count == 0)
        {
            Console.WriteLine("No results found.");
            return;
        }

        for (var i = 0; i < results.Count; i++)
        {
            Console.WriteLine($"\n  {i + 1}. {results[i].Title}");
            Console.WriteLine($"     {results[i].Url}");
        }
    }
}

public class SearchResult
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
}
