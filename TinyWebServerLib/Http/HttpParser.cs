namespace TinyWebServerLib.Http;

public static class HttpParser
{
    public static HttpRequest Parse(string requestText)
    {
        string[] lines = requestText.Split("\r\n");
        string[] requestLine = lines[0].Split(" ");
        string method = requestLine[0].Trim();
        string path = requestLine[1];
        Dictionary<string, string> headers = [];
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }
            string[] headerParts = line.Split(": ");
            string headerName = headerParts[0];
            string headerValue = headerParts[1];
            headers.Add(headerName, headerValue);
        }
        string body = string.Join("\r\n", lines.SkipWhile(l => !string.IsNullOrWhiteSpace(l)).Skip(1));
        return new HttpRequest(method, path, headers, body);
    }
}
