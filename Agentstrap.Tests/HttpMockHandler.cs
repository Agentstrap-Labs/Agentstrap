using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agentstrap.Tests
{
    /// <summary>
    /// Test helper: HTTP handler used to mock ipinfo.io and Bark responses without
    /// any real network access. Records every request so tests can assert that nothing
    /// was sent when the feature was disabled or the address was ineligible.
    /// </summary>
    internal sealed class HttpMockHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responder;

        public int Count = 0;

        public readonly List<HttpRequestMessage> Requests = new();

        public HttpMockHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
        {
            _responder = responder;
        }

        public static HttpMockHandler Json(string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
            Body("application/json", json, statusCode);

        public static HttpMockHandler Body(string mimeType, string body, HttpStatusCode statusCode = HttpStatusCode.OK) =>
            new((_message, _token) => Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, mimeType),
            }));

        public static HttpMockHandler Delayed(TimeSpan delay, HttpResponseMessage response) =>
            new(async (request, cancellationToken) =>
            {
                // observe cancellation like a real transport would: a stalled server
                // still gets aborted by the HttpClient cancellation token
                await Task.Delay(delay, cancellationToken);
                return response;
            });

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count += 1;
            Requests.Add(request);

            return _responder(request, cancellationToken);
        }
    }
}
