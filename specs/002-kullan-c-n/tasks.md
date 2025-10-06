# Task Plan: Consent-Based Webhook Export

## Parallel Execution Guidance
- **Examples**
  - `task-agent run --tasks T002,T003` (contract + unit tests)
  - `task-agent run --tasks T012,T013` (documentation polish)

## Task List
- [ ] T001 Setup: Add HTTP mocking support (`RichardSzalay.MockHttp`) to `tests/SafeKeyRecorder.Tests/SafeKeyRecorder.Tests.csproj` and create shared `WebhookTestServer` helper under `tests/SafeKeyRecorder.Tests/TestSupport/` for capturing outbound requests. (Prereq for T002–T004)
- [ ] T002 [P] Tests: Create failing contract tests in `tests/SafeKeyRecorder.Tests/Contracts/WebhookDeliveryContractTests.cs` verifying POST to predefined webhook with `Authorization: Bearer`, `Content-Type` headers, and status handling. (Waits on T001)
- [ ] T003 [P] Tests: Add unit tests in `tests/SafeKeyRecorder.Tests/Unit/WebhookUploadServiceTests.cs` covering retry/backoff, success logging, and consent enforcement against the mock server. (Waits on T001)
- [ ] T004 [P] Tests: Extend integration test in `tests/SafeKeyRecorder.Tests/Integration/ConsentFlowTests.cs` (or new `WebhookExportFlowTests.cs`) to simulate user consent, trigger upload, and assert audit entry plus failure path messaging. (Waits on T001)
- [ ] T005 Models: Implement `WebhookConsentRecord` and `WebhookTransmissionAttempt` in `src/SafeKeyRecorder/Models/` with validation and retention helpers aligned to tests. (Waits on T002–T004)
- [ ] T006 Configuration: Introduce `WebhookOptions` binding in `src/SafeKeyRecorder/Configuration/` reading URL/token from secure configuration plus `appsettings.json` skeleton and document secret rotation. (Waits on T005)
- [ ] T007 Service: Implement `WebhookUploadService` in `src/SafeKeyRecorder/Services/` using injected `HttpClient`, bearer header, retry policy, and raising audit events for attempts. (Waits on T006)
- [ ] T008 Telemetry: Update `src/SafeKeyRecorder/Telemetry/ComplianceAuditLogger.cs` to log webhook attempts, outcomes, and user cancellations. (Waits on T007)
- [ ] T009 ViewModel: Extend `src/SafeKeyRecorder/ViewModels/MainWindowViewModel.cs` (and related dialog VM) to surface export command, confirmation dialog, progress states, and error messaging. (Waits on T007–T008)
- [ ] T010 UI: Modify `src/SafeKeyRecorder/Views/ConsentDialog.axaml` and `Views/MainWindow.axaml` to add export controls, status indicators, and accessibility hooks. (Waits on T009)
- [ ] T011 Composition: Register new options, service, and HttpClient in `src/SafeKeyRecorder/App.axaml.cs` or DI bootstrap, ensuring feature flag respects consent revocation. (Waits on T007–T010)
- [ ] T012 [P] Polish: Update `specs/002-kullan-c-n/quickstart.md` with manual test steps for webhook export consent flow and failure recovery. (Waits on T009–T011)
- [ ] T013 [P] Polish: Document security posture in `docs/SECURITY.md` (or create if missing) covering bearer token management, TLS requirements, and audit retention. (Waits on T011)

## Notes
- [P] tasks can run together when they touch separate files or documentation.
- Respect TDD order by completing T002–T004 before implementation tasks.
