
import pandas as pd
from mlxtend.frequent_patterns import apriori, association_rules
import json
import os

# Paths are relative to this script or passed in
BASE = os.path.dirname(os.path.abspath(__file__))
ML_DATA = os.path.join(BASE, "..", "..", "ml_data")

def train_recommendation_model(
    min_support=0.05, 
    max_support=0.60, 
    min_confidence=0.40, 
    min_lift=1.5
):
    """
    Trains the Apriori recommendation model using market_basket.csv.
    Generates association_rules.csv and menu_oneriler.json.
    """
    basket_path = os.path.join(ML_DATA, "market_basket.csv")
    rules_csv_path = os.path.join(ML_DATA, "association_rules.csv")
    oneriler_json_path = os.path.join(ML_DATA, "menu_oneriler.json")

    if not os.path.exists(basket_path):
        return {"status": "error", "message": f"market_basket.csv not found at {basket_path}"}

    try:
        # 1. Read Data
        df = pd.read_csv(basket_path, index_col=0)
        df = df.astype(bool)
        
        # 2. Apriori
        frequent_itemsets = apriori(
            df,
            min_support=min_support,
            use_colnames=True,
            verbose=0
        )
        
        # Filter too frequent items (water, etc.)
        frequent_itemsets = frequent_itemsets[frequent_itemsets['support'] <= max_support]
        
        if len(frequent_itemsets) == 0:
            return {"status": "error", "message": "No frequent itemsets found with current support settings."}

        # 3. Association Rules
        rules = association_rules(
            frequent_itemsets,
            metric="confidence",
            min_threshold=min_confidence
        )
        
        rules = rules[rules["lift"] >= min_lift].copy()
        rules = rules.sort_values("lift", ascending=False).reset_index(drop=True)

        if len(rules) == 0:
            return {"status": "error", "message": "No association rules found with current thresholds."}

        # Human readable strings
        rules["antecedents_str"] = rules["antecedents"].apply(lambda x: ", ".join(sorted(x)))
        rules["consequents_str"] = rules["consequents"].apply(lambda x: ", ".join(sorted(x)))

        # 4. Save CSV
        cikti_df = rules[["antecedents_str","consequents_str","support","confidence","lift","leverage","conviction"]]
        cikti_df.columns = ["antecedent","consequent","support","confidence","lift","leverage","conviction"]
        cikti_df = cikti_df.round(4)
        cikti_df.to_csv(rules_csv_path, index=False, encoding="utf-8")

        # 5. Save JSON (API Format)
        oneriler = []
        # We use a slightly higher confidence for the automated "Would you like X?" suggestions
        for _, row in rules[rules["confidence"] >= 0.55].iterrows():
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

        with open(oneriler_json_path, "w", encoding="utf-8") as f:
            json.dump(oneriler, f, ensure_ascii=False, indent=2)

        return {
            "status": "ok",
            "rules_count": len(rules),
            "high_conf_count": len(oneriler),
            "top_rule": f"{rules.iloc[0]['antecedents_str']} -> {rules.iloc[0]['consequents_str']}" if not rules.empty else None
        }

    except Exception as e:
        return {"status": "error", "message": str(e)}

if __name__ == "__main__":
    # Test run
    result = train_recommendation_model()
    print(result)
