import sqlite3
import pandas as pd
import os

DB_PATH = r"c:\XPos\src\XPos.WebAPI\XPosDb_v3.sqlite"
ML_DATA = r"c:\XPos\ml_data"

def sync():
    if not os.path.exists(ML_DATA): os.makedirs(ML_DATA)
    conn = sqlite3.connect(DB_PATH)

    # 1. Market Basket Data (One-Hot Encoded)
    print("--- Syncing Market Basket Data ---")
    query = """
        SELECT 
            oi.OrderId,
            p.Name as ProductName
        FROM OrderItems oi
        JOIN Products p ON oi.ProductId = p.Id
    """
    df_items = pd.read_sql_query(query, conn)
    
    # Pivot to one-hot
    basket = df_items.groupby(['OrderId', 'ProductName'])['ProductName'].count().unstack().reset_index().fillna(0).set_index('OrderId')
    basket = basket.map(lambda x: 1 if x > 0 else 0)
    
    basket_path = os.path.join(ML_DATA, "market_basket.csv")
    basket.to_csv(basket_path)
    print(f"✅ market_basket.csv created ({len(basket)} orders)")

    # 2. Orders Summary (For Segmentation)
    print("--- Syncing Orders Summary ---")
    query_orders = """
        SELECT 
            o.Id as siparis_id,
            o.TotalAmount as toplam_tutar,
            o.CustomerName as musteri_adi,
            o.CustomerPhone as musteri_tel,
            STRFTIME('%H', o.CreatedAt) as saat,
            STRFTIME('%w', o.CreatedAt) as gun_no,
            o.WeatherCondition as hava_durumu,
            o.Temperature as sicaklik_c,
            (SELECT COUNT(*) FROM OrderItems WHERE OrderId = o.Id) as urun_sayisi
        FROM Orders o
    """
    df_orders = pd.read_sql_query(query_orders, conn)
    
    # Simple mapping
    df_orders['saat'] = pd.to_numeric(df_orders['saat'])
    df_orders['gun_no'] = pd.to_numeric(df_orders['gun_no'])
    df_orders['hafta_sonu'] = df_orders['gun_no'].isin([0, 6]).astype(int) # SQLite 0=Sun, 6=Sat
    df_orders['kisi_sayisi'] = df_orders['urun_sayisi'].apply(lambda x: max(1, x // 2)) # Estimated
    
    # Categorize Age/Weather (Simulated mapping)
    df_orders['yas_grubu'] = "25-34" # Default
    
    summary_path = os.path.join(ML_DATA, "orders_summary.csv")
    df_orders.to_csv(summary_path, index=False)
    print(f"✅ orders_summary.csv created ({len(df_orders)} rows)")

    conn.close()

if __name__ == "__main__":
    sync()
