"""
CafeML – Apriori Birliktelik Kuralları Eğitim Scripti
======================================================
Girdi : market_basket.csv  (generate_dataset.py çıktısı)
Çıktı : association_rules.csv  +  konsol raporu

Kullanım:
    python apriori_train.py

Ayarlar:
    MIN_SUPPORT    = 0.05   (en az 100 siparişte görülen)
    MIN_CONFIDENCE = 0.40
    MIN_LIFT       = 1.2
"""

import pandas as pd
from mlxtend.frequent_patterns import apriori, association_rules
from mlxtend.preprocessing import TransactionEncoder
import sqlite3
import os

# ─────────────────────────────────────────────
# AYARLAR
# ─────────────────────────────────────────────
MIN_SUPPORT    = 0.01
MAX_SUPPORT    = 0.60
MIN_CONFIDENCE = 0.10
MIN_LIFT       = 1.05
CIKTI_CSV      = "association_rules.csv"

# ─────────────────────────────────────────────
# 1. VERİ OKU (Mssql / SQLite üzerinden)
# ─────────────────────────────────────────────
print("📥 Veritabanından siparişler okunuyor…")

db_path = r"c:\XPos\src\XPos.WebAPI\XPosDb_v3.sqlite"
if not os.path.exists(db_path):
    print("❌ Veritabanı bulunamadı!")
    exit(1)

conn = sqlite3.connect(db_path)
query = """
    SELECT o.Id as siparis_id, p.Name as product_name
    FROM Orders o
    JOIN OrderItems oi ON o.Id = oi.OrderId
    JOIN Products p ON oi.ProductId = p.Id
"""
df_items = pd.read_sql_query(query, conn)
conn.close()

if df_items.empty:
    print("❌ Veritabanında sipariş bulunamadı!")
    exit(1)

# Sipariş ID ve Ürün adına göre sayıp sonrasında var/yok olarak boolean matrise çeviriyoruz
df = df_items.groupby(['siparis_id', 'product_name'])['product_name'].count().unstack().fillna(0)
df = df.map(lambda x: bool(x > 0))

print(f"   {len(df)} sipariş, {len(df.columns)} farklı ürün")

# ─────────────────────────────────────────────
# 2. APRIORI – SIKÇA GÖRÜLEN ÜRÜN KÜMELERİ
# ─────────────────────────────────────────────
print(f"\n⚙️  Apriori çalıştırılıyor (min_support={MIN_SUPPORT})…")
frequent_itemsets = apriori(
    df,
    min_support=MIN_SUPPORT,
    use_colnames=True,
    verbose=0
)
frequent_itemsets["length"] = frequent_itemsets["itemsets"].apply(len)

# Support değeri çok yüksek olan (fazla genelleşmiş, örneğin sadece su) itemsetleri ele
frequent_itemsets = frequent_itemsets[frequent_itemsets['support'] <= MAX_SUPPORT]

print(f"   {len(frequent_itemsets)} sık geçen itemset bulundu (max_support sınırı sonrası)")

# ─────────────────────────────────────────────
# 3. BİRLİKTELİK KURALLARI
# ─────────────────────────────────────────────
print(f"\n⚙️  Birliktelik kuralları hesaplanıyor (conf≥{MIN_CONFIDENCE}, lift≥{MIN_LIFT})…")
rules = association_rules(
    frequent_itemsets,
    metric="confidence",
    min_threshold=MIN_CONFIDENCE
)
rules = rules[rules["lift"] >= MIN_LIFT].copy()
rules = rules.sort_values("lift", ascending=False).reset_index(drop=True)

# Okunabilir format
rules["antecedents_str"] = rules["antecedents"].apply(lambda x: ", ".join(sorted(x)))
rules["consequents_str"] = rules["consequents"].apply(lambda x: ", ".join(sorted(x)))

print(f"   {len(rules)} kural üretildi")

# ─────────────────────────────────────────────
# 4. KONSOL RAPORU – EN İYİ 20 KURAL
# ─────────────────────────────────────────────
print("\n" + "─" * 80)
print(f"{'Kural (X → Y)':<55} {'Sup':>6} {'Conf':>6} {'Lift':>6}")
print("─" * 80)
for _, row in rules.head(20).iterrows():
    kural = f"{{{row['antecedents_str']}}} → {{{row['consequents_str']}}}"
    print(f"{kural:<55} {row['support']:>6.3f} {row['confidence']:>6.3f} {row['lift']:>6.3f}")
print("─" * 80)

# ─────────────────────────────────────────────
# 5. CSV ÇIKTISI
# ─────────────────────────────────────────────
cikti_df = rules[["antecedents_str","consequents_str","support","confidence","lift","leverage","conviction"]]
cikti_df.columns = ["antecedent","consequent","support","confidence","lift","leverage","conviction"]
cikti_df = cikti_df.round(4)
cikti_df.to_csv(CIKTI_CSV, index=False, encoding="utf-8")
print(f"\n✓ {CIKTI_CSV} kaydedildi ({len(rules)} kural)")

# ─────────────────────────────────────────────
# 6. MENÜ ÖNERİ JSON'U (API için)
# ─────────────────────────────────────────────
import json

oneriler = []
for _, row in rules[rules["confidence"] >= 0.10].iterrows():
    oneriler.append({
        "tetikleyici":  row["antecedents_str"],
        "oneri":        row["consequents_str"],
        "confidence":   round(float(row["confidence"]), 3),
        "lift":         round(float(row["lift"]), 3),
        "support":      round(float(row["support"]), 3),
        "oneri_metni":  f"'{row['antecedents_str']}' ile birlikte "
                        f"'{row['consequents_str']}' de ekleyin? "
                        f"(%{int(row['confidence']*100)} müşteri tercih etti)"
    })

with open("menu_oneriler.json", "w", encoding="utf-8") as f:
    json.dump(oneriler, f, ensure_ascii=False, indent=2)
print(f"✓ menu_oneriler.json kaydedildi ({len(oneriler)} yüksek güvenli kural)")

# ─────────────────────────────────────────────
# 7. ÖZET İSTATİSTİK
# ─────────────────────────────────────────────
print("\n── Özet ──────────────────────────────────────")
print(f"Yüksek lift (≥2.0) kural sayısı : {len(rules[rules['lift']>=2.0])}")
print(f"Yüksek conf (≥0.7) kural sayısı : {len(rules[rules['confidence']>=0.7])}")
if not rules.empty:
    en_iyi = rules.iloc[0]
    print(f"\n🏆 En iyi kural:")
    print(f"   {{{en_iyi['antecedents_str']}}} → {{{en_iyi['consequents_str']}}}")
    print(f"   Support={en_iyi['support']:.3f}  Confidence={en_iyi['confidence']:.3f}  Lift={en_iyi['lift']:.3f}")
else:
    print("\n⚠️  Hiçbir kural bulunamadı! Destek veya Güven eşiklerini düşürmeyi deneyin.")

print("\n✅ Apriori eğitimi tamamlandı!")
