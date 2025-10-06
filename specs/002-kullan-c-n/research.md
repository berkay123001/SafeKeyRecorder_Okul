# Research Notes: Consent-Based Webhook Export

## Decision: Upload via `HttpClient` with bearer token header
- **Rationale**: The existing desktop client already references .NET libraries; `HttpClient` is first-class, supports TLS validation, and allows simple header injection for the shared secret token.
- **Alternatives considered**: External CLI like `curl` (adds dependency, harder to distribute), third-party REST SDKs (unnecessary overhead).

## Decision: Store webhook configuration in app configuration files
- **Rationale**: Keeps the predefined webhook URL and bearer token outside of user control while still allowing environment-specific overrides through configuration providers.
- **Alternatives considered**: Hardcoding constants in code (less flexible, harder to rotate secrets), exposing settings in UI (violates requirement that end users cannot edit destination).

## Decision: Queue retry with exponential backoff for transient failures
- **Rationale**: Networks can exhibit intermittent issues; limited retries improve reliability without risking duplicate deliveries. Log outcome regardless of success.
- **Alternatives considered**: Single-shot attempt (reduces reliability), full background job queue (unnecessary complexity for single webhook).

## Decision: Limit payload to raw `session_log.txt` contents with metadata headers
- **Rationale**: Requirements emphasise sending the log as-is; metadata like session id, timestamp, and checksum can reside in HTTP headers for quick validation.
- **Alternatives considered**: Wrapping log content in JSON (requires escaping and larger payload), compressing file (adds processing cost and complexity unless required).
