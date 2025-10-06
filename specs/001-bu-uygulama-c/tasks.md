# Tasks: SafeKeyRecorder Avalonia Consent-Driven Simulation

**Input**: `/home/berkayhsrt/SafeKeyRecorder/specs/001-bu-uygulama-c/plan.md`
**Prerequisites**: research.md, data-model.md, contracts/, quickstart.md

## Execution Flow (main)
```
1. Load plan.md from feature directory
2. Load supporting documents (data-model.md, contracts/, research.md, quickstart.md)
3. Generate tasks grouped by category (Setup → Tests → Core → Integration → Polish)
4. Ensure TDD order (tests before implementation)
5. Mark [P] only when tasks touch independent files
6. Stop after producing ordered task list ready for execution
```

## Format: `[ID] [P?] Description`
- **[P]**: Task can run in parallel (different files / no dependency)

## Path Conventions
- Application code lives under `src/SafeKeyRecorder/`
- Tests live under `tests/SafeKeyRecorder.Tests/`
- Contracts stored in `specs/001-bu-uygulama-c/contracts/`

## Phase 3.1: Setup
- [x] T001 Create Avalonia application skeleton `src/SafeKeyRecorder/SafeKeyRecorder.csproj` with MVVM folders.
- [x] T002 Add `tests/SafeKeyRecorder.Tests/SafeKeyRecorder.Tests.csproj`, reference app project, enable xUnit + Avalonia headless runner.
- [x] T003 Configure `.editorconfig` and enable .NET analyzers + nullable across solution.
- [x] T004 Provision Playwright tooling (add `Microsoft.Playwright` reference, run `npx playwright install`) and document in `Directory.Build.props`.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
- [x] T005 [P] Add consent contract tests in `tests/SafeKeyRecorder.Tests/Contracts/ConsentContractTests.cs` validating `contracts/consent.json`.
- [x] T006 [P] Add logging contract tests in `tests/SafeKeyRecorder.Tests/Contracts/LoggingContractTests.cs` validating `contracts/logging.json`.
- [x] T007 [P] Create integration test `tests/SafeKeyRecorder.Tests/Integration/ConsentFlowTests.cs` covering rıza diyaloğu ve logging toggles.
- [x] T008 [P] Create integration test `tests/SafeKeyRecorder.Tests/Integration/TransparencyIndicatorTests.cs` ensuring GUI log yolu mesajı ve durum banner'ı.
- [x] T009 [P] Create integration test `tests/SafeKeyRecorder.Tests/Integration/AutoDeleteRetentionTests.cs` verifying 24 saat silme ve kapanış purge.
- [x] T010 [P] Create accessibility regression tests `tests/SafeKeyRecorder.Tests/Accessibility/HighContrastAndScreenReaderTests.cs` for theme ve `AutomationProperties`.
- [x] T011 [P] Create unit tests `tests/SafeKeyRecorder.Tests/Unit/SessionLogServiceTests.cs` covering append/purge behaviour.
- [x] T012 [P] Create unit tests `tests/SafeKeyRecorder.Tests/Unit/KeyCaptureServiceTests.cs` simulating key events ve modifier kayıtları.
- [x] T013 [P] Create UI automation latency test `tests/SafeKeyRecorder.Tests/UiAutomation/KeyLatencyPlaywrightTests.cs` measuring ≤50 ms hedefi.

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- T014 Implement data models `src/SafeKeyRecorder/Models/ConsentSession.cs`, `SessionLogEntry.cs`, `AccessibilityPreference.cs` with validation kuralları.
- [x] T015 Implement `src/SafeKeyRecorder/Services/SessionLogService.cs` (append, purge, 24h timer, file path exposure).
- T016 Implement `src/SafeKeyRecorder/Services/KeyCaptureService.cs` capturing focused window input via Avalonia events.
- T017 Implement `src/SafeKeyRecorder/Services/AccessibilityService.cs` handling high contrast & screen reader toggles.
- T018 Implement `src/SafeKeyRecorder/Telemetry/ComplianceAuditLogger.cs` logging consent + purge kararları.
- T019 Implement `src/SafeKeyRecorder/ViewModels/ConsentDialogViewModel.cs` binding to consent metni ve seçenekler.
- T020 Create `src/SafeKeyRecorder/Views/ConsentDialog.axaml` ve code-behind with onay / vazgeç butonları, optional logging toggle.
- T021 Update `src/SafeKeyRecorder/ViewModels/MainWindowViewModel.cs` to manage live log feed, transparency banner state, consent session data.
- T022 Update `src/SafeKeyRecorder/Views/MainWindow.axaml` to display `LogArea`, consent status, logging path mesajı, accessibility toggles.
- T023 Configure `src/SafeKeyRecorder/App.axaml` + `App.xaml.cs` to register services, dependency injection ve tema kaynakları.

## Phase 3.4: Integration
- T024 Wire `SessionLogService` ve `ComplianceAuditLogger` into application startup, ensure log yolu bildirimi GUI/console üzerinde.
- T025 Integrate auto delete scheduler with application closing events ve manual purge checkbox.
- T026 Connect accessibility service to theme switching ve `AutomationProperties` assignment across views.
- T027 Ensure Playwright test harness executes by adding launch profile/script and CI hook (`.github/workflows` or local script) for UI automation.

## Phase 3.5: Polish
- T028 [P] Update `specs/001-bu-uygulama-c/quickstart.md` with final CLI komutları ve gözlemler (log yolu, purge onayı, accessibles).
- T029 [P] Create compliance summary `docs/compliance/safekeyrecorder.md` capturing KVKK/GDPR değerlendirmesi ve anayasa kontrolleri.
- T030 [P] Run `dotnet format` + analyzer fixes, ensure tüm testler (`dotnet test ...`) PASS ve raporu `docs/reports/test-summary.md` içine yaz.
- T031 Perform manual validation per quickstart checklist, kaydı dokümante et ve release notlarını `docs/release-notes/v1.md` içinde hazırla.

## Dependencies
- Tests (T005-T013) must fail before starting implementation (T014-T023).
- Accessibility tasks (T017, T022, T026) depend on base views (T019-T021).
- Auto delete integration (T025) depends on log service (T015) and main window updates (T021-T022).
- Polish tasks (T028-T031) run only after integration completes.

## Parallel Execution Example
```
# Execute these independent tests in parallel once setup (T001-T004) is done:
tasks.Run(T005)
tasks.Run(T006)
tasks.Run(T011)
tasks.Run(T012)
```
