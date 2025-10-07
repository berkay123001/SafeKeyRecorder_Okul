using System;

namespace SafeKeyRecorder.Configuration;

public sealed class WebhookOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string BearerToken { get; set; } = string.Empty;

    public int RetryCount { get; set; } = 2;

    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public string PayloadName { get; set; } = "session_log.txt";
}
