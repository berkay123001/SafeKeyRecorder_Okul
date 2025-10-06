# Quickstart: SafeKeyRecorder Avalonia Consent-Driven Simulation

## Prerequisites
- .NET SDK 8.0
- Avalonia .NET templates (`dotnet new install Avalonia.Templates`)
- Playwright CLI (`npx playwright install`) for UI otomasyon testleri

## Setup Steps
1. Repository kökünde bağımlılıkları geri yükleyin:
   ```bash
   dotnet restore
   ```
2. Playwright test ortamını hazırlayın:
   ```bash
   dotnet test tests/SafeKeyRecorder.Tests/SafeKeyRecorder.Tests.csproj -- ListFullyQualifiedTests
   npx playwright install
   ```
3. Uygulamayı geliştirme modunda çalıştırın:
   ```bash
   dotnet run --project src/SafeKeyRecorder/SafeKeyRecorder.csproj
   ```
4. Açılışta görünen rıza diyaloğunda:
   - Mesajı inceleyin.
   - Dosya kaydını etkinleştirmek için onay kutusunu işaretleyin (isteğe bağlı).
   - "Devam" butonuna basarak oturumu başlatın.
5. Uygulama penceresinde `LogArea` aktif olduğunda klavye girişlerini yapın:
   - UI'nin ≤50 ms gecikme ile güncellendiğini gözlemleyin.
   - Konsol veya GUI bildiriminin log dosyası yolunu gösterdiğini doğrulayın.
6. Oturumu sonlandırın:
   - Kapanış diyaloğunda "Oturum logunu sil" seçeneğini test edin.
   - Logu tutmayı seçerseniz 24 saat otomatik silme uyarısını gözlemleyin.

## Test Suite Çalıştırma
1. Birim ve entegrasyon testleri:
   ```bash
   dotnet test tests/SafeKeyRecorder.Tests/SafeKeyRecorder.Tests.csproj
   ```
2. Playwright tabanlı gecikme ve şeffaflık testleri:
   ```bash
   dotnet test tests/SafeKeyRecorder.Tests/SafeKeyRecorder.Tests.csproj --filter TestCategory=UiAutomation
   ```

## Manuel Doğrulama Checklist
- Rıza mesajı net ve iki seçenekli mi?
- Log dosyası yolu GUI üzerinde açıkça gösteriliyor mu?
- Log dosyası oluşturulduğunda `ISO8601, karakter` formatını koruyor mu?
- "Logu tut" seçildiğinde 24 saat sonra otomatik silme planlanıyor mu (uyarı mesajını gözlemleyin)?
- Erişilebilirlik modunda yüksek kontrast ve ekran okuyucu etiketleri etkin mi?
