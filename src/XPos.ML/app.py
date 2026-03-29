"""
XPos AI – Machine Learning Service (FastAPI)
=============================================
Port: 5001
Endpoints:
  GET /                              → Service health & status
  GET /api/recommendations?product=  → Product association rules (Apriori)
  GET /api/recommendations/basket    → Basket-based recommendations
  GET /api/forecast?days=7           → Sales forecasting (Linear Regression)
  GET /api/campaigns                 → Dynamic AI campaign engine
  GET /api/segments                  → Customer segmentation (K-Means)
  GET /api/stats                     → ML model statistics
"""

from fastapi import FastAPI, Query, Body
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import json
import os
import sqlite3
import pandas as pd
from typing import Optional, List
import random
from recommendation_engine import train_recommendation_model


app = FastAPI(
    title="XPos AI Service",
    description="XPos Intelligent Restaurant Analytics – Recommendations, Forecasting, Campaigns, Segmentation",
    version="2.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# ─── Data Paths ───
BASE = os.path.dirname(os.path.abspath(__file__))
ML_DATA = os.path.join(BASE, "..", "..", "ml_data")
ONERILER_PATH = os.path.join(ML_DATA, "menu_oneriler.json")
RULES_PATH = os.path.join(ML_DATA, "association_rules.csv")
DB_PATH = os.path.join(BASE, "..", "XPos.WebAPI", "XPosDb_v3.sqlite")
ACTIVE_CAMPAIGN_PATH = os.path.join(BASE, "models", "active_campaign.json")

# ─── Load Data ───
oneriler = []
rules_df = None
orders_df = None

def load_data():
    global oneriler, rules_df, orders_df
    if os.path.exists(ONERILER_PATH):
        with open(ONERILER_PATH, "r", encoding="utf-8") as f:
            oneriler = json.load(f)
    if os.path.exists(RULES_PATH):
        rules_df = pd.read_csv(RULES_PATH)
        
    # Veritabanından geçmiş sipariş verilerini canlı çekiyoruz:
    if os.path.exists(DB_PATH):
        try:
            conn = sqlite3.connect(DB_PATH)
            query = """
                SELECT 
                    Id as siparis_id,
                    TableNumber as qr_masa_id,
                    CreatedAt as tarih_saat,
                    STRFTIME('%w', CreatedAt) as gun,
                    STRFTIME('%H', CreatedAt) as saat,
                    STRFTIME('%m', CreatedAt) as ay,
                    WeatherCondition as hava_durumu,
                    Temperature as sicaklik_c,
                    TotalAmount as toplam_tutar
                FROM Orders
            """
            orders_df = pd.read_sql_query(query, conn)
            conn.close()
        except Exception as e:
            print("DB Okuma Hatası:", e)

load_data()


@app.get("/")
def root():
    return {
        "service": "XPos AI Service",
        "version": "2.0.0",
        "status": "healthy",
        "endpoints": [
            "/api/recommendations",
            "/api/forecast",
            "/api/campaigns",
            "/api/segments",
            "/api/stats"
        ],
        "models": {
            "apriori_rules": len(oneriler),
            "order_data_count": len(orders_df) if orders_df is not None else 0
        }
    }


# ═══════════════════════════════════════════
# 1. PRODUCT RECOMMENDATIONS (Apriori)
# ═══════════════════════════════════════════
@app.get("/api/recommendations")
def get_recommendations(
    product: str = Query(..., description="Product name, e.g.: Lahmacun"),
    limit: int = Query(5, description="Max recommendations")
):
    """Return association-based product recommendations."""
    results = []
    for o in oneriler:
        trigger = o["tetikleyici"].lower()
        if product.lower() in trigger:
            results.append({
                "trigger": o["tetikleyici"],
                "recommendation": o["oneri"],
                "confidence": o["confidence"],
                "lift": o["lift"],
                "message": o["oneri_metni"]
            })

    results.sort(key=lambda x: -x["confidence"])
    results = results[:limit]

    return {
        "product": product,
        "count": len(results),
        "recommendations": results
    }


@app.get("/api/recommendations/basket")
def get_basket_recommendations(
    products: str = Query(..., description="Comma-separated product names"),
    limit: int = Query(5)
):
    """Basket-based multi-product recommendations with fuzzy keyword matching."""

    # Keyword alias map: maps common food words to canonical forms
    KEYWORD_ALIASES = {
        "burger": ["hamburger", "cheeseburger", "smash"],
        "hamburger": ["burger", "cheeseburger", "smash"],
        "pizza": ["margherita", "pepperoni"],
        "kola": ["cola", "coca"],
        "çay": ["tea"],
        "kahve": ["coffee", "latte", "cappuccino", "espresso"],
        "patates": ["fries", "kızartma"],
        "salata": ["sezar", "akdeniz", "yunan"],
        "kebap": ["kebab", "adana", "urfa", "iskender"],
        "makarna": ["spagetti", "fettuccine", "penne", "bolonez"],
        "tavuk": ["chicken", "kanat"],
        "soğan halkası": ["onion ring"],
    }

    def extract_keywords(name):
        """Extract matchable keywords from a product name."""
        words = name.lower().replace("-", " ").split()
        keywords = set(words)
        # Add alias expansions
        for word in words:
            for key, aliases in KEYWORD_ALIASES.items():
                if word in aliases or word == key:
                    keywords.add(key)
                    keywords.update(aliases)
        return keywords

    product_list = [u.strip() for u in products.split(",")]
    product_keywords = set()
    for p in product_list:
        product_keywords.update(extract_keywords(p))

    results = []

    for o in oneriler:
        trigger = o["tetikleyici"].lower()
        rec = o["oneri"].lower()
        trigger_kw = extract_keywords(o["tetikleyici"])
        rec_kw = extract_keywords(o["oneri"])

        # Match if any product keyword overlaps with trigger keywords
        has_trigger_match = bool(product_keywords & trigger_kw)
        # Exclude if recommendation keywords overlap with cart product keywords
        rec_in_cart = bool(product_keywords & rec_kw)

        if has_trigger_match and not rec_in_cart:
            if not any(s["recommendation"].lower() == o["oneri"].lower() for s in results):
                results.append({
                    "trigger": o["tetikleyici"],
                    "recommendation": o["oneri"],
                    "confidence": o["confidence"],
                    "lift": o["lift"],
                    "message": o["oneri_metni"]
                })

    results.sort(key=lambda x: -x["lift"])
    return {
        "basket": products,
        "count": min(len(results), limit),
        "recommendations": results[:limit]
    }


@app.post("/api/recommendations/retrain")
def retrain_recommendations(
    min_support: float = Query(0.05),
    min_confidence: float = Query(0.40)
):
    """Re-train Apriori association rules with latest market basket data."""
    try:
        result = train_recommendation_model(
            min_support=min_support,
            min_confidence=min_confidence
        )
        if result["status"] == "ok":
            load_data()  # Refresh global oneriler and rules_df
            return {
                "status": "ok",
                "message": "Öneri modeli başarıyla güncellendi",
                "stats": result
            }
        else:
            return {"status": "error", "message": result.get("message", "Unknown error")}
    except Exception as e:
        return {"status": "error", "message": str(e)}


# ── Smart Basket Recommendation (POST) ──
class AvailableProduct(BaseModel):
    id: int
    name: str
    price: float
    categoryId: int

class SmartBasketRequest(BaseModel):
    cart_products: List[AvailableProduct]
    available_products: List[AvailableProduct]
    limit: int = 6

# Category-based complementary recommendation rules
# categoryId → list of complementary categoryIds (with weights)
CATEGORY_COMPLEMENTS = {
    1: [(7, 0.9), (3, 0.7), (4, 0.7), (5, 0.6)],   # Başlangıçlar → Drinks, Burgers, Pizza, Mains
    2: [(7, 0.9), (1, 0.7), (8, 0.5)],               # Salatalar → Drinks, Starters, Desserts
    3: [(1, 0.95), (7, 0.9), (8, 0.6), (2, 0.4)],    # Burgerler → Starters(sides), Drinks, Desserts, Salads
    4: [(2, 0.85), (7, 0.9), (8, 0.5), (1, 0.6)],    # Pizzalar → Salads, Drinks, Desserts, Starters
    5: [(1, 0.85), (7, 0.9), (8, 0.6), (2, 0.5)],    # Ana Yemekler → Starters, Drinks, Desserts, Salads
    6: [(2, 0.85), (7, 0.9), (8, 0.5), (1, 0.5)],    # Makarnalar → Salads, Drinks, Desserts, Starters
    7: [(8, 0.85), (1, 0.6)],                         # İçecekler → Desserts, Starters
    8: [(7, 0.9)],                                     # Tatlılar → Drinks
}

# Popular pairings: specific product keyword → recommended category + boost
SMART_PAIRINGS = {
    "burger": [(1, "Patates Kızartması", 0.95), (1, "Soğan Halkası", 0.80), (7, None, 0.85)],
    "cheeseburger": [(1, "Patates Kızartması", 0.95), (1, "Soğan Halkası", 0.80), (7, None, 0.85)],
    "smash": [(1, "Patates Kızartması", 0.95), (1, "Soğan Halkası", 0.80), (7, None, 0.85)],
    "pizza": [(2, None, 0.80), (7, None, 0.85)],
    "margherita": [(2, None, 0.80), (7, None, 0.85)],
    "pepperoni": [(2, None, 0.80), (7, None, 0.85)],
    "köfte": [(1, "Patates Kızartması", 0.85), (7, None, 0.80), (2, None, 0.6)],
    "kebap": [(1, None, 0.80), (7, "Şalgam", 0.90), (7, None, 0.75)],
    "antrikot": [(2, None, 0.80), (7, None, 0.85), (1, "Patates Kızartması", 0.75)],
    "wrap": [(7, None, 0.80), (1, "Patates Kızartması", 0.75)],
    "makarna": [(2, None, 0.80), (7, None, 0.75)],
    "spagetti": [(2, None, 0.80), (7, None, 0.75)],
    "fettuccine": [(2, None, 0.80), (7, None, 0.75)],
    "penne": [(2, None, 0.80), (7, None, 0.75)],
    "salata": [(7, None, 0.80), (8, None, 0.50)],
    "çorba": [(3, None, 0.70), (5, None, 0.70), (7, None, 0.60)],
    "kahve": [(8, None, 0.90)],
    "latte": [(8, None, 0.90)],
    "cappuccino": [(8, None, 0.90)],
    "çay": [(8, None, 0.85)],
    "limonata": [(8, None, 0.75), (1, None, 0.50)],
    "cheesecake": [(7, "Türk Kahvesi", 0.85), (7, None, 0.80)],
    "sütlaç": [(7, "Türk Kahvesi", 0.80), (7, None, 0.75)],
    "tiramisu": [(7, "Latte", 0.85), (7, None, 0.80)],
    "brownie": [(7, None, 0.80)],
    "waffle": [(7, None, 0.80)],
}

# Keyword alias map for fuzzy matching
KEYWORD_ALIASES_MAP = {
    "burger": "burger", "hamburger": "burger", "cheeseburger": "burger", "smash": "burger",
    "pizza": "pizza", "margherita": "pizza", "pepperoni": "pizza",
    "kahve": "kahve", "coffee": "kahve", "espresso": "kahve",
    "patates": "patates", "fries": "patates", "kızartma": "patates",
    "salata": "salata", "sezar": "salata", "akdeniz": "salata", "yunan": "salata",
    "makarna": "makarna", "spagetti": "makarna", "fettuccine": "makarna", "penne": "makarna", "bolonez": "makarna",
    "tavuk": "tavuk", "chicken": "tavuk", "kanat": "tavuk",
}


@app.post("/api/recommendations/smart-basket")
def smart_basket_recommendations(req: SmartBasketRequest):
    print(f"\n🔍 AI DEBUG: Sepet Önerisi İsteği Alındı")
    print(f"   Sepetteki Ürünler: {[p.name for p in req.cart_products]}")
    
    """
    Smart cart-aware recommendations using:
    1. Category-based complementary matching (primary)
    2. Specific product pairing rules (boost)
    3. Apriori association rules (additional boost)
    Returns actual products from the available_products list.
    """
    cart_ids = {p.id for p in req.cart_products}
    cart_names_lower = {p.name.lower() for p in req.cart_products}
    cart_categories = {p.categoryId for p in req.cart_products}

    # Build candidate pool: all available products NOT in cart
    candidates = [p for p in req.available_products if p.id not in cart_ids]

    if not candidates:
        return {"recommendations": [], "count": 0}

    # Score each candidate
    scored = {}  # product_id → {product, score, reason}

    # ── Phase 1: Category complement scoring ──
    for cart_item in req.cart_products:
        complements = CATEGORY_COMPLEMENTS.get(cart_item.categoryId, [])
        for comp_cat, weight in complements:
            for cand in candidates:
                if cand.categoryId == comp_cat:
                    if cand.id not in scored:
                        scored[cand.id] = {"product": cand, "score": 0.0, "reasons": []}
                    scored[cand.id]["score"] += weight * 0.5
                    scored[cand.id]["reasons"].append(f"complement-to-{cart_item.name}")

    # ── Phase 2: Smart pairing boost ──
    for cart_item in req.cart_products:
        cart_words = cart_item.name.lower().replace("-", " ").split()
        for word in cart_words:
            pairings = SMART_PAIRINGS.get(word, [])
            for target_cat, target_name, boost in pairings:
                for cand in candidates:
                    if cand.categoryId == target_cat:
                        if target_name is None or target_name.lower() in cand.name.lower():
                            if cand.id not in scored:
                                scored[cand.id] = {"product": cand, "score": 0.0, "reasons": []}
                            scored[cand.id]["score"] += boost * 0.7
                            scored[cand.id]["reasons"].append(f"pairs-with-{word}")

    # ── Phase 3: Apriori rule boost ──
    KEYWORD_ALIASES = {
        "burger": ["hamburger", "cheeseburger", "smash"],
        "hamburger": ["burger", "cheeseburger", "smash"],
        "pizza": ["margherita", "pepperoni"],
        "kahve": ["coffee", "latte", "cappuccino", "espresso"],
        "patates": ["fries", "kızartma"],
        "salata": ["sezar", "akdeniz", "yunan"],
        "makarna": ["spagetti", "fettuccine", "penne", "bolonez"],
        "tavuk": ["chicken", "kanat"],
    }

    def expand_keywords(name):
        words = name.lower().replace("-", " ").split()
        kw = set(words)
        for w in words:
            for key, aliases in KEYWORD_ALIASES.items():
                if w in aliases or w == key:
                    kw.add(key)
                    kw.update(aliases)
        return kw

    cart_keywords = set()
    for p in req.cart_products:
        cart_keywords.update(expand_keywords(p.name))

    for o in oneriler:
        trigger_kw = expand_keywords(o["tetikleyici"])
        rec_kw = expand_keywords(o["oneri"])

        if cart_keywords & trigger_kw and not (cart_keywords & rec_kw):
            # This rule's recommendation is relevant
            # Find matching candidates
            rec_words = rec_kw
            for cand in candidates:
                cand_kw = expand_keywords(cand.name)
                if cand_kw & rec_words:
                    if cand.id not in scored:
                        scored[cand.id] = {"product": cand, "score": 0.0, "reasons": []}
                    scored[cand.id]["score"] += o["confidence"] * 0.8
                    scored[cand.id]["reasons"].append(f"apriori:{o['tetikleyici']}")

    # ── Phase 4: If still very few results, add popular items from missing categories ──
    if len(scored) < req.limit:
        missing_cats = {1, 2, 3, 4, 5, 6, 7, 8} - cart_categories
        for cand in candidates:
            if cand.id not in scored and cand.categoryId in missing_cats:
                scored[cand.id] = {"product": cand, "score": 0.15, "reasons": ["popular-fill"]}

    # ── Phase 5: Penalize same-category-as-cart items for MEAL categories ──
    # Meal categories where duplicates don't make sense
    MEAL_CATEGORIES = {3, 4, 5, 6}  # Burgers, Pizzas, Mains, Pasta
    # If cart already has a meal, heavily penalize other meal-category items
    cart_has_meal = bool(cart_categories & MEAL_CATEGORIES)
    for item_data in scored.values():
        prod_cat = item_data["product"].categoryId
        if prod_cat in MEAL_CATEGORIES:
            if prod_cat in cart_categories:
                # Same meal category (e.g., another burger) → very heavy penalty
                item_data["score"] *= 0.1
            elif cart_has_meal:
                # Different meal category (e.g., pizza when burger in cart) → moderate penalty
                item_data["score"] *= 0.25

    # ── Sort by score, diversify by category ──
    sorted_items = sorted(scored.values(), key=lambda x: -x["score"])

    # Pick top items with category diversity
    result = []
    category_count = {}
    for item in sorted_items:
        cat = item["product"].categoryId
        # Max 2 items from same category
        if category_count.get(cat, 0) >= 2:
            continue
        category_count[cat] = category_count.get(cat, 0) + 1

        # Normalize confidence to 0.5-0.95 range
        max_score = sorted_items[0]["score"] if sorted_items else 1
        norm_conf = min(0.95, max(0.50, item["score"] / max(max_score, 0.01) * 0.95))

        result.append({
            "ProductId": item["product"].id,
            "Name": item["product"].name,
            "Price": item["product"].price,
            "CategoryId": item["product"].categoryId,
            "Confidence": round(norm_conf, 2),
            "Reason": item["reasons"][0] if item["reasons"] else "popular"
        })

        if len(result) >= req.limit:
            break

    return {
        "Recommendations": result,
        "Count": len(result)
    }


# ═══════════════════════════════════════════
# 2. SALES FORECAST
# ═══════════════════════════════════════════
@app.get("/api/forecast")
def get_forecast(
    days: int = Query(7, description="Forecast days ahead (1-30)"),
    weather: Optional[str] = Query(None, description="Comma-separated weather list")
):
    """Sales forecast for the next N days using Linear Regression."""
    from sales_forecast import predict_next_days, train_model
    import os

    # Auto-train if model doesn't exist
    MODEL_CHECK = os.path.join(os.path.dirname(__file__), "models", "sales_model.pkl")
    if not os.path.exists(MODEL_CHECK):
        train_model()

    weather_list = None
    if weather:
        weather_list = [h.strip() for h in weather.split(",")]

    predictions = predict_next_days(days=min(days, 90), hava_listesi=weather_list)

    total = sum(t["tahmini_ciro"] for t in predictions)
    avg = total / len(predictions) if predictions else 0

    forecasts = []
    for t in predictions:
        forecasts.append({
            "date": t.get("tarih", ""),
            "day": t.get("gun", ""),
            "weather": t.get("hava", ""),
            "temperature": t.get("sicaklik", 0),
            "predicted_revenue": t.get("tahmini_ciro", 0),
            "is_weekend": t.get("hafta_sonu", t.get("is_weekend", False))
        })

    return {
        "forecast_days": len(forecasts),
        "total_predicted_revenue": round(total, 2),
        "daily_average": round(avg, 2),
        "forecasts": forecasts
    }


@app.post("/api/forecast/retrain")
def retrain_forecast():
    """Re-train forecast model with latest DB + CSV data."""
    from sales_forecast import train_model, get_training_stats
    try:
        model, scaler, daily = train_model()
        stats = get_training_stats()
        return {
            "status": "ok",
            "message": "Model yeniden eğitildi",
            "training_rows": len(daily),
            "r2_score": round(float(model.score(
                scaler.transform(daily[["gun_no", "ay", "hafta_sonu", "hava_kod", "sicaklik",
                                        "gun_sin", "gun_cos", "ay_sin", "ay_cos"]].values),
                daily["toplam_ciro"].values,
                sample_weight=daily["weight"].values
            )), 4),
            "data_sources": stats
        }
    except Exception as e:
        return {"status": "error", "message": str(e)}


@app.get("/api/forecast/stats")
def forecast_stats():
    """Return info about forecast training data sources."""
    from sales_forecast import get_training_stats
    try:
        return get_training_stats()
    except Exception as e:
        return {"error": str(e)}


# ═══════════════════════════════════════════
# 2b. DASHBOARD PRODUCT RECOMMENDATIONS
# ═══════════════════════════════════════════
class DashboardRecoRequest(BaseModel):
    productId: int
    productName: str
    categoryId: int
    allProducts: List[AvailableProduct]
    limit: int = 8

@app.post("/api/recommendations/dashboard")
def dashboard_recommendations(req: DashboardRecoRequest):
    """
    Product-aware recommendations for the Dashboard 'Ürün Önerileri' page.
    Combines: category complements + smart pairings + Apriori fuzzy match.
    Works with actual DB products.
    """
    candidates = [p for p in req.allProducts if p.id != req.productId]
    if not candidates:
        return {"product": req.productName, "count": 0, "recommendations": []}

    scored = {}

    # ── Phase 1: Category complement ──
    complements = CATEGORY_COMPLEMENTS.get(req.categoryId, [])
    for comp_cat, weight in complements:
        for cand in candidates:
            if cand.categoryId == comp_cat:
                if cand.id not in scored:
                    scored[cand.id] = {"product": cand, "score": 0.0, "reasons": []}
                scored[cand.id]["score"] += weight * 0.5
                scored[cand.id]["reasons"].append("Kategori tamamlayıcı")

    # ── Phase 2: Smart keyword pairing ──
    words = req.productName.lower().replace("-", " ").split()
    for word in words:
        canonical = KEYWORD_ALIASES_MAP.get(word, word)
        pairings = SMART_PAIRINGS.get(canonical, [])
        if not pairings:
            pairings = SMART_PAIRINGS.get(word, [])
        for target_cat, target_name, boost in pairings:
            for cand in candidates:
                if cand.categoryId == target_cat:
                    name_match = target_name and target_name.lower() in cand.name.lower()
                    if cand.id not in scored:
                        scored[cand.id] = {"product": cand, "score": 0.0, "reasons": []}
                    extra = boost * (0.6 if name_match else 0.3)
                    scored[cand.id]["score"] += extra
                    if name_match:
                        scored[cand.id]["reasons"].append(f"'{req.productName}' ile eşleşme")
                    else:
                        scored[cand.id]["reasons"].append("Akıllı eşleştirme")

    # ── Phase 3: Apriori boost (fuzzy) ──
    product_lower = req.productName.lower()
    for o in oneriler:
        trigger = o["tetikleyici"].lower()
        if any(w in trigger for w in words if len(w) > 2):
            rec_names = [r.strip().lower() for r in o["oneri"].split(",")]
            for cand in candidates:
                cand_lower = cand.name.lower()
                for rn in rec_names:
                    if rn in cand_lower or cand_lower in rn or any(
                        rw in cand_lower for rw in rn.split() if len(rw) > 3
                    ):
                        if cand.id not in scored:
                            scored[cand.id] = {"product": cand, "score": 0.0, "reasons": []}
                        scored[cand.id]["score"] += o["confidence"] * 0.4
                        scored[cand.id]["reasons"].append(f"Apriori (güven: %{int(o['confidence']*100)})")
                        break

    # ── Phase 4: Fill with popular from different categories ──
    if len(scored) < req.limit:
        for cand in candidates:
            if cand.id not in scored and cand.categoryId != req.categoryId:
                scored[cand.id] = {"product": cand, "score": 0.05, "reasons": ["Popüler ürün"]}

    # Sort and build response
    ranked = sorted(scored.values(), key=lambda x: -x["score"])[:req.limit]

    recs = []
    for item in ranked:
        p = item["product"]
        score = min(item["score"], 1.0)
        recs.append({
            "productId": p.id,
            "productName": p.name,
            "price": p.price,
            "categoryId": p.categoryId,
            "confidence": round(score, 3),
            "reason": item["reasons"][0] if item["reasons"] else "Öneri"
        })

    return {
        "product": req.productName,
        "count": len(recs),
        "recommendations": recs
    }


# ═══════════════════════════════════════════
# 3. AI CAMPAIGNS
# ═══════════════════════════════════════════

def _raw_to_campaign(c):
    """Convert raw campaign dict to API response format."""
    priority_map = {"yuksek": 1, "orta": 2, "dusuk": 3}
    return {
        "id": c["id"],
        "title": c["baslik"],
        "description": c["aciklama"],
        "discountPercent": c["indirim_yuzde"],
        "targetCategories": c["hedef_kategoriler"],
        "condition": c["kosul"],
        "priority": priority_map.get(c["oncelik"], 3),
        "color": c["renk"]
    }


def _load_active_campaign():
    if os.path.exists(ACTIVE_CAMPAIGN_PATH):
        with open(ACTIVE_CAMPAIGN_PATH, "r", encoding="utf-8") as f:
            return json.load(f)
    return None


def _save_active_campaign(campaign):
    os.makedirs(os.path.dirname(ACTIVE_CAMPAIGN_PATH), exist_ok=True)
    with open(ACTIVE_CAMPAIGN_PATH, "w", encoding="utf-8") as f:
        json.dump(campaign, f, ensure_ascii=False, indent=2)


def _clear_active_campaign():
    if os.path.exists(ACTIVE_CAMPAIGN_PATH):
        os.remove(ACTIVE_CAMPAIGN_PATH)


class CampaignActivateRequest(BaseModel):
    id: str
    title: str
    description: str
    discountPercent: int
    targetCategories: List[str]
    condition: str
    priority: int
    color: str


@app.get("/api/campaigns/suggest")
def suggest_campaign(
    weather: str = Query("Güneşli"),
    temperature: int = Query(20),
    hour: Optional[int] = Query(None)
):
    """Get a single AI campaign suggestion (the best one for current conditions)."""
    from campaign_engine import get_top_campaign
    raw = get_top_campaign(hava=weather, sicaklik=temperature, saat=hour)
    if not raw:
        return {"suggestion": None}
    return {"suggestion": _raw_to_campaign(raw)}


@app.post("/api/campaigns/activate")
def activate_campaign(data: CampaignActivateRequest):
    """Activate a campaign — makes it visible to customers and applies discounts."""
    campaign = data.dict()
    _save_active_campaign(campaign)
    return {"status": "activated", "campaign": campaign}


@app.get("/api/campaigns/active")
def get_active_campaign():
    """Get the currently active campaign (used by customer QR menu)."""
    campaign = _load_active_campaign()
    return {"active": campaign is not None, "campaign": campaign}


@app.post("/api/campaigns/deactivate")
def deactivate_campaign():
    """Deactivate the current campaign."""
    _clear_active_campaign()
    return {"status": "deactivated"}


@app.get("/api/campaigns")
def get_campaigns_endpoint(
    weather: str = Query("Güneşli", description="Current weather"),
    temperature: int = Query(20, description="Temperature (°C)"),
    hour: Optional[int] = Query(None, description="Hour (0-23)")
):
    """AI-powered dynamic campaign suggestions based on context (legacy/dashboard home)."""
    from campaign_engine import get_campaigns
    raw = get_campaigns(hava=weather, sicaklik=temperature, saat=hour)
    campaigns = [_raw_to_campaign(c) for c in raw]
    return {
        "weather": weather,
        "temperature": temperature,
        "campaignCount": len(campaigns),
        "campaigns": campaigns
    }


# ═══════════════════════════════════════════
# 4. CUSTOMER SEGMENTATION (K-Means)
# ═══════════════════════════════════════════
@app.get("/api/segments")
def get_segments_endpoint():
    """K-Means customer segmentation results."""
    from segmentation import get_segments
    raw = get_segments()

    segments = []
    for s in raw.get("segmentler", []):
        segments.append({
            "id": s["id"],
            "name": s["ad"],
            "color": s["renk"],
            "icon": s["ikon"],
            "description": s["aciklama"],
            "customer_count": s["musteri_sayisi"],
            "percentage": s["yuzde"],
            "avg_spend": s["ortalama_tutar"],
            "avg_group_size": s["ortalama_kisi"],
            "avg_items": s["ortalama_urun"],
            "total_revenue": s["toplam_ciro"],
            "weekend_ratio": s["hafta_sonu_orani"],
            "peak_hour": s["en_yogun_saat"]
        })

    return {
        "total_orders": raw.get("toplam_siparis", 0),
        "segment_count": raw.get("segment_sayisi", 0),
        "segments": segments
    }


# ═══════════════════════════════════════════
# 5. STATISTICS
# ═══════════════════════════════════════════
@app.get("/api/stats")
def get_stats():
    """Overall ML model statistics."""
    stats = {
        "apriori": {
            "total_rules": len(oneriler),
            "high_confidence": len([o for o in oneriler if o["confidence"] >= 0.7]),
            "best_rule": oneriler[0] if oneriler else None
        }
    }

    if orders_df is not None:
        stats["order_data"] = {
            "total_orders": len(orders_df),
            "total_revenue": round(float(orders_df["toplam_tutar"].sum()), 2),
            "avg_basket": round(float(orders_df["toplam_tutar"].mean()), 2),
            "date_range": f"{orders_df['tarih_saat'].min()} → {orders_df['tarih_saat'].max()}"
        }

        from collections import Counter
        all_products = []
        for content in orders_df["siparis_icerigi"].dropna():
            for part in content.split(", "):
                parts = part.strip().split(" ", 1)
                if len(parts) == 2 and parts[0].isdigit():
                    all_products.append(parts[1])
        freq = Counter(all_products).most_common(10)
        stats["top_products"] = [{"product": p, "count": c} for p, c in freq]

    return stats


# ─── Legacy endpoint compatibility (Türkçe → İngilizce redirect) ───
@app.get("/oneriler")
def legacy_oneriler(urun: str = Query(...), limit: int = Query(5)):
    return get_recommendations(product=urun, limit=limit)

@app.get("/tahmin")
def legacy_tahmin(gun: int = Query(7), hava: Optional[str] = Query(None)):
    return get_forecast(days=gun, weather=hava)

@app.get("/kampanya")
def legacy_kampanya(hava: str = Query("Güneşli"), sicaklik: int = Query(20), saat: Optional[int] = Query(None)):
    return get_campaigns_endpoint(weather=hava, temperature=sicaklik, hour=saat)

@app.get("/segment")
def legacy_segment():
    return get_segments_endpoint()

@app.get("/istatistik")
def legacy_istatistik():
    return get_stats()


if __name__ == "__main__":
    import uvicorn
    print("🚀 XPos AI Service starting → http://localhost:5001")
    uvicorn.run(app, host="0.0.0.0", port=5001)
