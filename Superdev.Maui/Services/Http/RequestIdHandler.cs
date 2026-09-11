namespace Superdev.Maui.Services.Http
{
    /// <summary>
    /// Tags each HTTP request with a unique X-Request-ID.
    /// </summary>
    public class RequestIdHandler : DelegatingHandler
    {
        private readonly string headerKey;

        public RequestIdHandler(HttpMessageHandler innerHandler, string headerKey = "X-Request-ID")
            : base(innerHandler)
        {
            this.headerKey = headerKey;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(this.headerKey))
            {
                var requestId = Guid.NewGuid().ToString("D");
                request.Headers.Add(this.headerKey, requestId);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}