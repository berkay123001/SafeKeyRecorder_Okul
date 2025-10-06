<!--
Sync Impact Report
- Version: 1.0.0 → 2.0.0
- Modified Principles: II. Şeffaflık ve Bilgilendirme; V. Simülasyon Sınırları; Etik Kullanım Kısıtları
- Added Sections: None
- Removed Sections: None
- Templates requiring updates: ✅ .specify/templates/plan-template.md; ✅ .specify/templates/spec-template.md
- Follow-up TODOs: ⚠ Update specs/001-bu-uygulama-c/plan.md to adopt conditional background capture rule
-->

# SafeKeyRecorder Constitution

## Core Principles

### I. Kod Kalitesi
- Tüm kod incelemeleri, otomatik testler ve statik analizler tamamlanmadan hiçbir değişiklik ana dala birleşemez.
- Test kapsamı kritik senaryoları ve veri gizliliği kontrollerini içermek ZORUNDADIR.
- Geliştirilen her modül, simülasyonun sınırlarını ve varsayımlarını açıklayan kısa dokümantasyonla teslim edilmelidir.

### II. Şeffaflık ve Bilgilendirme
- Uygulama her başlatıldığında kullanıcıya metinsel ve görsel bilgilendirme sunmalı, rıza alınmadan veri kaydı başlatılamaz.
- Konsol ve günlükler, kaydedilen tüm girdi olaylarını ve bunların saklama durumunu açıkça belirtmek ZORUNDADIR.
- Arka plan kaydı yalnızca kullanıcı açıkça onay verir, uygulama süresince kesintisiz ve kapatılamaz bir görsel uyarı gösterilir ve uyarı kanalına erişim engellenemezse mümkündür; aksi hâlde gizli çalışmayı sağlayacak servisler yasaktır.

### III. Veri Gizliliği ve Rıza
- Varsayılan olarak hiçbir tuş vuruşu kalıcı depolamaya yazılamaz; kayıtlar yalnızca kullanıcı onayıyla ve simülasyon süresince RAM üzerinde tutulmalıdır.
- Kullanıcı isteği ile tüm kayıtlar anında ve geri döndürülemez biçimde silinmelidir.
- Pseudonim veya maskeleme uygulanmadığı sürece gerçek kişi verileri işlenemez.

### IV. Sorumluluk ve Hukuki Uyum
- Proje sadece eğitim ve güvenlik farkındalığı amaçlıdır; kötüye kullanım girişimleri reddedilmeli ve raporlanmalıdır.
- Geliştirme kararları KVKK, GDPR ve ilgili yerel mevzuatla uyumlu olmak ZORUNDADIR.
- Her sürüm notu, etik ve hukuki uyum değerlendirmesi içerir.

### V. Simülasyon Sınırları
- Simülasyon gerçek sistemlere zarar veremez; testler izole sanal ortamlarda koşmalıdır.
- Ağ üzerinden veri sızdıracak veya üçüncü taraflara aktaracak herhangi bir fonksiyon eklenmesi yasaktır.
- Arka plan veya global hook tabanlı kayıtlar yalnızca bilgilendirilmiş rıza, kesintisiz görsel uyarı, cihaz kilitlendiğinde otomatik duraklatma ve telemetride tam şeffaflık koşullarını sağlıyorsa etkinleştirilebilir; aksi hâlde devre dışı bırakılmalıdır.

## Etik Kullanım Kısıtları
- Ürün belgelerinde ve kurulum yönergelerinde kullanıcı rızasının zorunlu olduğu açıkça belirtilmelidir.
- Varsayılan yapılandırma, kayıt özelliğini kapalı tutmalı ve etkinleştirme için iki adımlı onay istemelidir.
- Arka plan modu, açık onay verilmeden etkinleştirilemez; etkinleştiğinde uygulama kilitlendiğinde kayıt durmalı ve modun başlangıç/bitiş zamanları telemetriye şifrelenmiş biçimde kaydedilmelidir.
- Eğitim dışındaki dağıtımlar için ayrı bir risk değerlendirmesi ve yönetici onayı gereklidir.

## Geliştirme Süreçleri ve Denetimler
- Her sprint sonunda etik kontrol listesi gözden geçirilmeli, şeffaflık ve gizlilik testleri tekrar edilmelidir.
- Kod incelemeleri sırasında `Kod Kalitesi`, `Şeffaflık`, `Veri Gizliliği`, `Sorumluluk` ve `Simülasyon Sınırları` kontrol noktaları işaretlenmeden onay verilemez.
- Günlükler ve telemetri yalnızca simülasyon kapsamındaki metrikleri toplamalıdır; herhangi bir gerçek kullanıcı verisi tespit edilirse derhal temizlenmelidir.

## Governance
- Bu anayasa, SafeKeyRecorder projesindeki tüm süreçler için bağlayıcıdır ve çakışan dokümantasyonların üzerinde önceliğe sahiptir.
- Değişiklik önerileri yazılı taslak, etki analizi ve hukuki değerlendirme ile birlikte ekip oy çokluğu ile onaylanmalıdır.
- Anayasa sürümleri SemVer uygular: ilke değişiklikleri veya eklemeleri minör/major artış, ufak netleştirmeler patch artışı gerektirir.
- Her çeyrek başında uyum incelemesi yapılır; sapmalar tespit edilirse 2 hafta içinde düzeltme planı hazırlanmalıdır.

**Version**: 2.0.0 | **Ratified**: 2025-09-30 | **Last Amended**: 2025-10-04