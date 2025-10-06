# Data Model – Arka Plan Rızası ve Sürekli Kayıt Modu

## Entities

### ConsentDecision
- **Id** (Guid)
- **UserId** (string) – lokal profil veya cihaz kimliği
- **AllowBackgroundCapture** (bool)
- **ResumeEvents** (List<BackgroundResumeEvent>)
- **CreatedAtUtc** (DateTime)
- **UpdatedAtUtc** (DateTime)
- **PurgedAtUtc** (DateTime?) – purge tetiklendiğinde doldurulur
- **LogRetentionPreference** (enum `Keep24h` | `PurgeImmediately`)

### BackgroundTelemetryEvent
- **EventId** (Guid)
- **ConsentDecisionId** (Guid)
- **EventType** (enum `backgroundResume`, `backgroundPause`, `backgroundToggleChanged`, `backgroundConsentRecorded`)
- **CapturedAtUtc** (DateTime)
- **Mode** (string, `background` veya `foreground`)
- **EncryptedPayload** (byte[])
- **QueueState** (`Pending`, `Queued`, `Flushed`)

### SystemLockEvent
- **EventId** (Guid)
- **OccurredAtUtc** (DateTime)
- **IsLocked** (bool)
- **Source** (enum `DBus`, `LoginCtl`, `ManualOverride`)

### SessionLogConfiguration
- **CaptureWhenInactive** (bool)
- **LastToggleUtc** (DateTime)
- **ForegroundOnlyFallbackReason** (string?) – global hook başarısızsa kullanıcıya gösterilen mesaj

## Relationships

- Her `ConsentDecision` birden çok `BackgroundTelemetryEvent` kaydına sahiptir (`1:n`).
- `BackgroundTelemetryEvent.ConsentDecisionId` `ConsentDecision.Id` alanına yabancı anahtardır.
- `SystemLockEvent` kayıtları telemetriye bağlanmaz ancak `BackgroundTelemetryEvent` üretimini tetikler.
- `SessionLogConfiguration` tekil yapı; `SessionLogService` tarafından yüklenir ve `BackgroundCaptureService` ile paylaşılır.

## State Diagram

```
Idle
  └─(Consent Accepted, AllowBackgroundCapture=false)→ ForegroundCaptureOnly
  └─(Consent Accepted, AllowBackgroundCapture=true)→ BackgroundCapture
BackgroundCapture
  └─(SystemLock=true)→ Suspended
Suspended
  └─(SystemLock=false, Toggle=true)→ BackgroundCapture
BackgroundCapture
  └─(Toggle=false)→ ForegroundCaptureOnly
ForegroundCaptureOnly
  └─(Toggle=true)→ BackgroundCapture
ForegroundCaptureOnly | BackgroundCapture
  └─(PurgeTriggered)→ Idle (logs flushed, consent stored)
```

## Data Constraints

- `AllowBackgroundCapture` varsayılan olarak `false` olmalıdır.
- `BackgroundTelemetryEvent` yalnızca `AllowBackgroundCapture=true` olan kararlar için oluşturulur.
- `QueueState` `Flushed` olduğunda `EncryptedPayload` boşaltılmaz; denetim gereği korunur.
- 24 saatten eski `BackgroundTelemetryEvent` kayıtları purge sırasında silinir.

## Derived Views

- **ComplianceAuditView**: `ConsentDecision` + son 5 `BackgroundTelemetryEvent` + `SessionLogConfiguration.CaptureWhenInactive` durumu.
- **TelemetryExportEnvelope**: Aynı purge döngüsünde kuyruklanan tüm `BackgroundTelemetryEvent` kayıtları tek JSON paketine çevrilir.
