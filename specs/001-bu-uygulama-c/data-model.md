# Data Model: SafeKeyRecorder Avalonia Consent-Driven Simulation

## Entities

### ConsentSession
- **Purpose**: Tutulan tek oturum boyunca rıza ve kullanıcı tercihlerini izlemek.
- **Fields**:
  - `SessionId` (GUID) — benzersiz oturum kimliği.
  - `ConsentGrantedAt` (DateTimeOffset) — onayın zaman damgası.
  - `LoggingEnabled` (bool) — yerel dosya kaydının açık/kapalı bilgisi.
  - `LogFilePath` (string?) — etkinse `~/SafeKeyRecorder/session_log.txt`.
  - `AutoDeleteRequested` (bool) — kapanışta silme kutusu seçimi.
  - `RetentionExpiresAt` (DateTimeOffset?) — 24 saatlik otomatik silme zamanı.
  - `AccessibilityMode` (enum) — `Default`, `HighContrast`, `ScreenReader`, `Both`.

### SessionLogEntry
- **Purpose**: Her tuş vuruşunu, zaman damgasını ve erişilebilirlik bağlamını saklamak.
- **Fields**:
  - `SessionId` (GUID) — `ConsentSession` ile ilişki.
  - `RecordedAt` (DateTimeOffset) — ISO-8601 zaman damgası.
  - `KeySymbol` (string) — gösterilecek karakter veya özel tuş etiketi.
  - `IsPrintable` (bool) — görüntüleme filtreleri için.
  - `Modifiers` (string[]) — `Shift`, `Ctrl` vb.
  - `WasLoggedToFile` (bool) — dosya kaydının başarılı olup olmadığı.

### AccessibilityPreference
- **Purpose**: Kullanıcıya özel erişilebilirlik ayarlarının GUI üzerinde uygulanması.
- **Fields**:
  - `HighContrastEnabled` (bool)
  - `ScreenReaderEnabled` (bool)
  - `UpdatedAt` (DateTimeOffset)

## Relationships
- `ConsentSession 1..1` ↔ `AccessibilityPreference 1..1` — aynı oturum için erişilebilirlik tercihleri.
- `ConsentSession 1..*` → `SessionLogEntry` — her oturum çok sayıda log girdisi üretir.

## Identity & Uniqueness
- `ConsentSession.SessionId` benzersiz kimlik.
- `SessionLogEntry` benzersizliği `(SessionId, RecordedAt, KeySymbol, Modifiers)` bileşimi ile sağlanır.

## State Transitions
- `ConsentSession`
  - `Initialized` → `AwaitingConsent`
  - `AwaitingConsent` → `Active` (kullanıcı "Devam"ı seçtiğinde)
  - `Active` → `Closing` (kullanıcı uygulamayı kapatırken)
  - `Closing` → `Terminated` (log dosyası silinir veya saklanır, RAM temizlenir)
- `SessionLogEntry`
  - `CapturedInMemory` → `PersistedToFile` (logging açıksa)
  - `PersistedToFile` → `Deleted` (24 saat sonrası veya kapanışta silme)

## Data Volume & Retention
- Beklenen maksimum 10.000 tuş vuruşu (~100 KB düz metin).
- Otomatik silme 24 saat içinde tetiklenir; elle silme seçeneği kapanışta.

## Validation Rules
- `ConsentGrantedAt` set edilmeden `Active` durumuna geçilemez.
- `RetentionExpiresAt` sadece `LoggingEnabled = true` olduğunda dolu olmalıdır.
- `KeySymbol` boş bırakılamaz; kontrol dışı tuşlar için tanımlayıcı etiket gerekir (örn. `[Enter]`).
- `AccessibilityMode` en az bir erişilebilirlik seçeneği aktifleştirilmişse `AccessibilityPreference` ile tutarlı olmalıdır.

## Open Questions
- **None** — `/clarify` oturumunda çözüldü.
