"""
XPos AI – Customer Segmentation (K-Means)
==========================================
Extracts customer features from orders_summary.csv,
segments using K-Means clustering.
"""

import pandas as pd
import numpy as np
from sklearn.cluster import KMeans
from sklearn.preprocessing import StandardScaler
import joblib
import json
import os

BASE = os.path.dirname(os.path.abspath(__file__))
DATA_PATH = os.path.join(BASE, "..", "..", "ml_data", "orders_summary.csv")
MODEL_PATH = os.path.join(BASE, "models", "kmeans_model.pkl")
SCALER_PATH = os.path.join(BASE, "models", "kmeans_scaler.pkl")
RESULTS_PATH = os.path.join(BASE, "models", "segments.json")

SEGMENT_META = {
    0: {"ad": "Premium Gurme", "renk": "#ff9800", "ikon": "⭐",
        "aciklama": "Yüksek harcama, büyük grup, premium ürünler"},
    1: {"ad": "Hızlı Öğle", "renk": "#2196f3", "ikon": "⚡",
        "aciklama": "Öğle saati, küçük sepet, hızlı servis"},
    2: {"ad": "Aile & Sosyal", "renk": "#4caf50", "ikon": "👨‍👩‍👧‍👦",
        "aciklama": "Orta harcama, orta grup, hafta sonu ağırlıklı"},
    3: {"ad": "Kahve & Tatlı", "renk": "#e91e63", "ikon": "☕",
        "aciklama": "Düşük harcama, 1-2 kişi, tatlı ve sıcak içecek"},
}


def prepare_features():
    """Extract segmentation features from order data."""
    df = pd.read_csv(DATA_PATH)

    yas_map = {"18-24": 0, "25-34": 1, "35-44": 2, "45-54": 3, "55+": 4}
    df["yas_kod"] = df["yas_grubu"].map(yas_map).fillna(2)

    hava_map = {"Güneşli": 0, "Bulutlu": 1, "Serin": 2, "Yağmurlu": 3, "Soğuk/Karlı": 4}
    df["hava_kod"] = df["hava_durumu"].map(hava_map).fillna(2)

    features = ["toplam_tutar", "kisi_sayisi", "urun_sayisi", "saat",
                "hafta_sonu", "yas_kod", "hava_kod", "sicaklik_c"]

    X = df[features].values
    return df, X, features


def train_model(n_clusters=4):
    """Train K-Means model and generate segment results."""
    df, X, feature_names = prepare_features()

    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)

    model = KMeans(n_clusters=n_clusters, random_state=42, n_init=10)
    model.fit(X_scaled)

    df["segment_id"] = model.labels_

    os.makedirs(os.path.dirname(MODEL_PATH), exist_ok=True)
    joblib.dump(model, MODEL_PATH)
    joblib.dump(scaler, SCALER_PATH)

    segments = []
    for seg_id in range(n_clusters):
        seg_df = df[df["segment_id"] == seg_id]
        meta = SEGMENT_META.get(seg_id, {"ad": f"Segment {seg_id}", "renk": "#9e9e9e",
                                          "ikon": "📊", "aciklama": ""})
        segments.append({
            "id": seg_id,
            "ad": meta["ad"],
            "renk": meta["renk"],
            "ikon": meta["ikon"],
            "aciklama": meta["aciklama"],
            "musteri_sayisi": int(len(seg_df)),
            "yuzde": round(len(seg_df) / len(df) * 100, 1),
            "ortalama_tutar": round(float(seg_df["toplam_tutar"].mean()), 2),
            "ortalama_kisi": round(float(seg_df["kisi_sayisi"].mean()), 1),
            "ortalama_urun": round(float(seg_df["urun_sayisi"].mean()), 1),
            "toplam_ciro": round(float(seg_df["toplam_tutar"].sum()), 2),
            "hafta_sonu_orani": round(float(seg_df["hafta_sonu"].mean() * 100), 1),
            "en_yogun_saat": int(seg_df["saat"].mode()[0]) if len(seg_df) > 0 else 12,
        })

    segments.sort(key=lambda s: -s["toplam_ciro"])

    result = {
        "toplam_siparis": len(df),
        "segment_sayisi": n_clusters,
        "segmentler": segments
    }

    with open(RESULTS_PATH, "w", encoding="utf-8") as f:
        json.dump(result, f, ensure_ascii=False, indent=2)

    print(f"✓ K-Means model trained ({n_clusters} segments)")
    return result


def get_segments():
    """Return cached segment results."""
    if not os.path.exists(RESULTS_PATH):
        return train_model()
    with open(RESULTS_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


if __name__ == "__main__":
    print("XPos AI – K-Means Customer Segmentation")
    print("=" * 45)
    result = train_model()
    for s in result["segmentler"]:
        print(f"  {s['ikon']} {s['ad']} – {s['musteri_sayisi']} customers ({s['yuzde']}%)")
