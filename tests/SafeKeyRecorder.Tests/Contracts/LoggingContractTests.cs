using System.IO;
using System.Text.Json;
using Xunit;

namespace SafeKeyRecorder.Tests.Contracts;

public class LoggingContractTests
{
    private static readonly string ContractPath = Path.GetFullPath("../../../../../specs/001-bu-uygulama-c/contracts/logging.json");

    [Fact]
    public void LoggingContract_ShouldDefineAppendAndPurgeOperations()
    {
        Assert.True(File.Exists(ContractPath), $"Missing logging contract at {ContractPath}.");

        var json = File.ReadAllText(ContractPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("SessionLogging", root.GetProperty("title").GetString());
        Assert.Equal("{ISO8601 timestamp}, {key symbol}", root.GetProperty("entryFormat").GetString());
        Assert.True(root.TryGetProperty("operations", out var operations) && operations.GetArrayLength() >= 2, "Logging operations must include append and purge.");

        var append = operations.EnumerateArray().FirstOrDefault(op => op.GetProperty("name").GetString() == "AppendKey");
        Assert.True(append.ValueKind == JsonValueKind.Object, "AppendKey operation must exist.");

        var purge = operations.EnumerateArray().FirstOrDefault(op => op.GetProperty("name").GetString() == "PurgeNow");
        Assert.True(purge.ValueKind == JsonValueKind.Object, "PurgeNow operation must exist.");
    }
}
