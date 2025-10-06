# Quickstart – Arka Plan Rızası ve Sürekli Kayıt Modu

## Ön Koşullar
- SafeKeyRecorder build edilmiş olmalı (`dotnet build`).
- `TrayIcon.Avalonia`, `SharpHook`, `Tmds.DBus` paketleri restore edilmiş olmalı.
- Linux masaüstünde X11 veya Wayland oturumu açık; DBus erişimi mevcut.

## Senaryo 1 – Arka Plan Modunu Etkinleştirme
1. Uygulamayı `dotnet run --project src/SafeKeyRecorder/SafeKeyRecorder.csproj` ile başlatın.
2. Rıza diyaloğu açıldığında `Arka planda da kayıt yapmama izin ver` kutusunu işaretleyin.
3. Alt metni okuyun ve “Devam”a basın.
   - Beklenen: `MainWindow` üst banner sarıya döner, tray ikonu sarı uyarı simgesine geçer.
   - Telemetri kuyruğunda `mode="background"` etiketiyle `backgroundConsentRecorded` olayı oluşur.
4. Uygulamayı simge durumuna küçültün; başka uygulamada yazı yazın.
   - Beklenen: Log satırları `BackgroundCapture=True` olarak işaretlenir; banner/tray uyarısı kaybolmaz.

## Senaryo 2 – Kilit Açma Sonrası Devam
1. Senaryo 1 tamamlandıktan sonra sistemi kilitleyin (`Super+L`).
   - Beklenen: `BackgroundCaptureService` durur, banner “Kilitli” mesajını gösterir.
2. 5 saniye sonra oturumu açın.
   - Beklenen: `backgroundResume` olayı telemetri kuyruğuna eklenir; banner tekrar sarı “aktif” durumuna döner.
3. `logs/session_log.txt` dosyasında kilit süresince yeni giriş olmadığını doğrulayın.

## Senaryo 3 – Manual Purge ve Telemetri Flush
1. Ana pencereden purge komutunu çalıştırın (UI veya CLI kısayolu).
2. Beklenen:
   - `BackgroundTelemetryExporter` flush tetikler, kuyruktaki paketler AES-256-GCM ile şifrelenip Spec 002 queue’sine yazılır.
   - Banner “Kayıt pasif” mesajına döner, tray ikonu yeşile geçer.
   - Telemetri logunda `backgroundModeEnabled=false` kaydı oluşur.

## Senaryo 4 – Toggle ile Foreground’a Dönüş
1. Ana pencere bannerındaki toggle’ı kapatın.
   - Beklenen: `AllowBackgroundCapture=false` durumuna düşer, global hook durdurulur.
2. `logs/session_log.txt` içerisinde yeni satırlarda `BackgroundCapture=False` işaretini doğrulayın.
3. Toggle’ı yeniden açtığınızda rıza diyaloğu olmadan doğrudan arka plan aktif hale gelmeli (aynı oturum içindeyken).

## Erişilebilirlik Doğrulaması
- Banner toggle’ını klavye ile (Tab + Space) kontrol edin.
- Ekran okuyucu (örn. Orca) ile tray ikonunda yer alan “Arka plan kayıt modu aktif” geri bildirimini dinleyin.
- Wayland’da portal izni reddedildiğinde banner’da “Arka plan yakalama devre dışı” anonsunun yapıldığını doğrulayın.

## Kapanış Kontrolleri
- Uygulamayı kapatıp yeniden açın; arka plan modu varsayılan “kapalı” başlamalı.
- `specs/003-spec-003-arka/research.md` ve `data-model.md` ile tutarlı olacak şekilde yeni telemetri olaylarının işlendiğini gözden geçirin.
