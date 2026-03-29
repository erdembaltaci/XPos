"""
XPos AI – Dynamic Campaign Engine
===================================
Weather, time, day and customer segment-based
smart campaign suggestions.
Generates diverse, context-aware campaigns.
"""

from datetime import datetime
import random


def get_campaigns(hava="Güneşli", sicaklik=20, saat=None):
    """Return active campaigns based on current conditions."""
    if saat is None:
        saat = datetime.now().hour

    gun = datetime.now().weekday()  # 0=Mon .. 6=Sun
    campaigns = []

    # ─── Weather-based Campaigns ───
    if hava in ("Soğuk/Karlı", "Yağmurlu") or sicaklik < 10:
        campaigns.append({
            "id": "cold-soup",
            "baslik": "🍲 Soğuk Hava Kampanyası",
            "aciklama": "Çorba + Ana Yemek siparişinde çorba %30 indirimli!",
            "indirim_yuzde": 30,
            "hedef_kategoriler": ["Çorba"],
            "kosul": "Ana yemek ile birlikte",
            "oncelik": "yuksek",
            "renk": "#e65100"
        })
        campaigns.append({
            "id": "cold-hot-drink",
            "baslik": "☕ Sıcak İçecek Fırsatı",
            "aciklama": "Bugün tüm sıcak içecekler %20 indirimli",
            "indirim_yuzde": 20,
            "hedef_kategoriler": ["İçecek - Sıcak"],
            "kosul": "Hava soğuk/yağmurlu",
            "oncelik": "orta",
            "renk": "#795548"
        })

    if hava == "Güneşli" and sicaklik > 25:
        campaigns.append({
            "id": "hot-cold-drink",
            "baslik": "🧊 Serinle Kampanyası",
            "aciklama": "Limonata veya Ice Tea alana 2. bardak hediye!",
            "indirim_yuzde": 50,
            "hedef_kategoriler": ["İçecek - Soğuk"],
            "kosul": "Sıcaklık > 25°C",
            "oncelik": "yuksek",
            "renk": "#0288d1"
        })

    if hava == "Güneşli" and 15 <= sicaklik <= 25:
        campaigns.append({
            "id": "nice-weather-terrace",
            "baslik": "☀️ Teras Keyfi",
            "aciklama": "Bahçe/teras siparişlerinde soğuk içecek %15 indirimli!",
            "indirim_yuzde": 15,
            "hedef_kategoriler": ["İçecek - Soğuk"],
            "kosul": f"Hava güneşli, {sicaklik}°C",
            "oncelik": "orta",
            "renk": "#00acc1"
        })

    # ─── Time-based Campaigns ───
    if 8 <= saat < 11:
        campaigns.append({
            "id": "breakfast-early",
            "baslik": "🌅 Erken Kuş Kahvaltı",
            "aciklama": "Serpme Kahvaltı + sınırsız çay ₺550'ye!",
            "indirim_yuzde": 8,
            "hedef_kategoriler": ["Kahvaltı"],
            "kosul": "08:00–11:00 arası",
            "oncelik": "yuksek",
            "renk": "#ff9800"
        })

    if 11 <= saat < 14:
        campaigns.append({
            "id": "lunch-quick",
            "baslik": "⚡ Öğle Hızlı Menü",
            "aciklama": "Ana yemek + içecek kombosunda %12 indirim!",
            "indirim_yuzde": 12,
            "hedef_kategoriler": ["Ana Yemek", "İçecek"],
            "kosul": "11:00–14:00 arası",
            "oncelik": "yuksek",
            "renk": "#2e7d32"
        })

    if 14 <= saat < 17:
        campaigns.append({
            "id": "afternoon-dessert",
            "baslik": "🍰 İkindi Tatlı Keyfi",
            "aciklama": "Tatlı + Türk Kahvesi kombosu ₺199!",
            "indirim_yuzde": 15,
            "hedef_kategoriler": ["Tatlı", "İçecek - Sıcak"],
            "kosul": "14:00–17:00 arası",
            "oncelik": "orta",
            "renk": "#e91e63"
        })

    if 17 <= saat < 21:
        campaigns.append({
            "id": "dinner-prime",
            "baslik": "🍽️ Akşam Yemeği Özel",
            "aciklama": "2 ana yemek + 2 içecek siparişinde tatlı ikram!",
            "indirim_yuzde": 0,
            "hedef_kategoriler": ["Ana Yemek", "Tatlı"],
            "kosul": "17:00–21:00 arası",
            "oncelik": "yuksek",
            "renk": "#1565c0"
        })
        campaigns.append({
            "id": "dinner-steak",
            "baslik": "🥩 Akşam Et Menüsü",
            "aciklama": "Dana Antrikot + şarap kombosunda %10 indirim!",
            "indirim_yuzde": 10,
            "hedef_kategoriler": ["Ana Yemek"],
            "kosul": "17:00–21:00 arası, Et menü",
            "oncelik": "orta",
            "renk": "#c62828"
        })

    if 21 <= saat or saat < 2:
        campaigns.append({
            "id": "late-night-snack",
            "baslik": "🌙 Gece Atıştırma",
            "aciklama": "Patates Kızartması veya Soğan Halkası %25 indirimli!",
            "indirim_yuzde": 25,
            "hedef_kategoriler": ["Başlangıç"],
            "kosul": "21:00 sonrası",
            "oncelik": "yuksek",
            "renk": "#311b92"
        })
        campaigns.append({
            "id": "late-dessert",
            "baslik": "🍫 Gece Tatlı Fırsatı",
            "aciklama": "Profiterol veya Tiramisu + sıcak içecek ₺229!",
            "indirim_yuzde": 18,
            "hedef_kategoriler": ["Tatlı", "İçecek - Sıcak"],
            "kosul": "21:00 sonrası",
            "oncelik": "orta",
            "renk": "#6a1b9a"
        })

    # ─── Day-based Campaigns ───
    if gun in (5, 6):  # Saturday, Sunday
        campaigns.append({
            "id": "weekend-family",
            "baslik": "👨‍👩‍👧‍👦 Hafta Sonu Aile Paketi",
            "aciklama": "4+ kişilik masalarda tatlı ikram!",
            "indirim_yuzde": 0,
            "hedef_kategoriler": ["Tatlı"],
            "kosul": "4+ kişi, Cumartesi-Pazar",
            "oncelik": "orta",
            "renk": "#9c27b0"
        })
        campaigns.append({
            "id": "weekend-brunch",
            "baslik": "🥂 Hafta Sonu Brunch",
            "aciklama": "Brunch menüsü 2 kişi ₺899! (Kahvaltı + tatlı + sınırsız çay/kahve)",
            "indirim_yuzde": 15,
            "hedef_kategoriler": ["Kahvaltı", "Tatlı"],
            "kosul": "Cumartesi-Pazar, 09:00–14:00",
            "oncelik": "yuksek",
            "renk": "#ff6f00"
        })

    if gun == 0:  # Monday
        campaigns.append({
            "id": "monday-motivation",
            "baslik": "💪 Pazartesi Motivasyon",
            "aciklama": "Tüm kahve çeşitleri %20 indirimli! Haftaya enerjik başla.",
            "indirim_yuzde": 20,
            "hedef_kategoriler": ["İçecek - Sıcak"],
            "kosul": "Her Pazartesi",
            "oncelik": "yuksek",
            "renk": "#00695c"
        })

    if gun == 1:  # Tuesday
        campaigns.append({
            "id": "tuesday-pide",
            "baslik": "🫓 Salı Pide Günü",
            "aciklama": "Tüm pideler %15 indirimli!",
            "indirim_yuzde": 15,
            "hedef_kategoriler": ["Hamur İşi"],
            "kosul": "Her Salı",
            "oncelik": "yuksek",
            "renk": "#f57c00"
        })

    if gun == 2:  # Wednesday
        campaigns.append({
            "id": "wednesday-pasta",
            "baslik": "🍝 Çarşamba Makarna Festivali",
            "aciklama": "Tüm makarna çeşitlerinde %15 indirim!",
            "indirim_yuzde": 15,
            "hedef_kategoriler": ["Makarna"],
            "kosul": "Her Çarşamba",
            "oncelik": "yuksek",
            "renk": "#ef6c00"
        })

    if gun == 3:  # Thursday
        campaigns.append({
            "id": "thursday-kebab",
            "baslik": "🥩 Perşembe Kebap Şöleni",
            "aciklama": "Kebap çeşitlerinde %10 indirim + ikram çay",
            "indirim_yuzde": 10,
            "hedef_kategoriler": ["Ana Yemek - Kebap"],
            "kosul": "Her Perşembe",
            "oncelik": "yuksek",
            "renk": "#d32f2f"
        })

    if gun == 4:  # Friday
        campaigns.append({
            "id": "friday-pizza",
            "baslik": "🍕 Cuma Pizza Partisi",
            "aciklama": "Büyük boy pizzalarda 2. pizza %50 indirimli!",
            "indirim_yuzde": 50,
            "hedef_kategoriler": ["Pizza"],
            "kosul": "Her Cuma",
            "oncelik": "yuksek",
            "renk": "#d84315"
        })

    # ─── Always-on Campaigns (variety pool — pick 2-3 random) ───
    always_pool = [
        {
            "id": "combo-burger",
            "baslik": "🍔 Burger Kombo",
            "aciklama": "Burger + Patates + İçecek = ₺299 (₺350 yerine)",
            "indirim_yuzde": 14,
            "hedef_kategoriler": ["Burger", "Başlangıç", "İçecek"],
            "kosul": "Kombo sipariş",
            "oncelik": "orta",
            "renk": "#e65100"
        },
        {
            "id": "happy-hour",
            "baslik": "🎉 Happy Hour",
            "aciklama": "Tüm soğuk içeceklerde 2 al 1 öde!",
            "indirim_yuzde": 50,
            "hedef_kategoriler": ["İçecek - Soğuk"],
            "kosul": "14:00–18:00 arası",
            "oncelik": "orta",
            "renk": "#00838f"
        },
        {
            "id": "dessert-lovers",
            "baslik": "🎂 Tatlı Severler",
            "aciklama": "2 tatlı siparişinde 3. tatlı hediye!",
            "indirim_yuzde": 33,
            "hedef_kategoriler": ["Tatlı"],
            "kosul": "2+ tatlı siparişi",
            "oncelik": "orta",
            "renk": "#ad1457"
        },
        {
            "id": "salad-fit",
            "baslik": "🥗 Fit Menü",
            "aciklama": "Salata + Limonata kombosunda %10 indirim!",
            "indirim_yuzde": 10,
            "hedef_kategoriler": ["Salata", "İçecek"],
            "kosul": "Sağlıklı seçim",
            "oncelik": "dusuk",
            "renk": "#2e7d32"
        },
        {
            "id": "coffee-cake",
            "baslik": "☕ Kahve & Tatlı",
            "aciklama": "Espresso bazlı kahve + herhangi bir tatlı ₺199!",
            "indirim_yuzde": 12,
            "hedef_kategoriler": ["İçecek - Sıcak", "Tatlı"],
            "kosul": "Kahve + tatlı siparişi",
            "oncelik": "orta",
            "renk": "#4e342e"
        },
        {
            "id": "share-platter",
            "baslik": "🍽️ Paylaşım Tabağı",
            "aciklama": "Başlangıç tabağı (3 çeşit) ₺349! 2+ kişi için ideal.",
            "indirim_yuzde": 8,
            "hedef_kategoriler": ["Başlangıç"],
            "kosul": "2+ kişi masası",
            "oncelik": "dusuk",
            "renk": "#37474f"
        },
        {
            "id": "new-tastes",
            "baslik": "✨ Yeni Lezzetler",
            "aciklama": "Bu haftanın özel menüsünü deneyin — ilk siparişte %15 indirim!",
            "indirim_yuzde": 15,
            "hedef_kategoriler": ["Ana Yemek"],
            "kosul": "Yeni müşteri veya ilk deneme",
            "oncelik": "orta",
            "renk": "#283593"
        },
        {
            "id": "loyalty-program",
            "baslik": "⭐ Sadakat Programı",
            "aciklama": "10 siparişte 1 kahve hediye!",
            "indirim_yuzde": 0,
            "hedef_kategoriler": [],
            "kosul": "Sürekli",
            "oncelik": "dusuk",
            "renk": "#607d8b"
        },
    ]

    # Pick 2-3 random campaigns from always-pool for variety
    existing_ids = {c["id"] for c in campaigns}
    available_pool = [c for c in always_pool if c["id"] not in existing_ids]
    pick_count = min(random.randint(2, 3), len(available_pool))
    campaigns.extend(random.sample(available_pool, pick_count))

    # Sort by priority
    priority_order = {"yuksek": 0, "orta": 1, "dusuk": 2}
    campaigns.sort(key=lambda c: priority_order.get(c["oncelik"], 2))

    return campaigns


def get_top_campaign(hava="Güneşli", sicaklik=20, saat=None):
    """Return the single best campaign for current conditions."""
    all_campaigns = get_campaigns(hava=hava, sicaklik=sicaklik, saat=saat)
    if not all_campaigns:
        return None
    return all_campaigns[0]
