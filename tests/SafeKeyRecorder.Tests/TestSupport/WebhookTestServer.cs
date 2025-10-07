using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using RichardSzalay.MockHttp;

namespace SafeKeyRecorder.Tests.TestSupport;

public sealed class WebhookTestServer : IDisposable
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly Queue<Func<HttpResponseMessage>> _responseQueue = new();
    private bool _disposed;

    public WebhookTestServer(string? baseAddress = null)
    {
        BaseAddress = new Uri(baseAddress ?? "https://webhook.test/");
        _mockHttp = new MockHttpMessageHandler();
        _mockHttp.When("*").Respond(HandleRequest);
    }

    public Uri BaseAddress { get; }

    public List<WebhookTestRequest> Requests { get; } = new();

    public HttpClient CreateClient()
    {
        EnsureNotDisposed();
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = BaseAddress;
        return client;
    }

    public void EnqueueResponse(HttpStatusCode statusCode, string? body = null, string? mediaType = "text/plain")
    {
        EnsureNotDisposed();
        _responseQueue.Enqueue(CreateResponse(statusCode, body, mediaType));
    }

    public void Clear()
    {
        EnsureNotDisposed();
        Requests.Clear();
        _responseQueue.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _mockHttp.Dispose();
        Requests.Clear();
        _responseQueue.Clear();
    }

    private HttpResponseMessage HandleRequest(HttpRequestMessage request)
    {
        var captured = WebhookTestRequest.FromHttpRequest(request);
        Requests.Add(captured);
        var responder = _responseQueue.Count > 0
            ? _responseQueue.Dequeue()
            : CreateResponse(HttpStatusCode.OK);

        return responder();
    }

    private static Func<HttpResponseMessage> CreateResponse(HttpStatusCode statusCode, string? body = null, string? mediaType = "text/plain")
    {
        return () =>
        {
            var response = new HttpResponseMessage(statusCode);
            if (body is not null)
            {
                response.Content = mediaType is null
                    ? new StringContent(body)
                    : new StringContent(body, System.Text.Encoding.UTF8, mediaType);
            }

            return response;
        };
    }

    private static Func<HttpResponseMessage> CreateResponse(HttpStatusCode statusCode)
    {
        return CreateResponse(statusCode, null, null);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(WebhookTestServer));
        }
    }
}

public sealed record WebhookTestRequest(
    HttpMethod Method,
    Uri Uri,
    IReadOnlyDictionary<string, string[]> Headers,
    string Body)
{
    public string? GetHeader(string name)
    {
        return Headers.TryGetValue(name, out var values)
            ? string.Join(", ", values)
            : null;
    }

    internal static WebhookTestRequest FromHttpRequest(HttpRequestMessage request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in request.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }

        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = header.Value.ToArray();
            }
        }

        var body = request.Content is null
            ? string.Empty
            : request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        return new WebhookTestRequest(
            request.Method,
            request.RequestUri ?? throw new InvalidOperationException("RequestUri cannot be null."),
            headers,
            body);
    }
}
