# Tasks: Arka Plan Rızası ve Sürekli Kayıt Modu

**Input**: Design documents from `/specs/003-spec-003-arka/`
**Prerequisites**: plan.md, /home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/research.md, data-model.md, contracts/

## Phase 3.1: Setup & Research
- [X] T001 Align project dependencies in `src/SafeKeyRecorder/SafeKeyRecorder.csproj` for tray icon and background capture libraries; ensure existing Spec 002 exporter package references remain compatible.
- [X] T002 Document global background capture strategy (R0.1) in `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/research.md`, including chosen hook mechanism, fallback, ve izin akışı.
- [X] T003 Kapsamlı tray/banner bileşen yaklaşımını (R0.2) `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/research.md` içinde örnek kod ve erişilebilirlik notlarıyla yakala.
- [X] T004 Araştırma: Linux ekran kilidi tespiti (DBus/loginctl) ve `backgroundResume` olay akışını `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/research.md` içinde diyagramla (R0.3).
- [X] T005 Şifreli telemetri kuyruğu entegrasyonunu (R0.4) `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/research.md` içine JSON şeması ve anahtar yönetimiyle ekle.
- [X] T006 Banner toggle erişilebilirlik gereksinimlerini (R0.5) `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/research.md` içinde WCAG kontrol listesi olarak tamamla.
- [X] T009 `contracts/telemetry-background.json` dosyasında şifreli telemetri payload sözleşmesini tanımla.
- [X] T010 `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/quickstart.md` dokümanını arka plan modu etkinleştirme, banner/toggle kontrolü, kilit testi ve purge sonrası doğrulama adımlarıyla genişlet.

## Phase 3.2: Tests First (TDD)
- [X] T011 [P] Yeni dosya `tests/SafeKeyRecorder.Tests/Unit/BackgroundCaptureServiceTests.cs` içinde kilit/odak davranışlarını doğrulayan failing ünite testi yaz.
- [X] T012 [P] `tests/SafeKeyRecorder.Tests/Unit/TelemetryExporterTests.cs` içinde şifreli dışa aktarım kuyruklama senaryosunu kapsayan failing test hazırla.
- [X] T013 [P] `tests/SafeKeyRecorder.Tests/Integration/BackgroundConsentFlowTests.cs` içinde arka plan rıza akışını kapsayan failing entegrasyon testi oluştur.
- [X] T014 [P] `tests/SafeKeyRecorder.Tests/Integration/BackgroundResumeTelemetryTests.cs` içinde kilitten dönüşte telemetri kaydını doğrulayan failing entegrasyon testi yaz.
- [X] T015 [P] `tests/SafeKeyRecorder.Tests/UiAutomation/BackgroundBannerToggleTests.cs` içinde banner toggle’ın UI davranışını test eden failing Playwright testi ekle.
- [X] T018 Arka plan rıza koordinatörünü (`BackgroundConsentCoordinator`) uygula; telemetri, banner ve denetim günlükleri entegrasyonunu sağla.
- [X] T019 Arka plan resume koordinatörünü (`BackgroundResumeCoordinator`) uygula; kilit olaylarıyla telemetri ve capture servisini koordine et.
- [ ] T027 `src/SafeKeyRecorder/Services/AccessibilityService.cs` ve banner bileşeninde ekran okuyucu etiketlerini uygula.

## Phase 3.4: Integration & Polish
- [ ] T028 Spec 002 güvenli dışa aktarım akışında (örn. `src/SafeKeyRecorder/Telemetry/BackgroundTelemetryExporter.cs` → Spec 002 webhook servisi) arka plan payload’larının kuyruğa alınmasını ve purge tetiklerini bağla.
- [ ] T029 `tests/SafeKeyRecorder.Tests/*` altındaki yeni testleri çalıştır; failing durumlarını doğrulayıp gerekli düzeltmeleri tamamlandıktan sonra hepsinin PASS olduğunu belgeleyen sonuç raporu ekle.
- [ ] T031 Proje kökünde `README.md` veya ilgili belgelere arka plan modu, uyarı gereksinimleri ve etik kılavuz notlarını ekle.

## Dependencies
- **T001** tamamlanmadan diğer görevler başlatılmamalıdır.
- **T002-T006** araştırma çıktıları, **T007-T010** dokümantasyon görevlerinin ön koşuludur.
- **T011-T015** testleri PASS olmadan Phase 3.3 uygulamalarına başlanmamalıdır.
- **T016-T027** sırası veri modeli → servisler → UI → telemetri olarak izlenmelidir; aynı dosyayı etkileyen görevler (örn. T022, T023) ardışık yürütülmelidir.
- **T028** telemetri entegrasyonu, T024-T027 tamamlanana kadar başlatılamaz.
- **T029** tüm uygulama görevlerinin ardından çalıştırılmalıdır; başarısız testler için düzeltici aksiyon gerektirir.
- **T030-T031** nihai doğrulama ve dokümantasyon aşamasıdır; diğer tüm görevler tamamlandıktan sonra yürütülür.

## Parallel Execution Example
```
# Research ve test hazırlıkları tamamlandıktan sonra şu görevler paralel yürütülebilir:
Task: "T011 [P] Yeni dosya tests/SafeKeyRecorder.Tests/Unit/BackgroundCaptureServiceTests.cs ..."
Task: "T012 [P] tests/SafeKeyRecorder.Tests/Unit/TelemetryExporterTests.cs ..."
Task: "T015 [P] tests/SafeKeyRecorder.Tests/UiAutomation/BackgroundBannerToggleTests.cs ..."
Task: "T021 src/SafeKeyRecorder/Views/Components/BackgroundStatusBanner.axaml(.cs) bileşenini oluştur"
```
