using System.Net.Sockets;
using System.Text;

namespace spennyIRC.Scripting.Helpers;

public static class WordLookupHelper
{
    /// <summary>
    /// Look up a word definition from dict.org
    /// </summary>
    public static async Task<string> DefineAsync(string word)
    {
        try
        {
            using TcpClient client = new();
            await client.ConnectAsync("dict.org", 2628);

            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[8192];
            StringBuilder result = new();

            // Read greeting
            await stream.ReadAsync(buffer);

            // Send DEFINE command with CRLF line ending
            string command = $"DEFINE * {word}\r\n";
            byte[] cmdBytes = Encoding.UTF8.GetBytes(command);
            await stream.WriteAsync(cmdBytes);

            // Read entire response
            StringBuilder response = new();
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                string text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                response.Append(text);

                // Check if we've received the end of definitions
                if (text.Contains("250 ok") || text.Contains("552 no match"))
                    break;
            }

            // Send QUIT
            byte[] quit = Encoding.UTF8.GetBytes("QUIT\r\n");
            await stream.WriteAsync(quit);

            string fullResponse = response.ToString();

            // Parse the response
            if (fullResponse.Contains("552 no match"))
            {
                return $"No definitions found for '{word}'";
            }
            else if (fullResponse.Contains("150") && fullResponse.Contains("151"))
            {
                // Extract definitions
                string[] lines = fullResponse.Split('\n');
                bool inDefinition = false;

                foreach (string line in lines)
                {
                    if (line.StartsWith("151"))
                    {
                        // Extract database name
                        string[] parts = line.Split(' ', 4);
                        if (parts.Length >= 4)
                        {
                            result.AppendLine($"\n[{parts[3].Trim().Trim('"')}]");
                        }
                        inDefinition = true;
                    }
                    else if (line.Trim() == ".")
                    {
                        inDefinition = false;
                    }
                    else if (inDefinition && !line.StartsWith("250"))
                    {
                        result.AppendLine(line.TrimEnd('\r'));
                    }
                }

                return result.Length > 0 ? result.ToString() : "No definitions found";
            }
            else
            {
                return $"No definitions found for '{word}'";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}