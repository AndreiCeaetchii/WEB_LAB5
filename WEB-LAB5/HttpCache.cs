using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WEB_LAB5;

public class HttpCache
{
    private readonly string _cacheDir;

    public HttpCache()
    {
        _cacheDir = Path.Combine(AppContext.BaseDirectory, ".go2web_cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public CachedResponse? Get(string url)
    {
        var path = GetCachePath(url);
        if (!File.Exists(path)) return null;

        try
        {
            var json = File.ReadAllText(path);
            var cached = JsonSerializer.Deserialize<CachedResponse>(json);
            if (cached == null) return null;

            // Check if still fresh based on max-age
            if (cached.MaxAge > 0)
            {
                var age = (DateTimeOffset.UtcNow - cached.CachedAt).TotalSeconds;
                if (age < cached.MaxAge)
                {
                    Console.WriteLine("[Cache] Fresh hit");
                    return cached;
                }
            }

            // Stale but has validators for conditional request
            if (!string.IsNullOrEmpty(cached.ETag) || !string.IsNullOrEmpty(cached.LastModified))
            {
                Console.WriteLine("[Cache] Stale, will revalidate");
                return cached;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public void Store(string url, HttpResponse response)
    {
        var cached = new CachedResponse
        {
            Url = url,
            Body = response.Body,
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ETag = response.GetHeader("ETag"),
            LastModified = response.GetHeader("Last-Modified"),
            CachedAt = DateTimeOffset.UtcNow
        };

        var cacheControl = response.GetHeader("Cache-Control");
        if (cacheControl != null)
        {
            // Check for no-cache or no-store
            if (cacheControl.Contains("no-cache", StringComparison.OrdinalIgnoreCase) ||
                cacheControl.Contains("no-store", StringComparison.OrdinalIgnoreCase))
            {
                return; // Don't cache this response
            }

            var parts = cacheControl.Split(',').Select(p => p.Trim());
            foreach (var part in parts)
            {
                if (part.StartsWith("max-age=") && int.TryParse(part[8..], out var maxAge))
                    cached.MaxAge = maxAge;
            }
        }

        var json = JsonSerializer.Serialize(cached, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetCachePath(url), json);
    }

    public Dictionary<string, string> GetValidationHeaders(CachedResponse cached)
    {
        var headers = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(cached.ETag))
            headers["If-None-Match"] = cached.ETag;
        if (!string.IsNullOrEmpty(cached.LastModified))
            headers["If-Modified-Since"] = cached.LastModified;
        return headers;
    }

    public static bool IsFresh(CachedResponse cached)
    {
        if (cached.MaxAge <= 0) return false;
        var age = (DateTimeOffset.UtcNow - cached.CachedAt).TotalSeconds;
        return age < cached.MaxAge;
    }

    private string GetCachePath(string url)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        var hex = Convert.ToHexString(hash).ToLower();
        return Path.Combine(_cacheDir, hex + ".json");
    }
}

public class CachedResponse
{
    public string Url { get; set; } = "";
    public string Body { get; set; } = "";
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = "";
    public string? ETag { get; set; }
    public string? LastModified { get; set; }
    public int MaxAge { get; set; }
    public DateTimeOffset CachedAt { get; set; }
}
