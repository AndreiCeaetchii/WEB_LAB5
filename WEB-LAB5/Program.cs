namespace WEB_LAB5;

class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "-h")
        {
            PrintHelp();
            return;
        }

        var httpClient = new RawHttpClient();
        var cache = new HttpCache();

        switch (args[0])
        {
            case "-u":
                if (args.Length < 2) { PrintHelp(); return; }
                HandleUrl(args[1], httpClient, cache);
                break;
            case "-s":
                if (args.Length < 2) { PrintHelp(); return; }
                var searchTerm = string.Join(" ", args.Skip(1));
                HandleSearch(searchTerm, httpClient, cache);
                break;
            default:
                PrintHelp();
                break;
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  go2web -u <URL>          Make an HTTP request to the specified URL and print the response");
        Console.WriteLine("  go2web -s <search-term>  Search the term using DuckDuckGo and print top 10 results");
        Console.WriteLine("  go2web -h                Show this help");
    }

    private static void HandleUrl(string url, RawHttpClient httpClient, HttpCache cache)
    {
        var response = FetchWithCache(url, httpClient, cache);
        var content = ContentHandler.Process(response);
        Console.WriteLine(content);
    }

    private static void HandleSearch(string term, RawHttpClient httpClient, HttpCache cache)
    {
        var engine = new SearchEngine(httpClient);
        var results = engine.Search(term);
        SearchEngine.DisplayResults(results);

        if (results.Count == 0) return;

        Console.WriteLine("\nEnter a result number to open it (or press Enter to exit):");
        var input = Console.ReadLine()?.Trim();
        if (int.TryParse(input, out var choice) && choice >= 1 && choice <= results.Count)
        {
            Console.WriteLine($"\nFetching: {results[choice - 1].Url}\n");
            HandleUrl(results[choice - 1].Url, httpClient, cache);
        }
    }

    private static HttpResponse FetchWithCache(string url, RawHttpClient httpClient, HttpCache cache)
    {
        var cached = cache.Get(url);

        if (cached != null && HttpCache.IsFresh(cached))
        {
            return new HttpResponse
            {
                StatusCode = cached.StatusCode,
                Body = cached.Body,
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Content-Type"] = cached.ContentType
                }
            };
        }

        Dictionary<string, string>? extraHeaders = null;
        if (cached != null)
            extraHeaders = cache.GetValidationHeaders(cached);

        var response = httpClient.Get(url, extraHeaders);

        if (response.StatusCode == 304 && cached != null)
        {
            Console.WriteLine("[Cache] 304 Not Modified");
            return new HttpResponse
            {
                StatusCode = cached.StatusCode,
                Body = cached.Body,
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Content-Type"] = cached.ContentType
                }
            };
        }

        cache.Store(url, response);
        return response;
    }
}
