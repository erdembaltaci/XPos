"""
XPos AI – Sales Forecasting Module
====================================
Trains a Linear Regression model on:
  1) Real orders from SQLite database (priority source)
  2) Synthetic baseline data from orders_summary.csv (fallback/supplement)
Predicts next N days of revenue.
"""

import pandas as pd
import numpy as np
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import StandardScaler
import joblib
import os
import sqlite3
from datetime import datetime, timedelta

BASE = os.path.dirname(__file__)
MODEL_PATH = os.path.join(BASE, "models", "sales_model.pkl")
SCALER_PATH = os.path.join(BASE, "models", "sales_scaler.pkl")
CSV_PATH = os.path.join(BASE, "..", "..", "ml_data", "orders_summary.csv")

# Try multiple possible DB locations
DB_PATHS = [
    os.path.join(BASE, "..", "XPos.WebAPI", "XPosDb_v3.sqlite"),
    os.path.join(BASE, "..", "src", "XPos.WebAPI", "XPosDb_v3.sqlite"),
    os.path.join(BASE, "..", "..", "XPos-main", "src", "XPos.WebAPI", "XPosDb_v3.sqlite"),
]

HAVA_MAP = {"Güneşli": 0, "Bulutlu": 1, "Serin": 2, "Yağmurlu": 3, "Soğuk/Karlı": 4}

GUN_ADI_TR = ["Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar"]

SICAKLIK_MAP = {1: 5, 2: 7, 3: 12, 4: 17, 5: 22, 6: 28,
                7: 32, 8: 31, 9: 26, 10: 20, 11: 13, 12: 7}


def _find_db():
    """Locate the SQLite database file."""
    for p in DB_PATHS:
        resolved = os.path.abspath(p)
        if os.path.exists(resolved):
            return resolved
    return None


def _load_db_daily():
    """Load daily sales data from the real SQLite database."""
    db_path = _find_db()
    if not db_path:
        print("⚠️  SQLite DB not found, skipping real data")
        return pd.DataFrame()

    try:
        conn = sqlite3.connect(db_path)
        query = """
            SELECT
                date(o.CreatedAt) as tarih,
                SUM(CAST(oi.UnitPrice AS REAL) * oi.Quantity) as toplam_ciro,
                COUNT(DISTINCT o.Id) as siparis_sayisi,
                COALESCE(o.WeatherCondition, 'Güneşli') as hava,
                AVG(o.Temperature) as sicaklik
            FROM Orders o
            JOIN OrderItems oi ON oi.OrderId = o.Id
            WHERE o.Status >= 1
            GROUP BY date(o.CreatedAt)
            ORDER BY tarih
        """
        df = pd.read_sql_query(query, conn)
        conn.close()

        if df.empty:
            print("⚠️  No orders in DB yet")
            return pd.DataFrame()

        df["tarih"] = pd.to_datetime(df["tarih"])
        df["kaynak"] = "db"
        print(f"✓ DB'den {len(df)} gün sipariş verisi yüklendi ({df['tarih'].min().date()} → {df['tarih'].max().date()})")
        return df

    except Exception as e:
        print(f"⚠️  DB read error: {e}")
        return pd.DataFrame()


def _load_csv_daily():
    """Load daily sales data from the synthetic CSV."""
    if not os.path.exists(CSV_PATH):
        print("⚠️  orders_summary.csv not found")
        return pd.DataFrame()

    try:
        df = pd.read_csv(CSV_PATH)
        df["tarih"] = pd.to_datetime(df["tarih_saat"]).dt.date

        daily = df.groupby("tarih").agg(
            toplam_ciro=("toplam_tutar", "sum"),
            siparis_sayisi=("siparis_id", "count"),
        ).reset_index()

        hava_daily = df.groupby("tarih")["hava_durumu"].agg(lambda x: x.mode()[0]).reset_index()
        hava_daily.columns = ["tarih", "hava"]
        daily = daily.merge(hava_daily, on="tarih")

        sicaklik_daily = df.groupby("tarih")["sicaklik_c"].mean().reset_index()
        sicaklik_daily.columns = ["tarih", "sicaklik"]
        daily = daily.merge(sicaklik_daily, on="tarih")

        daily["tarih"] = pd.to_datetime(daily["tarih"])
        daily["kaynak"] = "csv"
        print(f"✓ CSV'den {len(daily)} gün sentetik veri yüklendi")
        return daily

    except Exception as e:
        print(f"⚠️  CSV read error: {e}")
        return pd.DataFrame()


def prepare_daily_data():
    """Merge real DB data with synthetic CSV data.

    Strategy:
    - Real DB orders are the primary source (weight=3x)
    - CSV provides baseline training data
    - If a date exists in both, DB data takes priority
    """
    db_daily = _load_db_daily()
    csv_daily = _load_csv_daily()

    if db_daily.empty and csv_daily.empty:
        raise ValueError("No training data available! Need either DB orders or CSV data.")

    if not db_daily.empty and not csv_daily.empty:
        # Remove CSV rows that overlap with DB dates
        db_dates = set(db_daily["tarih"].dt.date)
        csv_daily_filtered = csv_daily[~csv_daily["tarih"].dt.date.isin(db_dates)]
        daily = pd.concat([db_daily, csv_daily_filtered], ignore_index=True)
        print(f"✓ Birleşik: {len(db_daily)} gün gerçek + {len(csv_daily_filtered)} gün sentetik = {len(daily)} toplam")
    elif not db_daily.empty:
        daily = db_daily
    else:
        daily = csv_daily

    daily = daily.sort_values("tarih").reset_index(drop=True)

    # Feature engineering
    daily["gun_no"] = daily["tarih"].dt.dayofweek
    daily["ay"] = daily["tarih"].dt.month
    daily["hafta_sonu"] = (daily["gun_no"] >= 4).astype(int)
    daily["hava_kod"] = daily["hava"].map(HAVA_MAP).fillna(2)

    # Fill missing temperature with monthly average
    daily["sicaklik"] = daily.apply(
        lambda r: r["sicaklik"] if pd.notna(r["sicaklik"]) else SICAKLIK_MAP.get(r["ay"], 15),
        axis=1
    )

    # Cyclical encoding
    daily["gun_sin"] = np.sin(2 * np.pi * daily["gun_no"] / 7)
    daily["gun_cos"] = np.cos(2 * np.pi * daily["gun_no"] / 7)
    daily["ay_sin"] = np.sin(2 * np.pi * daily["ay"] / 12)
    daily["ay_cos"] = np.cos(2 * np.pi * daily["ay"] / 12)

    # Sample weights: real DB data gets 3x weight
    daily["weight"] = daily["kaynak"].apply(lambda k: 3.0 if k == "db" else 1.0)

    return daily


FEATURES = ["gun_no", "ay", "hafta_sonu", "hava_kod", "sicaklik",
            "gun_sin", "gun_cos", "ay_sin", "ay_cos"]


def train_model():
    """Train sales forecast model on combined data."""
    daily = prepare_daily_data()

    X = daily[FEATURES].values
    y = daily["toplam_ciro"].values
    w = daily["weight"].values

    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)

    model = LinearRegression()
    model.fit(X_scaled, y, sample_weight=w)

    score = model.score(X_scaled, y, sample_weight=w)
    db_count = int((daily["kaynak"] == "db").sum())
    csv_count = int((daily["kaynak"] == "csv").sum())
    print(f"📊 Model R²: {score:.4f}  (DB: {db_count} gün, CSV: {csv_count} gün)")

    os.makedirs(os.path.dirname(MODEL_PATH), exist_ok=True)
    joblib.dump(model, MODEL_PATH)
    joblib.dump(scaler, SCALER_PATH)
    print(f"✓ Model kaydedildi: {MODEL_PATH}")

    return model, scaler, daily


def predict_next_days(days=7, hava_listesi=None):
    """Predict revenue for the next N days."""
    if not os.path.exists(MODEL_PATH):
        print("⚠️ Model bulunamadı, eğitiliyor...")
        train_model()

    model = joblib.load(MODEL_PATH)
    scaler = joblib.load(SCALER_PATH)

    bugun = datetime.now()
    tahminler = []

    default_hava = ["Güneşli", "Bulutlu", "Güneşli", "Serin", "Bulutlu", "Güneşli", "Güneşli"]
    if hava_listesi is None:
        hava_listesi = (default_hava * ((days // 7) + 1))[:days]

    for i in range(days):
        tarih = bugun + timedelta(days=i + 1)
        gun_no = tarih.weekday()
        ay = tarih.month
        hafta_sonu = 1 if gun_no >= 4 else 0
        hava = hava_listesi[i] if i < len(hava_listesi) else "Güneşli"
        hava_kod = HAVA_MAP.get(hava, 0)
        sicaklik = SICAKLIK_MAP.get(ay, 15)

        gun_sin = np.sin(2 * np.pi * gun_no / 7)
        gun_cos = np.cos(2 * np.pi * gun_no / 7)
        ay_sin = np.sin(2 * np.pi * ay / 12)
        ay_cos = np.cos(2 * np.pi * ay / 12)

        X = np.array([[gun_no, ay, hafta_sonu, hava_kod, sicaklik,
                        gun_sin, gun_cos, ay_sin, ay_cos]])
        X_scaled = scaler.transform(X)
        tahmin = max(0, model.predict(X_scaled)[0])

        tahminler.append({
            "tarih": tarih.strftime("%Y-%m-%d"),
            "gun": GUN_ADI_TR[gun_no],
            "hava": hava,
            "sicaklik": sicaklik,
            "tahmini_ciro": round(tahmin, 2),
            "hafta_sonu": bool(hafta_sonu)
        })

    return tahminler


def get_training_stats():
    """Return summary of training data sources."""
    db_daily = _load_db_daily()
    csv_daily = _load_csv_daily()
    return {
        "db_days": len(db_daily),
        "csv_days": len(csv_daily),
        "db_date_range": f"{db_daily['tarih'].min().date()} → {db_daily['tarih'].max().date()}" if not db_daily.empty else "Veri yok",
        "db_total_revenue": round(float(db_daily["toplam_ciro"].sum()), 2) if not db_daily.empty else 0,
        "model_exists": os.path.exists(MODEL_PATH),
    }


if __name__ == "__main__":
    print("XPos AI – Sales Forecast Training")
    print("=" * 45)
    model, scaler, daily = train_model()
    print(f"\n📈 Next 7 days forecast:")
    tahminler = predict_next_days(7)
    for t in tahminler:
        emoji = "🌧️" if t["hava"] in ("Yağmurlu", "Soğuk/Karlı") else "☀️"
        print(f"  {t['tarih']} {t['gun']:12s} {emoji} {t['hava']:12s} → ₺{t['tahmini_ciro']:>10,.2f}")
