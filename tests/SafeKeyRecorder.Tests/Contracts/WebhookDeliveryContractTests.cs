using System.IO;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace SafeKeyRecorder.Tests.Contracts;

public sealed class WebhookDeliveryContractTests
{
    private static readonly string SpecPath = Path.GetFullPath("../../../../../specs/002-kullan-c-n/contracts/webhook_delivery.json");

    [Fact]
    public void SpecFile_ShouldExist()
    {
        Assert.True(File.Exists(SpecPath), $"Webhook delivery contract missing at {SpecPath}.");
    }

    [Fact]
    public void Contract_ShouldSpecifyRequiredHeadersAndMethod()
    {
        Assert.True(File.Exists(SpecPath), $"Webhook delivery contract missing at {SpecPath}.");

        using var stream = File.OpenRead(SpecPath);
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        Assert.Equal("WebhookDelivery", root.GetProperty("title").GetString());

        var http = root.GetProperty("http");
        Assert.Equal(HttpMethod.Post.Method, http.GetProperty("method").GetString());
        Assert.Equal("https://webhook.site/1c7b4698-6c44-43fe-a985-5657c977e353", http.GetProperty("url").GetString());

        var headers = http.GetProperty("headers");
        Assert.Contains(headers.EnumerateObject(), prop => prop.Name.Equals("Authorization") && prop.Value.GetString() == "Bearer <token>");
        Assert.Contains(headers.EnumerateObject(), prop => prop.Name.Equals("Content-Type") && prop.Value.GetString() == "text/plain; charset=utf-8");

        var body = root.GetProperty("body");
        Assert.Equal("session_log", body.GetProperty("format").GetString());
        Assert.Equal("text", body.GetProperty("type").GetString());
    }

    [Fact]
    public void Contract_ShouldDocumentFailureResponses()
    {
        Assert.True(File.Exists(SpecPath), $"Webhook delivery contract missing at {SpecPath}.");

        using var stream = File.OpenRead(SpecPath);
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        var failures = root.GetProperty("failureResponses");
        Assert.True(failures.GetArrayLength() >= 2, "At least two failure responses should be documented.");

        Assert.Contains(failures.EnumerateArray(), element => element.GetProperty("status").GetInt32() == 400);
        Assert.Contains(failures.EnumerateArray(), element => element.GetProperty("status").GetInt32() == 500);
    }
}
