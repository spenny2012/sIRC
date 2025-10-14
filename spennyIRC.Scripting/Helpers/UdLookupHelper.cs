using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace spennyIRC.Scripting.Helpers;

public static class UdLookupHelper
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://www.urbandictionary.com/")
    };

    public static async Task<List<UdDefinition>> UdLookupAsync(string ud)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ud, nameof(ud));

        try
        {
            string encodedTerm = Uri.EscapeDataString(ud);
            HttpResponseMessage response = await _httpClient.GetAsync($"define.php?term={encodedTerm}");
            response.EnsureSuccessStatusCode();

            string html = await response.Content.ReadAsStringAsync();
            return ParseDefinitions(html, ud);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to fetch data from Urban Dictionary: {ex.Message}", ex);
        }
    }

    private static List<UdDefinition> ParseDefinitions(string html, string term)
    {
        List<UdDefinition> definitions = [];
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        // Find all definition containers
        HtmlNodeCollection definitionNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'definition')]");

        if (definitionNodes == null || !definitionNodes.Any())
            return definitions;

        foreach (HtmlNode defNode in definitionNodes)
        {
            UdDefinition definition = new()
            {
                Word = term
            };

            // Extract meaning - contained within .definition
            HtmlNode meaningNode = defNode.SelectSingleNode("//div[contains(@class, 'meaning')]");
            if (meaningNode != null)
            {
                definition.Meaning = CleanText(meaningNode.InnerText);
            }

            // Extract example - contained within .definition
            HtmlNode exampleNode = defNode.SelectSingleNode("//div[contains(@class, 'example')]");
            if (exampleNode != null)
            {
                definition.Example = CleanText(exampleNode.InnerText);
            }

            // Try to extract author (often in a contributor div)
            HtmlNode authorNode = defNode.SelectSingleNode("//div[contains(@class, 'contributor')]//a");
            if (authorNode != null)
            {
                definition.Author = CleanText(authorNode.InnerText);
            }

            definitions.Add(definition);
        }

        return definitions;
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remove excessive whitespace and trim
        return Regex.Replace(text, @"\s+", " ").Trim();
    }
}