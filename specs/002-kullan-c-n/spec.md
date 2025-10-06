# Feature Specification: Consent-Based Webhook Export

**Feature Branch**: `002-kullan-c-n`  
**Created**: 2025-10-01  
**Status**: Draft  
**Input**: User description: "Kullanıcının onayıyla session_log.txt içeriği, HTTP POST protokolüyle önceden tanımlı bir webhook adresine gönderilecektir. Bu işlem sadece kullanıcı onayıyla gerçekleşir.Projenin sonraki aşamalarında yapılabilir"  
**Constitution Reference**: SafeKeyRecorder Constitution v1.0.0

## Clarifications

### Session 1 (2025-10-01)
- Webhook kimlik doğrulaması: Her POST isteği, yapılandırılmış statik paylaşılan gizli anahtarı `Authorization: Bearer <token>` başlığı ile taşımalıdır.

## User Scenarios & Testing *(mandatory)*

### Primary User Story
Bir SafeKeyRecorder kullanıcısı, kayıt altına alınmış oturum günlüklerini dış bir uyumluluk hizmetine iletmek istediğinde, uygulama kendisinden açık onay alarak `session_log.txt` dosyasını önceden tanımlanmış webhook adresine güvenli biçimde gönderir.

### Acceptance Scenarios
1. **Given** kullanıcı oturum loglamasını daha önce etkinleştirmiş ve gönderim panelini açmış, **When** kullanıcı "logları gönder" seçeneğini işaretleyip onay diyaloğunda izin verdiğinde, **Then** sistem dosyayı HTTP POST ile tanımlı webhook adresine gönderir ve başarıyla tamamlandığını bildirir.
2. **Given** kullanıcı onay diyaloğunda işlemi reddetmiş, **When** kullanıcı gönderim denemesi yaparsa, **Then** sistem hiçbir veri göndermeden işlemin iptal edildiğini ve logların yerel kaldığını açıkça belirtir.

### Edge Cases
- **Webhook erişilemez**: Ağ hatası veya 4xx/5xx yanıtı alınırsa kullanıcıya başarısızlık ve yeniden deneme seçenekleri sunulmalıdır.
- **Dosya yok**: `session_log.txt` oluşturulamamışsa sistem kullanıcıya dosyanın bulunamadığını veya boş olduğunu bildirir.
- **Yinelenen onay**: Kullanıcı gönderimden sonra onayını geri çekerse sonraki otomatik veya manuel çağrılar engellenir.

## Requirements *(mandatory)*

- **FR-001**: Sistem, `session_log.txt` içeriğini yalnızca kullanıcı arayüzü üzerinden alınan açık onay sonrasında dışa aktarmalıdır.
- **FR-002**: Gönderim öncesinde kullanıcıya gönderilecek veri özeti ve hedef webhook adresinin sahibi hakkında şeffaf bilgilendirme yapılmalıdır.
- **FR-003**: Webhook adresi proje yönetişimi tarafından belirlenen tekil bir yapılandırmadan okunmalı ve son kullanıcı tarafından değiştirilememelidir.
- **FR-004**: Veri aktarımı HTTPS üzerinden HTTP POST isteğiyle ve TLS doğrulaması ile gerçekleştirilmelidir.
- **FR-010**: Her POST isteği, yapılandırılmış statik paylaşılan gizli anahtarı `Authorization: Bearer <token>` başlığı ile göndererek webhook kimlik doğrulamasını sağlamalıdır.
- **FR-005**: Sistem, gönderim sonucunu (başarılı, başarısız, kullanıcı iptali) kullanıcıya gerçek zamanlı olarak göstermelidir.
- **FR-006**: Kullanıcı onayını geri çektiğinde sistem gelecekteki tüm gönderim girişimlerini engellemeli ve bu kararın kaydını tutmalıdır.
- **FR-007**: Başarılı veya başarısız her gönderim denemesi, zaman damgası ve sonuç bilgisiyle birlikte denetim günlüğüne eklenmelidir.
- **FR-008**: Gönderim sırasında ağ kesintisi veya sunucu hatası yaşanırsa sistem verileri saklamalı, kullanıcıya bilgi vermeli ve yeniden deneme seçeneği sunmalıdır.
- **FR-009**: Gönderim sonrası kullanıcı, gönderilen verinin kullanıcı talebi üzerine kalıcı olarak silinebildiğinden emin olmalıdır.

### Key Entities *(include if feature involves data)*
- **WebhookConsentRecord**: Kullanıcının webhook paylaşımına dair verdiği veya geri çektiği izni, zaman damgalarını ve bağlam notlarını tutar.
- **WebhookTransmissionAttempt**: Her gönderim denemesinin hedef adresini, içerik özetini, sonucu, hata mesajlarını ve kullanıcıya gösterilen bildirimleri temsil eder.

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

## Execution Status

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [x] Review checklist passed

## Ethical & Privacy Considerations *(mandatory)*

- **Consent Flow**: Gönderim yalnızca kullanıcıya tek seferlik onay ekranında aktarılan veri özetini ve hedef kurum bilgisini onaylaması durumunda başlatılır; kullanıcı her zaman işlemi iptal edebilir veya sonradan iznini geri çekebilir.
- **Transparency Mechanisms**: Gönderim ekranı, ilerleme durumunu, hedef webhook adresinin güvenlik ibaresini ve başarısızlık sebebini (varsa) açıkça listeler; tamamlandığında logların paylaşıldığı net şekilde belirtilir.
- **Data Handling & Retention**: `session_log.txt` yerel cihazda saklanmaya devam eder; gönderim logları yalnızca denetim amacıyla tutulur ve kullanıcı talebi üzerine 24 saat içinde silinir.
- **Simulation Boundaries**: Paylaşım, önceden tanımlı kurumsal webhook dışında başka hedeflere yönlendirilemez; sistem herhangi bir otomatik tekrar veya gizli gönderim başlatmaz, kullanıcı müdahalesi olmadan veri çıkışı yapılmaz.
