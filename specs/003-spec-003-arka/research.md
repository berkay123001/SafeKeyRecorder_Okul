# Research Log – Arka Plan Rızası ve Sürekli Kayıt Modu

## R0.1 Küresel Tuş Yakalama Stratejisi

- **Birincil araç**: `SharpHook` (libuiohook bağlaması) – .NET 8 uyumlu, X11 ve Wayland desteği. `IReactiveGlobalHook` ile Avalonia iş parçacığına güvenli aktarım sağlanıyor. 
- **Platform davranışı**:
  - **X11**: libuiohook doğrudan global event stream sağlar; uygulama odak dışı iken keycode + modifier yakalanır.
  - **Wayland**: Yerel global hook kısıtlı; Flatpak/portal gerektirir. Fallback olarak `org.freedesktop.portal.ScreenCast` üzerinden tuş/klavye olayları alınır.
- **Fallback planı**: Eğer portal erişimi reddedilirse, kullanıcıya foreground-only moda dönmesi için banner’da uyarı gösterilir; `AllowBackgroundCapture=false` olarak telemetriye işlenir.
- **İzin işlemi**: Rıza diyaloğunda `AllowBackgroundCapture` işaretlenirse `BackgroundCaptureService` `SharpHook` için `Task.Run` ile hook başlatır, aksi durumda foreground yakalama (`KeyCaptureService`) kullanılır.
- **Kilit senaryosu**: Global yakalama etkin olsa bile `SystemLockMonitor` `Suspend` sinyali gönderdiğinde hook durdurulur.

## R0.2 Tray ve Banner Bileşen Yaklaşımı

- **Tray**: `TrayIcon.Avalonia` paketi ile kalıcı durum ikonu. Durum değişimlerinde `TrayIcon.IconPath` sarı/yeşil svg dosyalarıyla güncellenecek.
- **Banner**: `MainWindow` başlık alanında `Border` + `StackPanel` kombinasyonu; sarı (#F9C74F) arka plan, siyah metin ve `ToggleSwitch` içerir. ReactiveUI binding ile `IsBackgroundCaptureEnabled` alanına bağlanır.
- **Örnek XAML**:
```xml
<Border Classes="BackgroundCaptureBanner" Background="{Binding BannerBackground}">
  <DockPanel>
    <TextBlock Text="Arka plan kayıt modu aktif"/>
    <ToggleSwitch IsChecked="{Binding ToggleState}" Margin="8,0,0,0"/>
  </DockPanel>
</Border>
```
- **Erişilebilirlik**: `ToggleSwitch` için `AutomationProperties.Name="Arka plan modu"`, tray ikonu için `ToolTip.Tip="Arka plan kayıt modu aktif"`.
- **Performans**: Banner değişimleri Reactive command ile throttled (`Throttle(TimeSpan.FromMilliseconds(50))`).

## R0.3 Kilit/Unlock Tespiti ve backgroundResume Olay Akışı

- **DBus**: `Tmds.DBus` ile `org.freedesktop.login1.Manager` aboneliği. `PrepareForSleep(bool)` sinyali false → resume, true → suspend.
- **loginctl fallback**: DBus erişimi yoksa `loginctl show-session $XDG_SESSION_ID -p LockedHint` periyodik kontrol (2s interval).
- **Olay akışı**:
  1. `SystemLockMonitor` `Suspend` aldığında `BackgroundCaptureService.StopHook()` çağrılır, telemetriye `backgroundPause` yazar.
  2. `Resume` aldığında kullanıcı rızası sürüyorsa yeniden `StartHook()` ve telemetriye `backgroundResume` olayı eklenir.
  3. Banner metni "Kilitli" → "Aktif" dönüşlerini tetikler.
- **Test notu**: `BackgroundResumeTelemetryTests` simüle edilmiş lock/unlock olay dizisini doğrular.

## R0.4 Şifreli Telemetri Kuyruğu

- **Şema**: `telemetry-background.json` payload’ı `<Envelope>` içinde `mode`, `events[]`, `consentDecisionId`, `encryptedPayload` alanları barındırır.
- **Entegrasyon**: Spec 002 `SecureTelemetryExporter` queue’sine ek payload tipi `BackgroundResumeEvent`. Kuyruk yazımı `BackgroundTelemetryExporter` tarafından yapılır.
- **Şifreleme**: AES-256-GCM, anahtar materyali Spec 002 `KeyRing` servisi aracılığıyla alınır. Nonce = `resumeEventId` ilk 12 baytı.
- **Trigger**: Purge veya oturum kapanışında `FlushBackgroundEventsAsync()` tetiklenir; queue’ye `mode="background"` olarak işaretlenmiş paketler eklenir.
- **Güvenlik**: Banner toggle kapandığında bekleyen paketler `Invalidate()` ile drop edilir.

## R0.5 Banner Toggle Erişilebilirliği

- **WCAG 2.1**: Kontrast > 4.5:1 (Sarı arkaplan #F9C74F + siyah metin #111). Toggle minimum hedef 44x44 px.
- **Klavye navigasyonu**: `ToggleSwitch` `Space`/`Enter` ile çalışmalı; `IsTabStop=true`.
- **Ekran okuyucu**: `AutomationProperties.HelpText="Arka plan modunu açıp kapatır. Açıkken sistem tepsisinde uyarı gösterilir."`
- **Hata durumları**: Fallback ihtiyacı olduğunda `AutomationProperties.LiveSetting="Assertive"` ile "Arka plan yakalama devre dışı" metni duyurulur.
- **Dokümantasyon**: Quickstart’a erişilebilirlik adımları eklenecek (T010).
