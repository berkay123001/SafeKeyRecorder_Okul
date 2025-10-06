
# Implementation Plan: SafeKeyRecorder Avalonia Consent-Driven Simulation

**Branch**: `001-bu-uygulama-c` | **Date**: 2025-09-30 | **Spec**: `/home/berkayhsrt/SafeKeyRecorder/specs/001-bu-uygulama-c/spec.md`
**Input**: Feature specification from `/specs/001-bu-uygulama-c/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
AvaloniaUI ve .NET 8 tabanlı etik keylogger simülasyonu; kullanıcı rızası olmadan kayıt yok, tuş vuruşları gerçek zamanlı GUI'de gösteriliyor ve isteğe bağlı olarak `~/SafeKeyRecorder/session_log.txt` dosyasına yazılıyor. Oturum sonu otomatik silme veya kullanıcı tarafından saklama (24 saat limitli) seçenekleri, ≤50 ms gecikme hedefi ve erişilebilirlik için yüksek kontrast + ekran okuyucu desteği sağlanacak.

## Technical Context
**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**: AvaloniaUI 11.x, ReactiveUI, Avalonia.Diagnostics (dev), System.Text.Json  
**Storage**: Yerel dosya (`~/SafeKeyRecorder/session_log.txt`), RAM buffer  
**Testing**: xUnit, Avalonia headless UI test harness, Playwright UI otomasyonu  
**Target Platform**: Linux masaüstü (X11/Wayland)  
**Project Type**: Tek masaüstü uygulaması (MVVM)  
**Performance Goals**: Tuş → GUI ≤50 ms; pencere açılışı ≤2 s  
**Constraints**: Arka plan çalışması yasak, ağ erişimi kapalı, yalnızca ön plan odağı  
**Scale/Scope**: Tek kullanıcı, eğitim amaçlı <10k tuş vuruşu

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Kod Kalitesi: PASS – xUnit + Playwright testleri ve .NET analyzers entegrasyonu planlandı.
- Şeffaflık ve Bilgilendirme: PASS – açılış rıza diyaloğu, GUI durum banner'ı ve log yolu mesajı tasarlandı.
- Veri Gizliliği ve Rıza: PASS – varsayılan kayıt kapalı, 24 saat otomatik silme prosedürü tanımlandı.
- Sorumluluk ve Hukuki Uyum: PASS – KVKK/GDPR için yalnızca yerel depolama, ağ bağlantısı yok, denetim logları oluşturulacak.
- Simülasyon Sınırları: PASS – yalnızca ön plan fokus, sandbox testleri ve ağ izolasyonu uygulanacak.

## Project Structure

### Documentation (this feature)
```
specs/001-bu-uygulama-c/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── consent.json
│   └── logging.json
└── tasks.md (plan aşamasında oluşturulmaz)
```

### Source Code (repository root)
```
src/
└── SafeKeyRecorder/
    ├── SafeKeyRecorder.csproj
    ├── App.axaml
    ├── App.xaml.cs
    ├── Views/
    │   ├── MainWindow.axaml
    │   ├── MainWindow.axaml.cs
    │   └── ConsentDialog.axaml(.cs)
    ├── ViewModels/
    │   ├── MainWindowViewModel.cs
    │   ├── ConsentDialogViewModel.cs
    │   └── AccessibilitySettingsViewModel.cs
    ├── Services/
    │   ├── KeyCaptureService.cs
    │   ├── SessionLogService.cs
    │   └── AccessibilityService.cs
    ├── Models/
    │   ├── ConsentSession.cs
    │   └── SessionLogEntry.cs
    ├── Telemetry/
    │   └── ComplianceAuditLogger.cs
    └── Resources/
        └── Styles.axaml

tests/
└── SafeKeyRecorder.Tests/
    ├── SafeKeyRecorder.Tests.csproj
    ├── Integration/
    │   ├── ConsentFlowTests.cs
    │   ├── TransparencyIndicatorTests.cs
    │   └── AutoDeleteRetentionTests.cs
    ├── Accessibility/
    │   └── HighContrastAndScreenReaderTests.cs
    ├── Unit/
    │   ├── SessionLogServiceTests.cs
    │   └── KeyCaptureServiceTests.cs
    └── UiAutomation/
        └── KeyLatencyPlaywrightTests.cs
```

**Structure Decision**: Tek Avalonia masaüstü uygulaması; MVVM katmanları ve servisler `src/SafeKeyRecorder/` altında, otomasyon ve birim testleri `tests/SafeKeyRecorder.Tests/` içinde konumlanacak.

## Phase 0: Outline & Research
- **R0.1 (rev. 2025-10-04)**  
  Avalonia key input yakalama stratejilerini karşılaştır (TextInput vs global hook) → gizlilik ve etik uyumu.  
  Arka plan veya global hook tabanlı yakalama **varsayılan olarak devre dışıdır.**  
  Ancak kullanıcı **açık, bilgilendirilmiş onay** verirse ve uygulama **kesintisiz görsel uyarı** gösteriyorsa,  
  bu mod **etik olarak etkinleşebilir.**  
  Ekran kilitlendiğinde kayıt otomatik durur; kilit açıldığında telemetriye `backgroundResume` olayı kaydedilir.  
  Bu ilke SafeKeyRecorder Anayasası v2.0.0 (Şeffaflık ve Simülasyon Sınırları) ile uyumludur.

- **R0.2** ReactiveUI ile ≤50 ms güncelleme hedefini doğrulamak için performans araştırması ve profiling planı çıkar.
- **R0.3** Yerel dosya otomatik silme için .NET `FileSystem` API kullanımını ve 24 saat zamanlayıcı stratejisini araştır.
- **R0.4** Avalonia yüksek kontrast ve ekran okuyucu desteği (AutomationProperties) için en iyi pratikleri derle.
- **R0.5** Playwright + Avalonia test runner entegrasyonu ve latency ölçümü yöntemini belirle.

**Output**: `/specs/001-bu-uygulama-c/research.md` (tamamlandı)

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

- **D1.1** `data-model.md` içinde `ConsentSession`, `SessionLogEntry`, `AccessibilityPreference` alanlarını ve geçişlerini tanımla (tamamlandı).
- **D1.2** `contracts/consent.json` ile rıza diyaloğu durumlarını ve sonuçlarını dokümante et (tamamlandı).
- **D1.3** `contracts/logging.json` ile dosya yazımı, otomatik silme ve kullanıcı bildirimi sözleşmesini belirt (tamamlandı).
- **D1.4** Quickstart rehberinde rıza akışı, dosya yolu bildirimi ve otomatik silme doğrulamalarını adım adım açıkla (tamamlandı).
- **D1.5** Test stratejisi: ReactiveUI view-model unit testleri + Playwright latency testleri için kategori planı çıkar (`tests/SafeKeyRecorder.Tests/`).

**Output**: `/specs/001-bu-uygulama-c/data-model.md`, `/contracts/*.json`, `/quickstart.md` (tamamlandı)

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Phase 0/1 artefactlarından türet:
  - `contracts/consent.json` → consent flow UI + integration test görevleri
  - `contracts/logging.json` → oturum log servisi + purge komut görevleri
  - `data-model.md` → modeller, servisler, view-model senkronizasyonu
  - `quickstart.md` → manuel doğrulama ve otomasyon checklist görevleri
- Ethics & Privacy kategorisi için consent banner, dosya yolu bildirimi, otomatik silme testleri ekle
- Playwright latency testi ve yüksek kontrast regression testleri için bağımsız [P] görevleri ekle

**Ordering Strategy**:
- TDD: Önce unit + integration testler (ConsentFlowTests, TransparencyIndicatorTests, KeyLatencyPlaywrightTests)
- Servisler (`SessionLogService`, `KeyCaptureService`) tamamlanmadan UI komutları uygulanmaz
- Accessibility ve Ethics görevleri release öncesi zorunlu gate; ilgili testler PASS olmadan uygulama paketlenmez

**Estimated Output**: 26-30 maddelik sıraya alınmış görev seti (tests, services, UI, erişilebilirlik, purge otomasyonu)

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
Hiçbir anayasa sapması planlanmadı; tabloya kayıt gerekmez.


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [ ] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on SafeKeyRecorder Constitution v1.0.0 - See `/memory/constitution.md`*
