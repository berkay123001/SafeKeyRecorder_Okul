
# Implementation Plan: Consent-Based Webhook Export

**Branch**: `002-kullan-c-n` | **Date**: 2025-10-01 | **Spec**: `specs/002-kullan-c-n/spec.md`
**Input**: Feature specification from `/specs/002-kullan-c-n/spec.md`

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
Uygulama, kullanıcının açık onayı olmadan hiçbir veri paylaşmayan mevcut kayıt mimarisine yeni bir manuel dışa aktarma adımı ekleyecek. Kullanıcı gönderim panelinden işlemi başlattığında, `session_log.txt` dosyası önceden tanımlı kurumsal webhook adresine TLS korumalı HTTP POST isteğiyle iletilecek, paylaşılan gizli anahtar `Authorization: Bearer <token>` başlığında taşınacak ve sonuç başarılı/başarısız olarak kullanıcıya bildirilecek. Denetim için tüm denemeler kaydedilecek ve başarısızlık durumlarında kullanıcı dosyanın yerelde kaldığını görecek.

## Technical Context
**Language/Version**: C# / .NET 8  
**Primary Dependencies**: Avalonia UI, `System.Net.Http` (`HttpClient`), `Microsoft.Extensions.Configuration`  
**Storage**: Yerel dosya sistemi (`session_log.txt`, yapılandırma dosyaları)  
**Testing**: xUnit, Avalonia Headless runner, Playwright (mevcut altyapı)  
**Target Platform**: Masaüstü (Windows, Linux, macOS)  
**Project Type**: Tek masaüstü uygulama (Avalonia)  
**Performance Goals**: Webhook çağrıları ≤3 saniyede tamamlanmalı veya kullanıcıya hata bildirimi sunulmalı  
**Constraints**: Tek hedef webhook, TLS zorunlu, kullanıcı onayı olmadan gönderim yapılamaz, paylaşılan gizli anahtarın güvenli saklanması  
**Scale/Scope**: Düşük hacimli log dosyaları (KB–MB), manuel tetiklenen gönderimler; aynı anda tek gönderim beklenir

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Kod Kalitesi: Webhook servisine ait birim testleri, başarısız/success durumlarını kapsayan sözleşme ve entegrasyon testleri yazılacak; mevcut .NET analyzer'ları ve CI boru hattı kullanılmaya devam edecek.
- Şeffaflık ve Bilgilendirme: Onay diyaloğu kullanıcıya gönderilecek veri ve hedef adresi gösteriyor; sonuç ekranı işlemin durumunu açıkça paylaşıyor.
- Veri Gizliliği ve Rıza: Gönderim varsayılan olarak kapalı, yalnızca anlık kullanıcı onayıyla tetikleniyor; başarısızlıkta veri cihazdan çıkmıyor ve kullanıcı isterse logları silebiliyor.
- Sorumluluk ve Hukuki Uyum: Webhook sadece proje yönetişimi tarafından belirlenen kurumsal uç noktaya gidiyor; denetim günlüğü KVKK/GDPR raporlaması için tutuluyor ve kullanıcı iptal hakkı korunuyor.
- Simülasyon Sınırları: Otomatik veya arka plan gönderimi yok; tek yönlü HTTPS çağrısı ile sınırlı, gerçek sistemlere zarar verme riski bulunmuyor.

## Project Structure

### Documentation (this feature)
```
specs/002-kullan-c-n/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md (Phase 2'de üretilecek)
```

### Source Code (repository root)
```
src/
└── SafeKeyRecorder/
    ├── Models/
    ├── Services/
    ├── Telemetry/
    ├── ViewModels/
    └── Views/

tests/
└── SafeKeyRecorder.Tests/
    ├── Contracts/
    ├── Integration/
    ├── UiAutomation/
    └── Unit/

**Structure Decision**: Tek Avalonia masaüstü projesi. Webhook gönderim servisi `src/SafeKeyRecorder/Services/` altında genişletilecek; denetim ve telemetri akışları `Telemetry/` klasöründe yönetilecek. Kullanıcı onay ve sonuç UI akışları `ViewModels` ile `Views` katmanlarında güncellenecek, xUnit testler `tests/SafeKeyRecorder.Tests/` altındaki mevcut klasörlerde sürdürülecek.
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/bash/update-agent-context.sh windsurf`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

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

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

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
- [x] Phase 0: Research complete (/plan command)
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
*Based on SafeKeyRecorder Constitution v1.0.0 - See `/memory/constitution.md`*
