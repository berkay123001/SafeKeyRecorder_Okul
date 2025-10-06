# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
   → Ethics & Privacy: consent UI, veri imha mekanizmaları, şeffaflık göstergeleri
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
   → User consent flow, transparency indicators, and data minimization tasks defined?
9. Return: SUCCESS (tasks ready for execution)

## Path Conventions
- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

## Phase 3.1: Setup
- [ ] T001 Create project structure per implementation plan
- [ ] T002 Initialize [language] project with [framework] dependencies
- [ ] T003 [P] Configure linting and formatting tools
- [ ] T004 Define consent prompt messaging and transparency indicators

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [ ] T005 [P] Contract test POST /api/users in tests/contract/test_users_post.py
- [ ] T006 [P] Contract test GET /api/users/{id} in tests/contract/test_users_get.py
- [ ] T007 [P] Integration test user registration in tests/integration/test_registration.py
- [ ] T008 [P] Integration test auth flow in tests/integration/test_auth.py
- [ ] T009 Transparency indicator test in tests/integration/test_transparency.py
- [ ] T010 Consent flow regression test in tests/integration/test_consent.py

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T011 [P] User model in src/models/user.py
- [ ] T012 [P] UserService CRUD in src/services/user_service.py
- [ ] T013 [P] CLI --create-user in src/cli/user_commands.py
- [ ] T014 POST /api/users endpoint
- [ ] T015 GET /api/users/{id} endpoint
- [ ] T016 Input validation
- [ ] T017 Error handling and logging
- [ ] T018 Consent enforcement middleware in src/middleware/consent.py
- [ ] T019 Transparency UI overlay in src/ui/transparency.py

## Phase 3.4: Integration
- [ ] T020 Connect UserService to DB
- [ ] T021 Auth middleware
- [ ] T022 Request/response logging
- [ ] T023 CORS and security headers
- [ ] T024 Secure in-memory buffer with timed purge

## Phase 3.5: Polish
- [ ] T025 [P] Unit tests for validation in tests/unit/test_validation.py
- [ ] T026 Performance tests (<200ms)
- [ ] T027 [P] Update docs/api.md
- [ ] T028 Remove duplication
- [ ] T029 Run manual-testing.md
- [ ] T030 Verify purge command clears all in-memory buffers

## Dependencies
- Tests (T005-T010) before implementation (T011-T019)
- T011 blocks T012, T020
- T021 blocks T023
- Implementation before polish (T025-T030)
- Ethics tasks (T018-T024, T030) must show PASS before release

## Parallel Example
```
# Launch T004-T007 together:
Task: "Contract test POST /api/users in tests/contract/test_users_post.py"
Task: "Contract test GET /api/users/{id} in tests/contract/test_users_get.py"
Task: "Integration test registration in tests/integration/test_registration.py"
Task: "Integration test auth in tests/integration/test_auth.py"
```

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Avoid: vague tasks, same file conflicts

## Task Generation Rules
*Applied during main() execution*

1. **From Contracts**:
   - Each contract file → contract test task [P]
   - Each endpoint → implementation task
   
2. **From Data Model**:
   - Each entity → model creation task [P]
   - Relationships → service layer tasks
   
3. **From User Stories**:
   - Each story → integration test [P]
   - Quickstart scenarios → validation tasks

4. **Ordering**:
   - Setup → Tests → Models → Services → Endpoints → Polish
   - Dependencies block parallel execution

## Validation Checklist
*GATE: Checked by main() before returning*

- [ ] All contracts have corresponding tests
- [ ] All entities have model tasks
- [ ] All tests come before implementation
- [ ] Parallel tasks truly independent
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task