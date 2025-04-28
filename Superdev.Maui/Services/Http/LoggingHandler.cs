using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Services.Http
{
    /// <summary>
    /// HTTP message delegating handler which logs outgoing and incoming HTTP requests/responses.
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        private static readonly string[] FilteredHeaderNames = { "Server" };
        private static readonly string[] SuppressedHeaderNames = { "Authorization" };

        private readonly ILogger logger;

        public static bool Enabled
#if DEBUG
            = true;
#else
            = false;
#endif

        public LoggingHandler(ILogger<LoggingHandler> logger, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this.logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.logger.LogDebug(await FormatHttpRequestLogMessage(request));

            var stopwatch = Stopwatch.StartNew();

            var response = await base.SendAsync(request, cancellationToken);

            this.logger.LogDebug(await FormatHttpResponseLogMessageAsync(response, stopwatch.Elapsed));
            return response;
        }

        private static async Task<string> FormatHttpRequestLogMessage(HttpRequestMessage httpRequestMessage)
        {
            var headers = FormatHeaders(httpRequestMessage.Headers, httpRequestMessage.Content?.Headers);

            var requestMessage =
                $"{httpRequestMessage.Method} {httpRequestMessage.RequestUri}{Environment.NewLine}" +
                $"> Request Headers:{Environment.NewLine}{headers}";

            if (Enabled)
            {
                var content = await FormatContentAsync(httpRequestMessage.Content);
                requestMessage += Environment.NewLine +
                                  $"> Request Content: {content}";
            }

            return requestMessage;
        }

        private static async Task<string> FormatHttpResponseLogMessageAsync(HttpResponseMessage httpResponseMessage, TimeSpan stopwatchElapsed)
        {
            if (Enabled && httpResponseMessage.Content is HttpContent httpContent)
            {
                // LoadIntoBufferAsync is used to load the content into a memory stream.
                // This is usually done automatically. However, if we want to log the content
                // and the content headers, we have to call this method manually.
                await httpContent.LoadIntoBufferAsync();
            }

            var httpStatusCode = httpResponseMessage.StatusCode;
            var headers = FormatHeaders(httpResponseMessage.Headers, httpResponseMessage.Content?.Headers);

            var responseMessage =
                $"{httpResponseMessage.RequestMessage.Method} {httpResponseMessage.RequestMessage.RequestUri} --> {(httpResponseMessage.IsSuccessStatusCode ? "Success" : "Failed")}{Environment.NewLine}" +
                $"> StatusCode: {(int)httpStatusCode} ({httpStatusCode}){Environment.NewLine}" +
                $"> Duration: {stopwatchElapsed.TotalSeconds:F3}{Environment.NewLine}" +
                $"> Response Headers:{Environment.NewLine}{headers}";

            if (Enabled)
            {
                var content = await FormatContentAsync(httpResponseMessage.Content);
                responseMessage += Environment.NewLine +
                                   $"> Response Content: {content}";
            }

            return responseMessage;
        }

        private static async Task<string> FormatContentAsync(HttpContent httpContent)
        {
            string formattedContent;

            if (httpContent?.Headers == null || httpContent.Headers.ContentLength == 0L)
            {
                formattedContent = "null";
            }
            else
            {
                var mediaType = httpContent.Headers?.ContentType?.MediaType;
                if (mediaType == MediaTypeNames.Application.Json)
                {
                    formattedContent = await httpContent.ReadAsStringAsync();
                }
                else
                {
                    formattedContent = $"{{{httpContent.GetType().Name}, {ByteFormatter.GetNamedSize(httpContent.Headers?.ContentLength ?? 0L)}}}";
                }
            }

            return formattedContent;
        }

        private static string FormatHeaders(HttpHeaders httpHeaders, HttpContentHeaders httpContentHeaders)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = new List<KeyValuePair<string, IEnumerable<string>>>();

            if (httpHeaders != null)
            {
                headers = headers.Concat(httpHeaders);
            }

            if (httpContentHeaders != null)
            {
                headers = headers.Concat(httpContentHeaders);
            }

            return string.Join(Environment.NewLine, headers.Where(AllowedHeaders).Select(FormatHeaderKeyValuePair));
        }

        private static bool AllowedHeaders(KeyValuePair<string, IEnumerable<string>> header)
        {
            return !FilteredHeaderNames.Contains(header.Key);
        }

        private static string FormatHeaderKeyValuePair(KeyValuePair<string, IEnumerable<string>> header)
        {
            var isSuppressed = SuppressedHeaderNames.Contains(header.Key);

            var values = header.Value.Select(v => $"  {{{header.Key}: {(isSuppressed ? "{suppressed}" : v ?? "null")}}}");

            return string.Join(Environment.NewLine, values);
        }
    }
}