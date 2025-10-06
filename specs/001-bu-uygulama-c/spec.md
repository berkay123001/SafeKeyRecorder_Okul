# Feature Specification: SafeKeyRecorder Avalonia Consent-Driven Simulation

**Feature Branch**: `001-bu-uygulama-c`  
**Created**: 2025-09-30  
**Status**: Draft  
**Input**: User description: "Bu uygulama, C# ve AvaloniaUI kullanılarak geliştirilen etik bir \"keylogger simülasyonu\"dur... Her oturum izne tabidir."  
**Constitution Reference**: SafeKeyRecorder Constitution v1.0.0

## Execution Flow (main)
```
1. Parse user description from Input
   → If empty: ERROR "No feature description provided"
2. Extract key concepts from description
   → Identify: actors, actions, data, constraints
3. For each unclear aspect:
   → Mark with [NEEDS CLARIFICATION: specific question]
4. Validate consent, şeffaflık ve veri gizliliği gereksinimlerini çıkar
   → Kullanıcı bilgilendirmesi, onay akışı ve veri imha kuralları tanımlı mı?
5. Fill User Scenarios & Testing section
   → If no clear user flow: ERROR "Cannot determine user scenarios"
6. Generate Functional Requirements
   → Each requirement must be testable
   → Mark ambiguous requirements
7. Identify Key Entities (if data involved)
8. Run Review Checklist
   → If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties"
   → If implementation details found: ERROR "Remove tech details"
9. Return: SUCCESS (spec ready for planning)
```

---

## ⚡ Quick Guidelines
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers
- 🔒 Kullanıcı rızası, veri saklama sınırları ve simülasyon kapsamını açıkça tanımla
- 🪪 Şeffaflık mesajları ve etik kullanım kısıtlarını dokümante et

### Section Requirements
- **Mandatory sections**: Must be completed for every feature
- **Optional sections**: Include only when relevant to the feature
- When a section doesn't apply, remove it entirely (don't leave as "N/A")

### For AI Generation
When creating this spec from a user prompt:
1. **Mark all ambiguities**: Use [NEEDS CLARIFICATION: specific question] for any assumption you'd need to make
2. **Don't guess**: If the prompt doesn't specify something (e.g., "login system" without auth method), mark it
3. **Think like a tester**: Every vague requirement should fail the "testable and unambiguous" checklist item
4. **Common underspecified areas**:
   - User types and permissions
   - Data retention/deletion policies  
   - Performance targets and scale
   - Error handling behaviors
   - Integration requirements
   - Security/compliance needs

---

## Clarifications

### Session 2025-09-30
- **Performance latency**: UI güncellemesi tuş basımından itibaren en fazla 50 ms içinde görünmelidir.
- **Log formatı**: `session_log.txt` dosyasında her satır `ISO8601 zaman damgası, karakter` şeklinde tutulacaktır.
- **Log tutma süresi**: Kullanıcı logu saklamayı seçse dahi dosya 24 saat sonra otomatik silinir.
- **Erişilebilirlik**: Uygulama hem yüksek kontrast tema hem de ekran okuyucu etiketleri sağlayacaktır.

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
Bir güvenlik eğitmeni SafeKeyRecorder simülasyonunu başlatır, kullanıcıya rıza metnini gösterir, onay alır ve oturum boyunca baskılanan tüm tuşları Avalonia tabanlı arayüzde anlık olarak izler; kullanıcının isteği üzerine oturum kaydını yerel dosyada saklar ve kapanışta kayıtların silinmesi için seçenek sunar.

### Acceptance Scenarios
1. **Given** uygulama ilk kez başlatılmıştır ve kullanıcı henüz onay vermemiştir, **When** kullanıcı bilgilendirme ekranındaki "Devam" seçeneğini onaylar, **Then** tuş kaydı başlar, GUI `LogArea` bileşeni gerçek zamanlı güncellenir ve rıza durumu günlüğe yazılır.
2. **Given** kullanıcı dosya kaydını etkinleştirmeyi seçmiştir, **When** kullanıcı bir tuşa basar, **Then** karakter GUI'de görünür ve eş zamanlı olarak `~/SafeKeyRecorder/session_log.txt` dosyasına eklenir.
3. **Given** oturum sona ermek üzeredir, **When** kullanıcı kapanış iletişim kutusunda "Oturum logunu sil" onay kutusunu işaretleyip çıkışı onaylar, **Then** geçici log dosyası güvenli biçimde silinir ve kullanıcıya işlem sonucu bildirilir.
4. **Given** erişilebilirlik ayarları etkinleştirilmiştir, **When** kullanıcı yüksek kontrast temasını ve ekran okuyucu etiketlerini talep eder, **Then** arayüz yüksek kontrast palete geçer ve tüm etkileşimli öğeler erişilebilirlik etiketleriyle sunulur.

### Edge Cases
- Kullanıcı bilgilendirme ekranında "Hayır" seçerse oturum başlamamalı, arayüz keylogger öğelerini devre dışı bırakmalı ve uygulama güvenle kapanmalıdır.
- `~/SafeKeyRecorder/` dizini oluşturulamıyorsa dosya kaydı devre dışı bırakılmalı ve kullanıcıya izin/hak sorununa dair açıklayıcı hata sunulmalıdır.
- Uzun süreli oturumlarda log alanı aşırı büyürse kaydırma ve bellekte sınırlandırma stratejileri devreye girmelidir.
- Erişilebilirlik modu aktifken yüksek kontrast teması veya ekran okuyucu etiketleri yüklenemezse uygulama kullanıcıya uyarı gösterip varsayılan temaya güvenli geri dönüş sağlamalıdır.

## Requirements *(mandatory)*

- **FR-001**: System MUST blok key capture until kullanıcı rızası açıkça alınır ve onay kutusu durumunu kaydeder.
- **FR-002**: System MUST render the consent message exactly as "Bu uygulama bir keylogger simülasyonudur..." metniyle, kullanıcıya devam/iptal seçenekleri sunar.
- **FR-003**: System MUST display all basılan tuşları gerçek zamanlı olarak Avalonia `TextBox`/`LogArea` bileşeninde göstermelidir.
- **FR-004**: System MUST allow users to toggle optional local logging; varsayılan durum kapalı olmalıdır.
- **FR-005**: System MUST create and append to `~/SafeKeyRecorder/session_log.txt` only when kullanıcı onay vermiştir ve dizin yoksa güvenle oluşturmalıdır.
- **FR-006**: System MUST show active logging path bilgisini GUI üzerinde okunabilir bir mesajla paylaşıp kullanıcıyı dosya üzerinde tam kontrol sahibi yapmalıdır.
- **FR-007**: System MUST provide kapanışta "Oturum logunu sil" seçeneği ve onaylandığında dosyayı kalıcı olarak kaldırmalıdır.
- **FR-008**: System MUST ensure oturum bitişinde ve uygulama beklenmedik kapanışlarında tüm geçici verileri RAM ve dosya sisteminden imha eder.
- **FR-009**: System MUST forbid background execution; pencere odaklı çalışmalı ve arka planda gizli kayıt başlatmamalıdır.
- **FR-010**: System MUST never ilet captured data üzerinden ağ veya IPC; tüm veri yerel makine sınırları içinde kalmalıdır.
- **FR-011**: System MUST log kullanıcı rıza durumunu ve log silme kararını şeffaflık amacıyla ayrı bir denetim kanalında kaydetmeli (GUI mesajı veya ayrı denetim kaydı).
- **FR-012**: System MUST support .NET 8.0 üzerinde Linux dağıtımlarında çalışacak şekilde paketlenmeli; AvaloniaUI bileşenleri platform bağımsız işlevselliği korumalıdır.
- **FR-013**: UI latency MUST remain ≤50 ms between tuş basımı ve GUI'de görünme, bu limit otomasyon testleriyle doğrulanmalıdır.
- **FR-014**: Session log entries MUST be written as "ISO8601 timestamp, karakter" formatında satır başına bir olay olacak şekilde saklanmalıdır.
- **FR-015**: Kullanıcı "Logu tut" seçeneğini işaretlese bile dosya 24 saat sonra otomatik olarak silinmeli ve bu işlem kullanıcıya bildirilmeli.
- **FR-016**: Uygulama yüksek kontrast tema ve ekran okuyucu (ARIA benzeri) etiketleri sağlayarak erişilebilirlik için tam destek sunmalıdır.

### Key Entities *(include if feature involves data)*

- **ConsentSession**: Kullanıcının rıza durumu, onay zaman damgası, oturum kimliği ve logging tercihi gibi metadata'yı içerir.
- **SessionLog**: Teker teker kaydedilen tuş vuruşlarını, zaman damgalarını ve silme/retention durumunu tutan (geçici dosya veya bellekteki) yapı.

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [ ] Consent akışı ve bilgilendirme metni iş kurallarıyla uyumlu
- [ ] Gerçek zamanlı görselleştirme kullanıcı değerini net aktarır
- [ ] Dosya kaydı ve silme seçenekleri açıkça anlatılmıştır
- [ ] Tüm zorunlu bölümler dolduruldu
- [ ] Etik kullanım kısıtları ve simülasyon sınırları tanımlandı

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain
- [ ] Requirements are testable and unambiguous  
- [ ] Success criteria are measurable
- [ ] Scope is clearly bounded
- [ ] Dependencies and assumptions identified
- [ ] Kullanıcı rızası, veri imhası ve şeffaflık gereksinimleri açık

---

## Execution Status
*Updated by main() during processing*

- [ ] User description parsed
- [ ] Key concepts extracted
- [ ] Ambiguities marked
- [ ] User scenarios defined
- [ ] Requirements generated
- [ ] Entities identified
- [ ] Review checklist passed

---

## Ethical & Privacy Considerations *(mandatory)*

- **Consent Flow**: Uygulama ilk açılışta tam ekran/modal bilgilendirme gösterir; kullanıcı "Devam" butonunu onaylamadan hiçbir tuş kaydetmez. Onay red edilirse uygulama kapanır.
- **Transparency Mechanisms**: GUI üst bölümünde durum banner'ı ve log alanı görünür; aktif logging sırasında "Oturum kaydediliyor" mesajı ve dosya yolu etiketlenir.
- **Data Handling & Retention**: Varsayılan olarak yalnızca RAM üzerinde geçici saklama yapılır; kullanıcı dosya kaydını açarsa log tek bir oturum dosyasında tutulur, çıkışta silme tercihi uygulanır ve kullanıcı "Logu tut" dese dahi dosya 24 saat sonunda otomatik silinir.
- **Simulation Boundaries**: Uygulama yalnızca ön plan penceresi fokusundayken çalışır, global hook'lar veya ağ iletişimi kullanmaz; Linux üzerinde .NET 8 Avalonia dağıtımı için paketlenir ve sandbox testleri zorunludur.

---
