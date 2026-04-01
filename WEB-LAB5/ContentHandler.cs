using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace WEB_LAB5;

public static class ContentHandler
{
    public static string Process(HttpResponse response)
    {
        var contentType = response.ContentType.ToLower();

        if (contentType.Contains("application/json"))
            return ProcessJson(response.Body);
        return contentType.Contains("text/html") ? ProcessHtml(response.Body) : response.Body;
    }

    private static string ProcessJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }

    private static string ProcessHtml(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Remove script, style, and noscript elements
        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//noscript|//svg|//head");
        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        // Extract text with line breaks for block elements
        var text = ExtractText(doc.DocumentNode);

        // Decode HTML entities
        text = HttpUtility.HtmlDecode(text);

        // Collapse multiple blank lines into one
        text = Regex.Replace(text, @"\n{3,}", "\n\n");

        return text.Trim();
    }

    private static string ExtractText(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = node.InnerText.Trim();
            return text.Length > 0 ? text + " " : "";
        }

        var blockTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "p", "div", "br", "h1", "h2", "h3", "h4", "h5", "h6",
            "li", "tr", "blockquote", "pre", "hr", "article", "section"
        };

        var result = new System.Text.StringBuilder();

        foreach (var child in node.ChildNodes)
        {
            var childText = ExtractText(child);
            if (blockTags.Contains(child.Name))
            {
                result.Append('\n');
                result.Append(childText);
                result.Append('\n');
            }
            else
            {
                result.Append(childText);
            }
        }

        return result.ToString();
    }
}
