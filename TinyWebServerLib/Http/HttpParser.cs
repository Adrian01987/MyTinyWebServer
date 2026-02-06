namespace TinyWebServerLib.Http;

/// <summary>
/// Parses raw HTTP request text into structured <see cref="HttpRequest"/> objects.
/// </summary>
public static class HttpParser
{
    /// <summary>
    /// Parses the raw HTTP request text into an <see cref="HttpRequest"/> object.
    /// </summary>
    /// <param name="requestText">The raw HTTP request as a string.</param>
    /// <returns>A parsed <see cref="HttpRequest"/> object.</returns>
    /// <exception cref="HttpParseException">Thrown when the request format is invalid.</exception>
    public static HttpRequest Parse(string requestText)
    {
        if (string.IsNullOrWhiteSpace(requestText))
        {
            throw new HttpParseException("Empty request");
        }

        string[] lines = requestText.Split("\r\n");
        if (lines.Length == 0)
        {
            throw new HttpParseException("Invalid request format: no lines found");
        }

        string[] requestLine = lines[0].Split(' ');
        if (requestLine.Length < 2)
        {
            throw new HttpParseException($"Invalid request line: '{lines[0]}'");
        }

        string method = requestLine[0].Trim();
        string path = requestLine[1];

        if (string.IsNullOrEmpty(method))
        {
            throw new HttpParseException("HTTP method is missing");
        }

        Dictionary<string, string> headers = [];
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            int separatorIndex = line.IndexOf(": ", StringComparison.Ordinal);
            if (separatorIndex > 0)
            {
                string headerName = line[..separatorIndex];
                string headerValue = line[(separatorIndex + 2)..];
                headers[headerName] = headerValue;
            }
        }

        // Body is handled separately by the server via Content-Length header,
        // since the parser may receive only the header portion of the request.
        return new HttpRequest(method, path, headers, string.Empty);
    }
}

/// <summary>
/// Exception thrown when HTTP request parsing fails.
/// </summary>
public class HttpParseException : Exception
{
    /// <summary>
    /// Creates a new instance of <see cref="HttpParseException"/> with the specified message.
    /// </summary>
    /// <param name="message">The error message describing the parse failure.</param>
    public HttpParseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="HttpParseException"/> with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message describing the parse failure.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public HttpParseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
