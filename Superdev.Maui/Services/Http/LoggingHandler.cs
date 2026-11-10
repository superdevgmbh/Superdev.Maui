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

        /// <summary>
        /// Global kill switch for this handler. Body logging is configured per instance through
        /// <see cref="LoggingHandlerOptions.LogContent" />.
        /// </summary>
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
            // Nothing below is free: the header formatting alone allocates per request, so it must not
            // run when the log level would discard the result anyway.
            if (!Enabled || !this.logger.IsEnabled(LogLevel.Debug))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            this.logger.LogDebug("{HttpRequest}", await this.FormatHttpRequestLogMessageAsync(request, cancellationToken));

            var stopwatch = Stopwatch.StartNew();

            var response = await base.SendAsync(request, cancellationToken);

            this.logger.LogDebug(
                "{HttpResponse}", await this.FormatHttpResponseLogMessageAsync(response, stopwatch.Elapsed, cancellationToken));
            return response;
        }

        private async Task<string> FormatHttpRequestLogMessageAsync(
            HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            var headers = this.FormatHeaders(httpRequestMessage.Headers, httpRequestMessage.Content?.Headers);

            var requestMessage =
                $"{httpRequestMessage.Method} {this.FormatRequestUri(httpRequestMessage.RequestUri)}{Environment.NewLine}" +
                $"> Request Headers:{Environment.NewLine}{headers}";

            if (this.options.LogContent)
            {
                var content = await this.FormatContentAsync(httpRequestMessage.Content, cancellationToken);
                requestMessage += Environment.NewLine +
                                  $"> Request Content: {content}";
            }

            return requestMessage;
        }

        /// <summary>
        /// Formats a request URI for the log. The query string is redacted unless
        /// <see cref="LoggingHandlerOptions.LogQueryString" /> is set, because API keys and signed-URL
        /// tokens are carried there.
        /// </summary>
        private string FormatRequestUri(Uri? requestUri)
        {
            if (requestUri == null)
            {
                return "<RequestUri?>";
            }

            if (this.options.LogQueryString || !requestUri.IsAbsoluteUri || string.IsNullOrEmpty(requestUri.Query))
            {
                return requestUri.ToString();
            }

            return $"{requestUri.GetLeftPart(UriPartial.Path)}?{{redacted}}";
        }

        private async Task<string> FormatHttpResponseLogMessageAsync(
            HttpResponseMessage httpResponseMessage, TimeSpan stopwatchElapsed, CancellationToken cancellationToken)
        {
            if (httpResponseMessage.Content is HttpContent httpContent && this.ShouldLogContent(httpContent))
            {
                // LoadIntoBufferAsync loads the content into a memory stream, which is what makes it
                // readable twice. The media type is checked first, so that a large download or an
                // open-ended stream is not buffered only to be logged as a placeholder.
                await httpContent.LoadIntoBufferAsync(cancellationToken);
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
                requestUri = this.FormatRequestUri(httpRequestMessage.RequestUri);
            }

            var responseMessage =
                $"{httpMethod} {requestUri} --> {(httpResponseMessage.IsSuccessStatusCode ? "Success" : "Failed")}{Environment.NewLine}" +
                $"> StatusCode: {(int)httpStatusCode} ({httpStatusCode}){Environment.NewLine}" +
                $"> Duration: {stopwatchElapsed.TotalSeconds:F3}{Environment.NewLine}" +
                $"> Response Headers:{Environment.NewLine}{headers}";

            if (this.options.LogContent)
            {
                var content = await this.FormatContentAsync(httpResponseMessage.Content, cancellationToken);
                responseMessage += Environment.NewLine +
                                   $"> Response Content: {content}";
            }

            return responseMessage;
        }

        private async Task<string> FormatContentAsync(HttpContent? httpContent, CancellationToken cancellationToken)
        {
            string formattedContent;

            if (httpContent?.Headers == null || httpContent.Headers.ContentLength == 0L)
            {
                formattedContent = "null";
            }
            else if (this.ShouldLogContent(httpContent))
            {
                formattedContent = await httpContent.ReadAsStringAsync(cancellationToken);

                if (this.options.MaxContentLength >= 0 && formattedContent.Length > this.options.MaxContentLength)
                {
                    formattedContent =
                        $"{formattedContent[..this.options.MaxContentLength]}… ({formattedContent.Length} characters, truncated)";
                }
            }
            else
            {
                formattedContent = $"{{{httpContent.GetType().Name}, {ByteFormatter.GetNamedSize(httpContent.Headers.ContentLength ?? 0L)}}}";
            }

            return formattedContent;
        }

        /// <summary>
        /// Determines whether the body of <paramref name="httpContent" /> is logged inline.
        /// </summary>
        private bool ShouldLogContent(HttpContent? httpContent)
        {
            return this.options.LogContent &&
                   httpContent?.Headers != null &&
                   this.IsContentLoggingEnabled(httpContent.Headers.ContentType?.MediaType);
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