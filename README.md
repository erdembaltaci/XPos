# XPos – AI Destekli Restoran POS Sistemi

Modern restoranlar için geliştirilmiş, uçtan uca tam yönetim çözümü. QR kod tabanlı müşteri menüsü, makine öğrenmesi destekli öneri motoru, gerçek zamanlı sipariş takibi ve kapsamlı yönetim paneliyle donatılmıştır.

---

## 🏗️ Mimari Genel Bakış

Proje **Clean Architecture** prensiplerine dayalı olarak katmanlı bir yapıda geliştirilmiştir:

```
XPos/
├── src/
│   ├── XPos.Domain/          → Temel varlıklar (Entity) ve iş kuralları
│   ├── XPos.Application/     → Servis arayüzleri (Interface) ve use-case işlemleri
│   ├── XPos.Infrastructure/  → EF Core, SQLite, veri erişim katmanı
│   ├── XPos.WebAPI/          → ASP.NET Core REST API + SignalR Hub
│   ├── XPos.Client/          → Blazor WebAssembly müşteri menüsü
│   ├── XPos.Mobile/          → MAUI Blazor Hybrid yönetim uygulaması
│   ├── XPos.Shared/          → Paylaşılan DTO ve modeller
│   └── XPos.ML/              → Python FastAPI yapay zeka servisi
└── ml_data/                  → Apriori eğitim verileri ve öneri JSON dosyası
```

---

## ✨ Özellikler

### 🖥️ Yönetim Uygulaması (`XPos.Mobile` – MAUI Blazor Hybrid)
- **Dashboard** – Günlük ciro, sipariş sayısı, masa doluluk oranı; yapay zeka kampanya önerileri

> **Not:** `XPos.Mobile` hem Windows masaüstü hem Android olarak çalışır. Hedef platforma göre farklı komut kullanılır (bkz. Kurulum).
- **Sipariş Takibi** – SignalR ile gerçek zamanlı sipariş akışı
- **Masa Yönetimi** – Masa durumu, token tabanlı güvenli QR kod üretimi ve yazdırma
- **Menü Yönetimi** – Ürün/kategori CRUD, fotoğraf yükleme (Base64), anlık arama
- **Raporlar** – Gelir grafikleri, en çok satan ürünler
- **AI Önerileri** – Ürün bazlı akıllı öneri paneli (Apriori + kategori kuralları)
- **Personel Yönetimi** – Garson/kasiyer hesap yönetimi

### 🌐 Müşteri Menüsü (`XPos.Client` – Blazor WebAssembly)
- QR kod ile masa bazlı erişim (token doğrulaması)
- Kategori filtreli dijital menü
- Sepet yönetimi, sipariş gönderme
- **AI Lezzet Sihirbazı** – Sepet içeriğine göre akıllı ürün önerileri
- Aktif kampanya bildirimi (indirim/fırsat)
- Karanlık/Aydınlık mod desteği

### 🔙 Backend (`XPos.WebAPI` – ASP.NET Core .NET 9)
- RESTful API, JWT kimlik doğrulama ve yetkilendirme
- **SignalR** (`/orderHub`) ile anlık sipariş bildirimleri
- EF Core + SQLite (`XPosDb_v3.sqlite`) ile veri yönetimi
- Swagger UI (geliştirme ortamında otomatik açılır)

**Mevcut Controller'lar:**
| Controller | Açıklama |
|---|---|
| `AuthController` | Staff girişi, JWT token üretimi |
| `OrdersController` | Sipariş oluşturma, güncelleme, listeleme |
| `ProductsController` | Menü ürün yönetimi |
| `CategoriesController` | Kategori yönetimi |
| `TablesController` | Masa yönetimi, QR token |
| `ReportsController` | Satış raporları |
| `StaffController` | Personel yönetimi |
| `StationController` | İstasyon/kasa ayarları |
| `MlController` | AI servisine proxy uç noktaları |

### 🤖 Yapay Zeka Servisi (`XPos.ML` – Python FastAPI)
Bağımsız bir mikroservis olarak **port 5001**'de çalışır. Doğrudan SQLite veritabanını okuyarak gerçek zamanlı analiz yapar.

| Endpoint | Yöntem | Açıklama |
|---|---|---|
| `GET /` | – | Servis durumu |
| `GET /api/recommendations` | Ürün bazlı | Apriori ilişkilendirme kuralları |
| `GET /api/recommendations/basket` | Sepet bazlı | Çoklu ürün öneri motoru |
| `POST /api/recommendations/smart-basket` | Akıllı sepet | Kategori + Apriori hibrit öneri |
| `POST /api/recommendations/dashboard` | Dashboard | Yönetim paneli ürün önerileri |
| `POST /api/recommendations/retrain` | – | Modeli yeniden eğit |
| `GET /api/forecast` | Satış tahmini | Lineer regresyon (1-90 gün) |
| `POST /api/forecast/retrain` | – | Tahmin modelini yeniden eğit |
| `GET /api/campaigns` | Kampanya | Hava/saat bazlı dinamik kampanyalar |
| `GET /api/campaigns/suggest` | Öneri | En iyi kampanyayı öner |
| `POST /api/campaigns/activate` | Aktivasyon | Kampanya yayınla |
| `GET /api/campaigns/active` | Aktif | Mevcut kampanyayı getir |
| `GET /api/segments` | Segmentasyon | K-Means müşteri segmentleri |
| `GET /api/stats` | İstatistik | ML model özeti |

---

## �️ Teknoloji Yığını

| Teknoloji | Kullanım Alanı |
|---|---|
| **.NET 9** | Temel framework |
| **MAUI Blazor Hybrid** | Cross-platform yönetim uygulaması (Windows + Android) |
| **Blazor WebAssembly** | Müşteri web menüsü |
| **ASP.NET Core WebAPI** | RESTful backend |
| **SignalR** | Gerçek zamanlı sipariş bildirimleri |
| **Entity Framework Core** | SQLite ORM |
| **JWT Bearer** | Kimlik doğrulama ve yetkilendirme |
| **MudBlazor** | Material Design UI bileşen kütüphanesi |
| **Python + FastAPI** | Yapay zeka mikroservisi |
| **Pandas + MLxtend** | Apriori, tahmin ve segmentasyon modelleri |
| **SQLite** | Yerleşik veritabanı (`XPosDb_v3.sqlite`) |

---

## ▶️ Kurulum ve Çalıştırma

### Ön Gereksinimler
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Python 3.10+](https://www.python.org/)
- Windows 10/11 (MAUI Desktop için)

### 1. Python AI Servisini Kurun
```bash
cd src/XPos.ML
pip install -r requirements.txt
```

### 2. Tüm Bileşenleri Tek Seferde Başlatın
Proje kökünde `RunApps.ps1` betiği mevcuttur; API, Client ve ML servisini paralel başlatır:
```powershell
.\RunApps.ps1
```

### 3. Manuel Başlatma (Bileşen Bazlı)

**API (Port 5029):**
```bash
cd src/XPos.WebAPI
dotnet run
```

**Müşteri Web Menüsü:**
```bash
cd src/XPos.Client
dotnet run
```

**Yapay Zeka Servisi (Port 5001):**
```bash
cd src/XPos.ML
uvicorn app:app --port 5001 --reload
```

**Masaüstü Yönetim Uygulaması (Windows Desktop):**
```bash
cd src/XPos.Mobile
dotnet run -f net9.0-windows10.0.19041.0
```

**Mobil Yönetim Uygulaması (Android – Emülatör veya Fiziksel Cihaz):**
```bash
cd src/XPos.Mobile
dotnet run -f net9.0-android
```

### 4. ML Modelini Eğitin (İlk Kullanım)
```bash
cd ml_data
python apriori_train.py
```

---

## 📡 Varsayılan Portlar

| Servis | Adres |
|---|---|
| XPos.WebAPI | `http://localhost:5029` |
| XPos.Client | `http://localhost:5030` (veya otomatik) |
| XPos.ML | `http://localhost:5001` |
| Swagger UI | `http://localhost:5029/swagger` |

---

## 💳 Ödeme Sistemi Altyapısı

Ödeme şu an manuel olarak işaretlenmektedir. Fiziksel POS entegrasyonu için mimari hazırdır:

- **GMP3 / Kablolu**: RS232/USB üzerinden POS'a tutar gönderimi
- **Android POS (App-to-App)**: Intent yöntemiyle banka uygulamasına yönlendirme
- **SoftPOS / API**: NFC özellikli cihaz üzerinden temassız ödeme bulut entegrasyonu

---

*Geliştirici Notu: Bu proje aktif geliştirme aşamasındadır.*
