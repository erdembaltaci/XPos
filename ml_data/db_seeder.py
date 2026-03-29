import sqlite3
import random
from datetime import datetime, timedelta

def main():
    print("Veritabanı için geçmiş 2000 sipariş üretiliyor...")
    db_path = r"c:\XPos\src\XPos.WebAPI\XPosDb_v3.sqlite"
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()

    # Ürünleri çek
    cursor.execute("SELECT Id, Name, Price FROM Products")
    products = cursor.fetchall()
    
    if len(products) == 0:
        print("Ürün bulunamadı!")
        return

    # Örnek ağırlıklar (bazı ürünler daha sık sipariş edilsin)
    weights = [random.randint(1, 10) for _ in products]
    
    hava_durumlari = ["Güneşli", "Bulutlu", "Yağmurlu", "Karlı", "Sağanak Yağışlı", "Kapalı"]
    
    baslangic = datetime(2026, 1, 1)
    bitis = datetime(2026, 3, 29)
    toplam_gun = (bitis - baslangic).days

    orders_to_insert = []
    order_items_to_insert = []
    
    # Mevcut en büyük OrderId yi alalım
    cursor.execute("SELECT MAX(Id) FROM Orders")
    max_id_row = cursor.fetchone()
    current_order_id = max_id_row[0] if max_id_row[0] is not None else 0

    for i in range(2000):
        current_order_id += 1
        
        # Rastgele tarih (Geçmiş 90 gün)
        gun_offset = random.randint(0, toplam_gun)
        saat = random.randint(8, 23)
        dakika = random.randint(0, 59)
        tarih = baslangic + timedelta(days=gun_offset, hours=saat, minutes=dakika)
        
        masa_no = f"Masa {random.randint(1, 20)}"
        hava = random.choice(hava_durumlari)
        sıcaklik = random.randint(-5, 35)
        
        # 1-5 arası rastgele ürün sayısı
        urun_sayisi = random.randint(1, 5)
        secilen_urunler = random.choices(products, weights=weights, k=urun_sayisi)
        
        toplam_tutar = 0
        for p in set(secilen_urunler):
            miktar = secilen_urunler.count(p)
            fiyat = p[2]
            toplam_tutar += fiyat * miktar
            
            # OrderItems listesine ekle
            order_items_to_insert.append((
                current_order_id, # OrderId
                p[0],            # ProductId
                miktar,          # Quantity
                fiyat,           # UnitPrice
                "",              # Note
                1                # ItemStatus (1=Pending/Preparing vs)
            ))
            
        # Orders listesine ekle
        orders_to_insert.append((
            current_order_id,
            masa_no,
            1, # Status = Paid vs (genelde idsi) aslında OrderStatus enum 3 diyelim (Paid)
            toplam_tutar,
            tarih.strftime("%Y-%m-%d %H:%M:%S.0000000"),
            None, # WaiterId
            toplam_tutar, # PaidAmount
            hava,
            sıcaklik
        ))

    # Toplu ekleme yap (Veritabanı id çakışmasını önlemek için id belirttik)
    cursor.executemany("""
        INSERT INTO Orders (Id, TableNumber, Status, TotalAmount, CreatedAt, WaiterId, PaidAmount, WeatherCondition, Temperature)
        VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
    """, orders_to_insert)

    cursor.executemany("""
        INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Note, ItemStatus)
        VALUES (?, ?, ?, ?, ?, ?)
    """, order_items_to_insert)

    conn.commit()
    conn.close()
    
    print(f"Başarıyla {len(orders_to_insert)} geçmiş sipariş ve {len(order_items_to_insert)} sipariş detayı oluşturuldu!")

if __name__ == "__main__":
    main()
