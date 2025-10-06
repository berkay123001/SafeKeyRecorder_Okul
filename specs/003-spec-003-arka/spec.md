# Feature Specification: Arka Plan Rızası ve Sürekli Kayıt Modu

**Feature Branch**: `003-spec-003-arka`  
**Created**: 2025-10-04  
**Status**: Draft  
**Input**: User description: "# Spec 003 – Arka Plan Rızası ve Sürekli Kayıt Modu ..."  
**Constitution Reference**: SafeKeyRecorder Constitution v2.0.0




## Clarifications

### Session 2025-10-04
- **BackgroundResume**: Evet, cihaz kilidi açıldığında arka plan kaydı otomatik yeniden başlar ve telemetriye `backgroundResume` olayı yazılır.
- **ToggleLocation**: Arka plan modu, ana pencere banner üzerindeki toggle ile yönetilir (açık → sarı, kapalı → yeşil).
- **TelemetryTiming**: `mode="background"` telemetri kayıtları purge veya oturum kapanışı sırasında şifrelenip dışa aktarım kuyruğuna eklenir.

## User Scenarios & Testing *(mandatory)*

### Primary User Story
Görme engelli bir kullanıcı, uzun süreli tuş vuruşu eğitimi kaydı almak istiyor. SafeKeyRecorder uygulamasını açıyor, rıza diyaloğunda “odak dışında da kayıt yapmama izin ver” seçeneğini işaretliyor ve arka plan modunu etkinleştiriyor. Uygulamayı simge durumuna küçültüp başka bir eğitim uygulamasında yazmaya devam ederken, SafeKeyRecorder penceresi ve sistem tepsisi sürekli “Arka plan kayıt modu aktif” uyarısını gösteriyor. Eğitim bittiğinde kullanıcı ana uygulamaya dönerek kaydı durduruyor ve logların saklanması veya silinmesi seçeneklerini değerlendiriyor.

### Acceptance Scenarios
1. **Given** kullanıcı rıza diyaloğunu açmış ve arka plan seçeneğini işaretlememiş, **When** “Devam” butonuna basarsa, **Then** yalnızca odak içi kayıt etkinleşir ve banner “Tuş kaydı aktif – dosya kaydı açık/kapalı” mesajını gösterir.
2. **Given** kullanıcı rıza diyaloğunda arka plan seçeneğini işaretlemiş, **When** “Devam” butonuna basarsa, **Then** banner “Arka plan kayıt modu aktif” mesajına ve sarı renge döner; telemetri `mode="background"` olarak kaydedilir.
3. **Given** arka plan modu açık, **When** kullanıcı uygulamayı simge durumuna küçültür ya da odak dışına çıkar, **Then** log dosyası arka plan tuşlarını da kaydeder ve banner uyarısı kaybolmaz.

### Edge Cases
- Kullanıcı arka plan seçeneğini işaretleyip “Devam”a basmadan diyaloğu kapatırsa, uygulama kayıt başlatmamalı ve Banner “Rıza bekleniyor” durumunda kalmalıdır.
- Kullanıcı arka plan modunu etkinleştirdikten sonra manuel olarak purge yaparsa, telemetri kayıtlarında `backgroundModeEnabled=false` güncellemesi yer almalı ve banner “Kayıt pasif”e dönmelidir.
- Arka plan modu açıkken uygulama kapanırsa, yeniden açıldığında varsayılan olarak kapalı başlamalı ve önceki karar telemetri/denetim kayıtlarında bulunmalıdır.

## Requirements *(mandatory)*

- **FR-001**: Uygulama, rıza diyaloğunda arka plan modu için açık kutulu bir onay seçeneği sunmalıdır.
- **FR-002**: Kullanıcı arka plan modunu kabul etmediği sürece sistem yalnızca odak içi tuşları kaydetmelidir.
- **FR-003**: Arka plan modu aktifken ana pencerede başlık çubuğu banner’ı sürekli sarı renkte, sistem tepsisinde ise eşleşen bir uyarı ikonu kapatılamaz şekilde gösterilmelidir.
- **FR-004**: Telemetry/denetim kayıtları arka plan modunun durumunu (`enabled/disabled`) ve kullanıcının verdiği zamanı kaydetmelidir.
- **FR-005**: Kullanıcı manuel olarak kaydı durdurduğunda arka plan modu devre dışı kalmalı, ana pencere banner üzerindeki toggle kapalı konuma geçmeli ve tüm bildirimler eski haline dönmelidir.
- **FR-006**: Arka plan modu seçildiyse SessionLogService, ekran kilitliyken duracak şekilde odaktan bağımsız tuş girişlerini kaydetmelidir.
- **FR-007**: Arka plan modu aktifken log satırları, kaydın arka plan veya odak içi olduğunu belirten bir işaret (örn. `BackgroundCapture=True/False`) içermelidir.
- **FR-008**: Kullanıcı arka plan modunu seçmiş olsa bile istediği zaman modu kapatabileceği bir arayüz kontrolü sunulmalıdır; bu kontrol ana pencere bannerı üzerinde yer alan bir toggle olmalıdır.
- **FR-009**: Arka plan modu açıkken otomatik imha (24 saat) ve manuel purge davranışları değişmeden çalışmalı, ancak telemetri bu aksiyonları `mode="background"` etiketiyle kaydedip purge veya oturum kapanışı sırasında şifreli dışa aktarım kuyruğuna eklemelidir.
- **FR-010**: Rıza diyaloğu kapandığında uygulama, kullanıcının seçimini denetim günlüklerine (`ComplianceAuditLogger`) `mode=foreground|background` olarak iletmelidir; arka plan modunda bu kayıt şifreli dışa aktarım için işaretlenmelidir.
- **FR-011**: Kullanıcı arka plan modunu seçtiği anda anlık bir bilgilendirme ile alt metinler veya yardım diyalogu sunulmalıdır (erişilebilirlik desteği).
- **FR-012**: Sistem, arka plan modunun varsayılanın dışında bir etik etki yarattığını kullanıcıya açıklayan kısa bir bilgilendirme metnini rıza diyaloğunda göstermelidir.

### Key Entities
- **ConsentDecision**: Kullanıcının rıza kararını, loglama tercihini, arka plan modunu ve zaman damgasını içerir.
- **AuditEntry**: Telemetry/denetim sistemi tarafından saklanan, `eventType`, `mode`, `timestamp`, `userAction` alanlarını içeren kayıt.
| Entity | Type | Description |
|---------|------|-------------|
| ConsentDecision | Model | User’s consent selection (foreground/background) |
| AuditEntry | Record | Standardized log entry for consent and telemetry |
| **BackgroundTelemetryEvent** | Struct | Represents telemetry event emitted during background mode (e.g., `backgroundResume`, `backgroundPause`) |
| **SystemLockMonitor** | Service | Observes OS-level lock/unlock events and signals the SessionLogService to pause/resume logging |


## Review & Acceptance Checklist

### Content Quality
- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed
- [x] Etik kullanım kısıtları ve simülasyon sınırları tanımlandı

### Requirement Completeness
- [x] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous  
- [x] Success criteria are measurable
- [x] Scope is clearly bounded
- [x] Dependencies and assumptions identified
- [x] Kullanıcı rızası, veri imhası ve şeffaflık gereksinimleri açık

## Ethical & Privacy Considerations *(mandatory)*

- **Consent Flow**: Kullanıcı, arka plan modu seçeneği varsayılan olarak kapalı şekilde sunulur. “Devam”a basmadan önce bilgilendirici metin ve onay kutusunu işaretlemek zorundadır; seçim denetim loglarına `mode=background` olarak kaydedilir.
- **Transparency Mechanisms**: Banner rengi sarıya döner ve “Arka plan kayıt modu aktif” mesajı sürekli görünür. Ek olarak sistem tepsisi veya başlık çubuğu uyarısı sağlanır; kullanıcı modu kapatana kadar uyarı yok edilemez.
- **Data Handling & Retention**: Loglar mevcut 24 saatlik otomatik imha politikasına tabidir. Manual purge çalıştığında arka plan modu devre dışı bırakılır ve telemetri `backgroundModeEnabled=false` kaydı üretir.
- **Simulation Boundaries**: Arka plan modu yalnızca bilgilendirilmiş rıza sonrası çalışır; sessiz veya gizli kayıt yapılamaz. Ağ erişimi kapalıdır, veriler yereldir ve kullanıcı modu istediği an sonlandırabilir.
