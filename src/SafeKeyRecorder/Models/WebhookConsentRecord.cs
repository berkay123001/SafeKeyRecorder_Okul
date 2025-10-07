using System;

namespace SafeKeyRecorder.Models;

public sealed class WebhookConsentRecord
{
    public WebhookConsentRecord(Guid sessionId, DateTimeOffset? grantedAt, bool isGranted)
    {
        SessionId = sessionId;
        GrantedAt = isGranted ? grantedAt : null;
        IsGranted = isGranted;
    }

    public Guid SessionId { get; }

    public DateTimeOffset? GrantedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsGranted { get; private set; }

    public bool CanUpload => IsGranted && RevokedAt is null;

    public void Grant(DateTimeOffset grantedAt)
    {
        GrantedAt = grantedAt;
        RevokedAt = null;
        IsGranted = true;
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        RevokedAt = revokedAt;
        IsGranted = false;
    }
}
