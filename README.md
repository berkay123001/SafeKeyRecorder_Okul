# Etik Eğitim Sürümü

> Bu proje yalnızca eğitim ve ödev amaçlıdır.  
> Kullanıcı açık rızası olmadan hiçbir kayıt yapılmaz.  
> Arka plan modu yalnızca bilgilendirilmiş onay ile çalışır.  
> Kötüye kullanım veya gizli izleme geliştiricinin sorumluluğunda değildir.

# SafeKeyRecorder

SafeKeyRecorder, Avalonia UI ve MVVM mimarisiyle geliştirilmiş, kullanıcının açık rızasına dayanan bir tuş yakalama ve telemetri uygulamasıdır. Amaç; erişilebilirlik araştırmaları, uyumluluk denetimleri ve kullanıcıların bilgilendirilmiş onay verdiği senaryolarda arka plan tuş izleme ve kayıt süreçlerini güvenli, şeffaf ve etik biçimde yönetmektir.

## Özellikler

- **Rıza odaklı akış**: Herhangi bir kayıt öncesinde kullanıcıya detaylı bir rıza diyaloğu sunulur ve seçimler uyum amaçlı kaydedilir.
- **Ön/arka plan kaydı**: Odakta iken `KeyCaptureService`, arka planda ise SharpHook tabanlı `GlobalHookAdapter` ile veri toplanır; arka plan modu yalnızca rıza sonrası etkinleşir.
- **Telemetri hattı**: `BackgroundTelemetryExporter`, `BackgroundTelemetryEncryptor` ve `SecureTelemetryQueue` birlikte çalışarak şifrelenmiş olay paketlerini güvenle sıraya alır.
- **Sistem farkındalığı**: `SystemLockMonitor`, oturum kilitlendiğinde yakalamayı durdurup, kilit açıldığında otomatik devamı koordine eder.
- **Durum bildirimleri**: `BackgroundStatusBannerViewModel` ve `BackgroundStatusTrayIcon` kullanıcıya anlık durum bilgisi verir, arka plan modunu hızlıca açıp kapamayı sağlar.
- **Erişilebilirlik desteği**: `AccessibilityService`, kullanıcı tercihleri doğrultusunda arayüz bildirimlerini uyarlayarak deneyimi iyileştirir.
- **Webhook ve log yönetimi**: Ana pencerede varsayılan bir webhook.site örnek adresi önceden doldurulur; kullanıcı gerekirse "+ Log Konumunu Değiştir" butonuyla kayıt dosyasının dizinini güncelleyebilir.

## Mimari Genel Bakış

```
SafeKeyRecorder/
├─ src/SafeKeyRecorder/
│  ├─ Background/                # Rıza ve devam koordinatörleri
│  ├─ Services/                  # Yakalama servisleri, hook adaptörü, kilit izleyici
│  ├─ Telemetry/                 # Telemetri soyutlamaları ve uygulamaları
│  ├─ ViewModels/                # MainWindowViewModel ve banner VM’leri
│  ├─ Views/                     # Avalonia görünümleri ve diyalogları
│  └─ Tray/                      # Sistem tepsisi entegrasyonu
└─ tests/SafeKeyRecorder.Tests/  # Telemetri hattı için birim testleri
```

Uygulama MVVM prensiplerini izler. `MainWindowViewModel`, rıza akışı, kayıt durumu ve günlük yönetimini orkestre ederken `MainWindow.axaml` ise Avalonia arayüz kabuğunu sağlar.

## Önkoşullar

- .NET 8 SDK veya daha yeni sürüm
- `dotnet restore` ile indirilebilen Avalonia bağımlılıkları
- NuGet üzerinden sağlanan SharpHook (libuiohook) yerel kütüphaneleri
- Kilit izleme için DBus erişimi olan bir Linux masaüstü ortamı

## Hızlı Başlangıç

```bash
dotnet restore
dotnet build src/SafeKeyRecorder/SafeKeyRecorder.csproj
dotnet run --project src/SafeKeyRecorder/SafeKeyRecorder.csproj
```

İlk çalıştırmada rıza diyaloğu otomatik açılır. Kullanıcı onaylamadığı sürece arka plan kaydı devreye alınmaz.

1. Rızayı kabul ederken “Sunucuya logları gönderme izni ver” kutusunu işaretle.
2. Ana pencerede `Webhook adresi` alanında örnek (`https://webhook.site/#!/v/example-endpoint`) URL’si yer alır; kendi uç noktanla değiştir.
3. `Log Konumunu Değiştir` butonuyla oturum logunu kaydetmek istediğin dizini seçebilir, ardından `Sunucuya Gönder` ile içeriği webhook.site üzerinde doğrulayabilirsin.

## Testler

```bash
dotnet test tests/SafeKeyRecorder.Tests/SafeKeyRecorder.Tests.csproj
```

Mevcut testler ağırlıklı olarak telemetri ihracatçısını doğrular. Servisler ve koordinatörler için kapsamın genişletilmesi planlanmaktadır.

## Güvenlik ve Etik İlkeler

- **Açık rıza şartı**: Kullanıcı onayı olmadan arka plan kaydı asla başlatılmaz ve kullanıcı istediği an devre dışı bırakabilir.
- **Şeffaflık**: Arayüzdeki banner ve tray bileşenleri yakalamanın açık/kapalı durumunu sürekli gösterir.
- **Veri koruma**: Telemetri verileri diske yazılmadan önce `BackgroundTelemetryEncryptor` tarafından şifrelenir.
- **Sorumlu kullanım**: SafeKeyRecorder; erişilebilirlik desteği, olay sonrası denetim veya eğitim gibi bilgilendirilmiş kullanıcı onayına dayanan amaçlar içindir. İzinsiz izleme, zararlı keylogger senaryoları veya yasa dışı faaliyetlerde kullanılması **kesinlikle yasaktır**. Yerel yasalara ve kurum politikalarına daima uyun.

## Yol Haritası

- **Hata düzeltmeleri**: Bilinen arayüz sorunları ve hook/kilit servisleri etrafındaki hata yakalama iyileştirilecek.
- **Webhook dışa aktarımı**: Kuyruktaki telemetri paketlerinin kimlik doğrulamalı uç noktalara gönderimi planlanıyor.
- **Çapraz platform iyileştirmeleri**: Wayland, macOS ve Windows ortamlarındaki kullanıcı deneyimi geliştirilecek.

## Lisans ve Atıf

Proje henüz yayımlanmamıştır. Dağıtımdan önce kurumsal gereksinimler ve etik taahhütlerle uyumlu uygun bir lisans (ör. MIT, Apache 2.0) seçilmelidir.

## Katkıda Bulunma

1. Depoyu forklayın.
2. Yeni bir dal açın: `git checkout -b feature/yeni-ozellik`.
3. Anlamlı commit mesajlarıyla değişiklikleri kaydedin: `git commit -am "Özelliği açıkla"`.
4. Yaptığınız değişiklikleri ve testleri özetleyen bir pull request oluşturun.

Yeni özellikler geliştirirken rıza odaklı modeli ve kullanıcıya yönelik bilgilendirici arayüz mesajlarını korumaya özen gösterin.
