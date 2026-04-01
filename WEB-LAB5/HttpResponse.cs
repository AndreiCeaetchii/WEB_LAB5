namespace WEB_LAB5;

public class HttpResponse
{
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string Body { get; set; } = "";

    public string? GetHeader(string name)
    {
        return Headers.TryGetValue(name, out var value) ? value : null;
    }

    public bool IsRedirect => StatusCode is 301 or 302 or 303 or 307;

    public string? RedirectLocation => GetHeader("Location");

    public string ContentType => GetHeader("Content-Type") ?? "text/html";
}
