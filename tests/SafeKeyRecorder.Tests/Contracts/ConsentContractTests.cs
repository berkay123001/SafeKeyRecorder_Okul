using System.IO;
using System.Text.Json;
using Xunit;

namespace SafeKeyRecorder.Tests.Contracts;

public class ConsentContractTests
{
    private static readonly string ContractPath = Path.GetFullPath("../../../../../specs/001-bu-uygulama-c/contracts/consent.json");

    [Fact]
    public void ConsentContract_ShouldMatchRequiredFields()
    {
        Assert.True(File.Exists(ContractPath), $"Missing consent contract at {ContractPath}.");

        var json = File.ReadAllText(ContractPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("ConsentFlow", root.GetProperty("title").GetString());
        Assert.True(root.TryGetProperty("states", out var states) && states.GetArrayLength() > 0, "Consent states must be defined.");

        var awaiting = states[0];
        Assert.Equal("AwaitingConsent", awaiting.GetProperty("name").GetString());
        Assert.True(awaiting.TryGetProperty("actions", out var actions) && actions.GetArrayLength() >= 2, "Consent actions must include continue and cancel.");
    }
}
