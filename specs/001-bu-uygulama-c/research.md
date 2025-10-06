# Phase 0 Research: SafeKeyRecorder Avalonia Consent-Driven Simulation

## 1. Avalonia Key Input Capture without Global Hooks
- **Decision**: Use `TextInput` and `KeyDown` events within focused Avalonia window; avoid OS-level global hooks.
- **Rationale**: Aligns with constitution requirement to forbid gizli arka plan kaydı; Avalonia destekli mekanizmalar platform bağımsız.
- **Alternatives Considered**:
  - **Win32 global hooks**: Reddedildi (platform bağımlı, gizlilik ihlali).
  - **X11 key grab**: Reddedildi (Linux spesifik, arka plan gizliliği ihlali).

## 2. Gerçek Zamanlı Görselleştirme Performansı
- **Decision**: ReactiveUI ile `ObservableCollection<string>` bağlayıp `ItemsControl`/`TextBox` üzerinde incremental güncelleme.
- **Rationale**: Reactive pipeline minimal gecikme, UI thread senkronizasyonu kolay; ≤50 ms hedefini destekler.
- **Alternatives Considered**:
  - Polling tabanlı güncelleme: Fazla gecikme, gereksiz CPU.
  - Custom drawing canvas: Overkill, maintanability düşük.

## 3. Log Dosyası Yönetimi ve Otomatik Silme
- **Decision**: `SessionLogService` ile `FileStream` append + `FileSystemWatcher` fallback; kapanışta ve 24 saat sonrası temizleme için `Timer` kullanımı.
- **Rationale**: Basit, platform bağımsız, kullanıcıya konum bildirimi kolay.
- **Alternatives Considered**:
  - SQLite/diğer depolar: Gereksiz karmaşıklık.
  - Tempfs sadece RAM: Kullanıcının logu inceleme isteği ile çelişiyor.

## 4. Erişilebilirlik (High Contrast & Screen Reader)
- **Decision**: Avalonia `RequestedThemeVariant=HighContrast` profilini desteklemek, UI kontrollerine `AutomationProperties.Name` atamak.
- **Rationale**: Yerleşik mekanizmalar, minimum ekstra kod, test edilebilir.
- **Alternatives Considered**:
  - Kendi theme sistemi: Ek yük, test maliyeti.

## 5. UI Otomasyon Testleri
- **Decision**: Playwright ile Avalonia webview runner yerine Avalonia `TestApplication` + Playwright driver (Avalonia 11 destekli) kullanmak.
- **Rationale**: Gerçekçi kullanıcı etkileşimi, latency ölçümü otomasyonu mümkün.
- **Alternatives Considered**:
  - Manuel UI testleri: Tekrarlanabilir değil.
  - Sadece unit test: UI performans ölçümü yapılamaz.
