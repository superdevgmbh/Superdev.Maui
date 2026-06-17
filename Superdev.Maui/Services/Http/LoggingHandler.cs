using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Utils;

namespace Superdev.Maui.Services.Http
{
    /// <summary>
    /// HTTP message delegating handler which logs outgoing and incoming HTTP requests/responses.
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger logger;
        private readonly LoggingHandlerOptions options;

        public static bool Enabled = Debugger.IsAttached;

        public LoggingHandler(ILogger<LoggingHandler> logger, HttpMessageHandler innerHandler)
            : this(logger, innerHandler, options: null)
        {
        }

        public LoggingHandler(ILogger<LoggingHandler> logger, HttpMessageHandler innerHandler, LoggingHandlerOptions? options)
            : base(innerHandler)
        {
            this.logger = logger;
            this.options = options ?? new LoggingHandlerOptions();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.logger.LogDebug(await this.FormatHttpRequestLogMessageAsync(request));

            var stopwatch = Stopwatch.StartNew();

            var response = await base.SendAsync(request, cancellationToken);

            this.logger.LogDebug(await this.FormatHttpResponseLogMessageAsync(response, stopwatch.Elapsed));
            return response;
        }

        private async Task<string> FormatHttpRequestLogMessageAsync(HttpRequestMessage httpRequestMessage)
        {
            var headers = this.FormatHeaders(httpRequestMessage.Headers, httpRequestMessage.Content?.Headers);

            var requestMessage =
                $"{httpRequestMessage.Method} {httpRequestMessage.RequestUri}{Environment.NewLine}" +
                $"> Request Headers:{Environment.NewLine}{headers}";

            if (Enabled)
            {
                var content = await this.FormatContentAsync(httpRequestMessage.Content);
                requestMessage += Environment.NewLine +
                                  $"> Request Content: {content}";
            }

            return requestMessage;
        }

        private async Task<string> FormatHttpResponseLogMessageAsync(HttpResponseMessage httpResponseMessage, TimeSpan stopwatchElapsed)
        {
            if (Enabled && httpResponseMessage.Content is HttpContent httpContent)
            {
                // LoadIntoBufferAsync is used to load the content into a memory stream.
                // This is usually done automatically. However, if we want to log the content
                // and the content headers, we have to call this method manually.
                await httpContent.LoadIntoBufferAsync();
            }

            var httpStatusCode = httpResponseMessage.StatusCode;
            var headers = this.FormatHeaders(httpResponseMessage.Headers, httpResponseMessage.Content.Headers);

            string httpMethod;
            string requestUri;
            var httpRequestMessage = httpResponseMessage.RequestMessage;
            if (httpRequestMessage == null)
            {
                httpMethod = "<Method?>";
                requestUri = "<RequestUri?>";
            }
            else
            {
                httpMethod = httpRequestMessage.Method.ToString();
                requestUri = httpRequestMessage.RequestUri?.ToString() ?? "<RequestUri?>";
            }

            var responseMessage =
                $"{httpMethod} {requestUri} --> {(httpResponseMessage.IsSuccessStatusCode ? "Success" : "Failed")}{Environment.NewLine}" +
                $"> StatusCode: {(int)httpStatusCode} ({httpStatusCode}){Environment.NewLine}" +
                $"> Duration: {stopwatchElapsed.TotalSeconds:F3}{Environment.NewLine}" +
                $"> Response Headers:{Environment.NewLine}{headers}";

            if (Enabled)
            {
                var content = await this.FormatContentAsync(httpResponseMessage.Content);
                responseMessage += Environment.NewLine +
                                   $"> Response Content: {content}";
            }

            return responseMessage;
        }

        private async Task<string> FormatContentAsync(HttpContent? httpContent)
        {
            string formattedContent;

            if (httpContent?.Headers == null || httpContent.Headers.ContentLength == 0L)
            {
                formattedContent = "null";
            }
            else
            {
                var mediaType = httpContent.Headers.ContentType?.MediaType;
                if (this.IsContentLoggingEnabled(mediaType))
                {
                    formattedContent = await httpContent.ReadAsStringAsync();
                }
                else
                {
                    formattedContent = $"{{{httpContent.GetType().Name}, {ByteFormatter.GetNamedSize(httpContent.Headers.ContentLength ?? 0L)}}}";
                }
            }

            return formattedContent;
        }

        /// <summary>
        /// Determines whether a content's media type carries human-readable text that can be logged inline,
        /// based on <see cref="LoggingHandlerOptions.SupportedMediaTypes" />.
        /// </summary>
        private bool IsContentLoggingEnabled(string? mediaType)
        {
            if (string.IsNullOrEmpty(mediaType))
            {
                return false;
            }

            return this.options.SupportedMediaTypes.Any(pattern => IsSupportedMediaType(mediaType, pattern));
        }

        /// <summary>
        /// Matches a concrete <paramref name="mediaType" /> against a configured <paramref name="pattern" />.
        /// Supports exact matches, prefix wildcards (<c>text/*</c>) and structured-suffix wildcards (<c>*+json</c>).
        /// </summary>
        private static bool IsSupportedMediaType(string mediaType, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            if (string.Equals(mediaType, pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (pattern.StartsWith('*'))
            {
                return mediaType.EndsWith(pattern.TrimStart('*'), StringComparison.OrdinalIgnoreCase);
            }

            if (pattern.EndsWith('*'))
            {
                return mediaType.StartsWith(pattern.TrimEnd('*'), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private string FormatHeaders(HttpHeaders? httpHeaders, HttpContentHeaders? httpContentHeaders)
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

            return string.Join(Environment.NewLine, headers.Where(this.AllowedHeaders).Select(this.FormatHeaderKeyValuePair));
        }

        private bool AllowedHeaders(KeyValuePair<string, IEnumerable<string>> header)
        {
            return !this.options.FilteredHeaderNames.Contains(header.Key, StringComparer.OrdinalIgnoreCase);
        }

        private string FormatHeaderKeyValuePair(KeyValuePair<string, IEnumerable<string>> header)
        {
            var isSuppressed = this.options.SuppressedHeaderNames.Contains(header.Key, StringComparer.OrdinalIgnoreCase);

            var values = header.Value.Select(v => $"  {{{header.Key}: {(isSuppressed ? "{suppressed}" : v ?? "null")}}}");

            return string.Join(Environment.NewLine, values);
        }
    }
}