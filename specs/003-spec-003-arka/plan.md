
# Implementation Plan: Arka Plan Rızası ve Sürekli Kayıt Modu

**Branch**: `003-spec-003-arka` | **Date**: 2025-10-04 | **Spec**: `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/spec.md`
**Input**: Feature specification from `/specs/003-spec-003-arka/spec.md`

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
Arka plan kayıt modu, SafeKeyRecorder’ın yalnızca odak içi yakalama sınırını kullanıcı rızası doğrultusunda genişletir. Yeni akışta rıza diyaloğu arka plan izni seçeneği sunar; kullanıcı kabul ettiğinde uygulama banner’ı sarıya döner, sistem tepsisinde kalıcı uyarı gösterilir ve `SessionLogService` odağa bakılmaksızın tuşları kaydeder. Cihaz kilitlenince kayıt otomatik durur, kilit açıldığında telemetriye `backgroundResume` olayı yazılarak süreç devam eder. Tüm arka plan oturumları purge/oturum kapanışı sırasında şifreli paket halinde, Spec 002’de tanımlanan güvenli dışa aktarım boru hattına gönderilir. Kullanıcı ana pencere bannerındaki toggle ile modu istediği an kapatabilir.

## Technical Context
**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**: Avalonia 11.3.6, ReactiveUI, existing TrayIcon helper (Avalonia.Desktop), Spec 002’nin güvenli dışa aktarım servisi  
**Storage**: Yerel oturum log dosyası (`~/SafeKeyRecorder/session_log.txt`), şifreli telemetri kuyruğu (Spec 002 ile paylaşılmış)  
**Testing**: xUnit, Avalonia headless UI test harness, Playwright tabanlı UI otomasyon  
**Target Platform**: Linux masaüstü (X11/Wayland)  
**Project Type**: Tek Avalonia masaüstü uygulaması  
**Performance Goals**: UI güncellemeleri ≤50 ms; arka plan telemetri kuyruklama işlemleri purge anında <200 ms overhead  
**Constraints**: Arka plan modu varsayılan kapalı; kesintisiz görsel uyarı zorunlu; cihaz kilitliyken kayıt durmalı; telemetri sadece purge/oturum kapanışında gönderilmeli; kişisel veri şifreli aktarılmalı  
**Scale/Scope**: Tek kullanıcı, eğitim amaçlı simülasyon; dakikada ≈300 keystroke, günlük <10k tuş

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Kod Kalitesi: Yeni servis, view-model ve banner/toggle değişiklikleri için ünite + entegrasyon testleri planlandı; telemetri kuyruğu için ek test seti oluşturulacak.
- Şeffaflık ve Bilgilendirme: Rıza diyaloğu arka plan seçeneği, ana pencere bannerı, sistem tepsisi ikonu ve telemetri olaylarıyla sürekli şeffaflık sağlanıyor.
- Veri Gizliliği ve Rıza: Mod varsayılan olarak kapalı; kullanıcı her oturumda açık rıza veriyor; log imha politikası 24 saat olarak korunuyor.
- Sorumluluk ve Hukuki Uyum: KVKK/GDPR gereği şifreli telemetri aktarımı ve kilitlenince durdurma mekanizması planlandı.
- Simülasyon Sınırları: Arka plan kayıt yalnızca yeni anayasa koşulları (kesintisiz görsel uyarı, kilit durdurma, telemetri şeffaflığı) sağlandığında etkin; gerçek sistemlere zarar verilmeyecek.

## Project Structure

### Documentation (this feature)
```
specs/003-spec-003-arka/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 findings & unknown resolution log
├── data-model.md        # Phase 1 entity & state definitions
├── quickstart.md        # Phase 1 kullanıcı doğrulama rehberi
└── contracts/
    ├── background-consent.json
    └── telemetry-background.json
```

### Source Code (repository root)
```
src/
└── SafeKeyRecorder/
    ├── App.axaml
    ├── App.axaml.cs
    ├── MainWindow.axaml
    ├── MainWindow.axaml.cs          # Banner toggle, tray ikonu yönetimi
    ├── Views/
    │   ├── ConsentDialog.axaml(.cs)  # Arka plan onay seçeneği
    │   └── Components/
    │       └── BackgroundStatusBanner.axaml(.cs)   # Yeni banner kontrolü
    ├── ViewModels/
    │   ├── MainWindowViewModel.cs   # Toggle komutları, telemetri tetikleri
    │   └── ConsentDialogViewModel.cs# AllowBackgroundCapture alanı
    ├── Services/
    │   ├── SessionLogService.cs     # CaptureWhenInactive & lock durdurma
    │   ├── KeyCaptureService.cs     # Foreground capture
    │   └── BackgroundCaptureService.cs # Yeni global capture adaptörü (platform-specific)
    ├── Models/
    │   ├── ConsentDecision.cs       # AllowBackground, ResumeHistory
    │   └── Telemetry/
    │       └── BackgroundTelemetryEvent.cs
    ├── Telemetry/
    │   ├── ComplianceAuditLogger.cs # mode=background kayıtları
    │   └── BackgroundTelemetryExporter.cs
    └── Tray/
        └── BackgroundStatusTrayIcon.cs

tests/
└── SafeKeyRecorder.Tests/
    ├── Unit/
    │   ├── BackgroundCaptureServiceTests.cs
    │   └── TelemetryExporterTests.cs
    ├── Integration/
    │   ├── BackgroundConsentFlowTests.cs
    │   └── BackgroundResumeTelemetryTests.cs
    └── UiAutomation/
        └── BackgroundBannerToggleTests.cs
```

**Structure Decision**: Tek Avalonia masaüstü uygulaması korunur; yeni banner, tray ve background capture bileşenleri mevcut `src/SafeKeyRecorder/` ağacına eklenir; ilgili testler `tests/SafeKeyRecorder.Tests/` altında genişletilir.

## Phase 0: Outline & Research
1. **R0.1 – Global key yakalama stratejisi (rev. 2025-10-04)**: Avalonia dışı global hook seçeneklerini (X11 `xinput`, evdev, Wayland portal) değerlendir; kullanıcı rızası sonrası etkinleşen, kilit durumunu algılayıp duraklatabilen en az bir platform bağımsız yaklaşım seç. Çıktı: avantaj/dezavantaj tablosu, seçilen yöntem, fallback planı.
2. **R0.2 – Sistem tepsisi ve başlık banner bileşenleri**: Avalonia’da tray ikonu ve başlık çubuğu rengini dinamik değiştirme best practice’lerini topla. Çıktı: örnek kod parçası, erişilebilirlik etiketleri.
3. **R0.3 – Kilit durumu tespiti**: Linux’ta ekran kilidi durumunu dinlemek için DBus/`loginctl` entegrasyonu araştır; kilit açıldığında `backgroundResume` tetikleyecek mekanizma planla. Çıktı: olay akışı diyagramı.
4. **R0.4 – Şifreli telemetri kuyruğu**: Spec 002 dışa aktarım pipeline’ı ile entegrasyon, anahtar yönetimi ve payload formatını netleştir. Çıktı: JSON şema, şifreleme akışı.
5. **R0.5 – Banner toggle erişilebilirliği**: Toggle bileşeni için WCAG uyumlu klavye/ekran okuyucu etiketleri ve dokunmatik hedef boyutu araştır. Çıktı: erişilebilirlik kontrol listesi.

**Output**: `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/research.md`

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Data Model güncellemesi** (`data-model.md`): `ConsentDecision` için `AllowBackgroundCapture`, `ResumeEvents`; `SessionLogService` konfigürasyonu için `CaptureWhenInactive`; `BackgroundTelemetryEvent` şeması. Durum diyagramı: Idle → ForegroundCapture → BackgroundCapture → Suspended (lock) → Resumed.
2. **Contract üretimi** (`contracts/`):
   - `background-consent.json`: Rıza diyaloğu çıktısı (bool `allowBackground`, timestamp, toggle state).
   - `telemetry-background.json`: `mode`, `resumeEvents[]`, `purgeSummary` alanlarını içeren şifreli payload zarfı.
3. **Test stratejisi**: `BackgroundCaptureServiceTests` için global hook mock’ları; `BackgroundBannerToggleTests` UI otomasyonu; `BackgroundResumeTelemetryTests` entegrasyonu.
4. **Quickstart** (`quickstart.md`): Kullanıcının modu etkinleştirme, banner/toggle kontrolü, kilit test senaryosu, purge sonrası telemetri doğrulaması adımları.
5. **Agent bağlamı**: Mevcut windsurf agent dosyası varsa, yeni kavramlar (background capture, tray icon, encrypted telemetry) ile güncellenecek.

**Output**: `/home/berkayhsrt/SafeKeyRecorder/specs/003-spec-003-arka/data-model.md`, `/contracts/background-consent.json`, `/contracts/telemetry-background.json`, `/quickstart.md`

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract → contract test task [P]
- Each entity → model creation task [P] 
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 18-22 maddelik görev seti; Rıza UI revizyonu, banner/toggle geliştirmesi, background capture servisi, telemetri kuyruğu ve ilgili testler.

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [ ] Phase 0: Research complete (/plan command)
- [ ] Phase 1: Design complete (/plan command)
- [ ] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [ ] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on SafeKeyRecorder Constitution v2.0.0 - See `/memory/constitution.md`*
