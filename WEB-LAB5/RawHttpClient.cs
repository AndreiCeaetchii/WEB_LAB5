using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace WEB_LAB5;

public class RawHttpClient
{
    private const int MaxRedirects = 5;

    public HttpResponse Get(string url, Dictionary<string, string>? extraHeaders = null)
    {
        return GetWithRedirects(url, extraHeaders, 0);
    }

    private HttpResponse GetWithRedirects(string url, Dictionary<string, string>? extraHeaders, int redirectCount)
    {
        if (redirectCount > MaxRedirects)
            throw new Exception("Too many redirects");

        var uri = ParseUrl(url);
        var response = SendRequest(uri, extraHeaders);

        if (!response.IsRedirect || response.RedirectLocation == null) return response;
        var newUrl = response.RedirectLocation;
        if (newUrl.StartsWith("/"))
            newUrl = $"{uri.Scheme}://{uri.Host}{newUrl}";

        Console.WriteLine($"[Redirect {response.StatusCode}] -> {newUrl}");
        return GetWithRedirects(newUrl, extraHeaders, redirectCount + 1);

    }

    private HttpResponse SendRequest(Uri uri, Dictionary<string, string>? extraHeaders)
    {
        var port = uri.Scheme == "https" ? 443 : 80;
        using var tcp = new TcpClient(uri.Host, port);
        Stream stream = tcp.GetStream();

        if (uri.Scheme == "https")
        {
            var ssl = new SslStream(stream, false);
            ssl.AuthenticateAsClient(uri.Host);
            stream = ssl;
        }

        var request = BuildRequest(uri, extraHeaders);
        var requestBytes = Encoding.ASCII.GetBytes(request);
        stream.Write(requestBytes, 0, requestBytes.Length);
        stream.Flush();

        return ReadResponse(stream);
    }

    private static string BuildRequest(Uri uri, Dictionary<string, string>? extraHeaders)
    {
        var path = string.IsNullOrEmpty(uri.PathAndQuery) ? "/" : uri.PathAndQuery;
        var sb = new StringBuilder();
        sb.Append($"GET {path} HTTP/1.1\r\n");
        sb.Append($"Host: {uri.Host}\r\n");
        sb.Append("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36\r\n");
        sb.Append("Accept: application/json, text/html, text/plain;q=0.9, */*;q=0.8\r\n");
        sb.Append("Accept-Encoding: identity\r\n");
        sb.Append("Connection: close\r\n");

        if (extraHeaders != null)
        {
            foreach (var (key, value) in extraHeaders)
                sb.Append($"{key}: {value}\r\n");
        }

        sb.Append("\r\n");
        return sb.ToString();
    }

    private HttpResponse ReadResponse(Stream stream)
    {
        var reader = new StreamReader(stream, Encoding.UTF8);

        // Read status line
        var statusLine = reader.ReadLine();
        if (statusLine == null)
            throw new Exception("Empty response");

        var parts = statusLine.Split(' ', 3);
        var statusCode = int.Parse(parts[1]);
        var statusText = parts.Length > 2 ? parts[2] : "";

        // Read headers
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? line;
        while ((line = reader.ReadLine()) != null && line != "")
        {
            var colonIdx = line.IndexOf(':');
            if (colonIdx > 0)
            {
                var key = line[..colonIdx].Trim();
                var value = line[(colonIdx + 1)..].Trim();
                headers[key] = value;
            }
        }

        // Read body
        string body;
        try
        {
            if (headers.TryGetValue("Transfer-Encoding", out var te) &&
                te.Equals("chunked", StringComparison.OrdinalIgnoreCase))
            {
                body = ReadChunkedBody(reader);
            }
            else
            {
                body = reader.ReadToEnd();
            }
        }
        catch (IOException)
        {
            body = "";
        }

        return new HttpResponse
        {
            StatusCode = statusCode,
            StatusText = statusText,
            Headers = headers,
            Body = body
        };
    }

    private static string ReadChunkedBody(StreamReader reader)
    {
        var sb = new StringBuilder();
        try
        {
            while (true)
            {
                var sizeLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(sizeLine)) continue;

                // Strip chunk extensions (e.g., "1a2b;ext=val" -> "1a2b")
                var semiIdx = sizeLine.IndexOf(';');
                var sizeStr = (semiIdx >= 0 ? sizeLine[..semiIdx] : sizeLine).Trim();

                if (!int.TryParse(sizeStr, System.Globalization.NumberStyles.HexNumber, null, out var size) || size == 0)
                    break;

                var buffer = new char[size];
                var read = 0;
                while (read < size)
                {
                    var n = reader.Read(buffer, read, size - read);
                    if (n == 0) break;
                    read += n;
                }
                sb.Append(buffer, 0, read);
                reader.ReadLine(); // consume trailing \r\n
            }
        }
        catch (IOException)
        {
            // Connection closed by server mid-read; return what we have
        }
        return sb.ToString();
    }

    private static Uri ParseUrl(string url)
    {
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;
        return new Uri(url);
    }
}
