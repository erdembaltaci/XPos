import sqlite3
import random
from datetime import datetime, timedelta
import os

DB_PATH = r"c:\XPos\src\XPos.WebAPI\XPosDb_v3.sqlite"

# 1. Categories & Products Setup
# 1: Başlangıç, 2: Salata, 3: Burger, 4: Pizza, 5: Ana Yemek, 6: Makarna, 7: İçecek, 8: Tatlı, 9: Kahvaltı, 10: Çorba, 11: Kebap

PRODUCTS = [
    # Çorbalar (10)
    (1, "Mercimek Çorbası", "Limon ve kıtır ekmek ile", 85, 10, "https://images.unsplash.com/photo-1547592166-23ac45744acd?w=500"),
    (2, "Ezogelin Çorbası", "Geleneksel Anadolu lezzeti", 85, 10, "https://images.unsplash.com/photo-1603105076183-0262490c0420?w=500"),
    (3, "Domates Çorbası", "Rendelenmiş kaşar ve kruton ile", 90, 10, "https://images.unsplash.com/photo-1547592166-23ac45744acd?w=500"),
    
    # Başlangıçlar (1)
    (4, "Paçanga Böreği", "Pastırmalı, kaşarlı, kapya biberli", 145, 1, "https://images.unsplash.com/photo-1608794862145-fe64746f3223?w=500"),
    (5, "İçli Köfte (2 Adet)", "Cevizli ve nar ekşili kıyma dolgulu", 180, 1, "https://plus.unsplash.com/premium_photo-1695297514332-9017646a782e?w=500"),
    (6, "Patates Kızartması", "Özel baharatlı ve yanında dip soslar", 110, 1, "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=500"),
    (7, "Soğan Halkası (8 Adet)", "Çıtır panelenmiş soğan halkaları", 105, 1, "https://images.unsplash.com/photo-1639024471283-035188801981?w=500"),
    (8, "Sigara Böreği", "Lor peynirli ve maydanozlu", 95, 1, "https://images.unsplash.com/photo-1608794862145-fe64746f3223?w=500"),
    
    # Salatalar (2)
    (9, "Çoban Salatası", "Domates, salatalık, soğan, biber", 120, 2, "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=500"),
    (10, "Sezar Salata", "Izgara tavuk, kruton ve parmesan", 240, 2, "https://images.unsplash.com/photo-1550304943-4f24f54ddde9?w=500"),
    (11, "Akdeniz Salatası", "Mısır, peynir ve özel sos ile", 165, 2, "https://images.unsplash.com/photo-1540189549336-e6e99c3679fe?w=500"),
    
    # Burgerler (3)
    (12, "Klasik Cheeseburger", "150g köfte, cheddar, karamelize soğan", 320, 3, "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500"),
    (13, "Double Smash Burger", "2x80g köfte, cheddar, özel turşu", 410, 3, "https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=500"),
    (14, "Mantar Soslu Burger", "Kremalı mantar sosu ve swiss peyniri", 360, 3, "https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=500"),
    (15, "Çıtır Tavuk Burger", "Özel paneli tavuk göğsü", 280, 3, "https://images.unsplash.com/photo-1525059696038-2312f9fe5427?w=500"),
    
    # Pizzalar (4)
    (16, "Margarita Pizza", "İtalyan mozarella ve taze fesleğen", 290, 4, "https://images.unsplash.com/photo-1574071318508-1cdbad80ad50?w=500"),
    (17, "Karışık Pizza", "Sucuk, mısır, zeytin, biber", 380, 4, "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=500"),
    (18, "Pepperoni Pizza", "Bol dana sucuklu ve mozarella", 360, 4, "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=500"),
    (19, "Sebzeli Pizza", "Mantar, biber, kabak, mısır", 310, 4, "https://images.unsplash.com/photo-1571407970349-bc81e7e96d47?w=500"),
    
    # Kebaplar (11)
    (20, "Adana Kebap", "Acılı zırh kıyması, sumaklı soğan", 450, 11, "https://images.unsplash.com/photo-1633321702518-7feccafacdc4?w=500"),
    (21, "Urfa Kebap", "Acısız zırh kıyması", 450, 11, "https://images.unsplash.com/photo-1599487488170-d11ec9c172f0?w=500"),
    (22, "İskender Kebap", "Pide üzeri döner, tereyağı, yoğurt", 510, 11, "https://images.unsplash.com/photo-1662116765994-1e22067409bd?w=500"),
    (23, "Beyti Sarma", "Lavaş arası kıyma, özel soslu", 530, 11, "https://images.unsplash.com/photo-1619983081563-430f63602796?w=500"),
    (24, "Kuzu Şiş", "Biber ve domates ile közlenmiş", 520, 11, "https://images.unsplash.com/photo-1544124499-58d356dc7a51?w=500"),
    (25, "Ciğer Şiş", "Güneydoğu usulü küçük doğranmış", 410, 11, "https://images.unsplash.com/photo-1633321088355-d0f81134ca3b?w=500"),
    
    # Ana Yemekler (5)
    (26, "Dana Antrikot", "220g ızgara antrikot, püre ile", 650, 5, "https://images.unsplash.com/photo-1546241072-48010ad10h1d?w=500"),
    (27, "Köri Soslu Tavuk", "Pilav ve patates eşliğinde", 390, 5, "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?w=500"),
    (28, "Tavuk Schnitzel", "Viyana usulü çıtır panelenmiş", 340, 5, "https://images.unsplash.com/photo-1594946291022-de9db43444c1?w=500"),
    (29, "Mantarlı Biftek", "Kremalı soslu dana bonfile", 610, 5, "https://images.unsplash.com/photo-1558030006-450675393462?w=500"),
    
    # Makarnalar (6)
    (30, "Penne Arrabbiata", "Acılı domates sosu ve fesleğen", 280, 6, "https://images.unsplash.com/photo-1551183053-bf91a1d81141?w=500"),
    (31, "Fettuccine Alfredo", "Tavuk dilimleri, krema, mantar", 310, 6, "https://images.unsplash.com/photo-1645112481338-3162ef9623e1?w=500"),
    (32, "Spagetti Bolonez", "Klasik kıymalı domates sosu", 295, 6, "https://images.unsplash.com/photo-1598866539627-9a6983731cbc?w=500"),
    
    # Tatlılar (8)
    (33, "Künefe", "Antep fıstıklı ve sıcak şerbetli", 220, 8, "https://images.unsplash.com/photo-1614707166646-cd92518320b9?w=500"),
    (34, "Fıstıklı Baklava (4 Dilim)", "Gaziantep usulü çıtır baklava", 240, 8, "https://images.unsplash.com/photo-1519676867240-f03562e64548?w=500"),
    (35, "San Sebastian Cheesecake", "Yanık dış yüzey, akışkan iç", 195, 8, "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=500"),
    (36, "Sıcak Brownie", "Belçika çikolatalı, dondurmalı", 210, 8, "https://images.unsplash.com/photo-1564355808539-22fda35bcd36?w=500"),
    (37, "Fırın Sütlaç", "Üstü kızarmış geleneksel yöntem", 140, 8, "https://images.unsplash.com/photo-1605274415843-69062319080b?w=500"),
    (38, "Profiterol", "Özel çikolata sosu ve krema dolgulu", 175, 8, "https://plus.unsplash.com/premium_photo-1672363220478-f7d463d1aba7?w=500"),
    
    # İçecekler (7)
    (39, "Kola / Fanta / Sprite", "330ml kutu", 65, 7, "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=500"),
    (40, "Ayran", "Naneli naneli taze ayran", 45, 7, "https://images.unsplash.com/photo-1644143494747-817ab7ad069c?w=500"),
    (41, "Taze Limonata", "Taze sıkılmış naneli limonata", 85, 7, "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=500"),
    (42, "Büyük Su", "750ml cam şişe", 60, 7, "https://images.unsplash.com/photo-1548839140-29a749e1cf4d?w=500"),
    (43, "Çilekli Frozen", "Orman meyveleri ve çilekli", 130, 7, "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=500"),
    (44, "Iced Latte", "Double shot espresso ve soğuk süt", 115, 7, "https://images.unsplash.com/photo-1517701604599-bb29b565090c?w=500"),
    (45, "Türk Kahvesi", "Lokum eşliğinde geleneksel", 75, 7, "https://images.unsplash.com/photo-1563222511-d0073587b140?w=500"),
    (46, "Sıcak Çay", "İnce belli bardakta Rize demli", 35, 7, "https://images.unsplash.com/photo-1576091160550-2173bdd99971?w=500"),
    
    # Kahvaltı (9)
    (47, "Serpme Kahvaltı", "Minimum 2 kişilik - her şey dahil", 450, 9, "https://images.unsplash.com/photo-1533089860892-a7c6f0a88666?w=500"),
    (48, "Hızlı Kahvaltı Tabağı", "Klasik içerikli tek kişilik", 280, 9, "https://images.unsplash.com/photo-1525351484163-7529414344d8?w=500"),
    (49, "Menemen", "Biberli, domatesli ve kaşarlı", 160, 9, "https://images.unsplash.com/photo-1616613045618-971708819545?w=500"),
]

CUSTOMERS = [
    ("Ahmet Yılmaz", "05321112233", "Premium"),
    ("Ayşe Kaya", "05412223344", "Premium"),
    ("Mehmet Demir", "05053334455", "Standard"),
    ("Zeynep Şahin", "05554445566", "Standard"),
    ("Can Erdem", "05335556677", "Frequent"),
    ("Elif Yıldız", "05456667788", "Coffee-only"),
    ("Burak Öztürk", "05307778899", "Standard"),
    ("Selin Aydın", "05428889900", "Frequent"),
    ("Kerem Koç", "04441112222", "Standard"),
    ("Fatma Çelik", "05329990011", "Premium"),
    (None, None, "Anonymous"), # Anonymous table
]

WEATHERS = ["Güneşli", "Bulutlu", "Yağmurlu", "Soğuk/Karlı"]

def enrich():
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()

    print("--- Database Cleaning ---")
    cursor.execute("DELETE FROM OrderItems")
    cursor.execute("DELETE FROM Orders")
    cursor.execute("DELETE FROM Products")
    conn.commit()

    print(f"--- Inserting {len(PRODUCTS)} Realistic Products ---")
    for p in PRODUCTS:
        # id, name, description, price, categoryId, imageUrl
        cursor.execute("""
            INSERT INTO Products (Id, Name, Description, Price, ImageUrl, CategoryId, IsAvailable, IsActive)
            VALUES (?, ?, ?, ?, ?, ?, 1, 1)
        """, (p[0], p[1], p[2], p[3], p[5], p[4]))
    conn.commit()

    print("--- Generating Realistic Transactions ---")
    start_date = datetime(2026, 3, 1)
    end_date = datetime(2026, 3, 31, 21, 0) # Current Time
    
    from itertools import count
    id_gen = count(2000)
    current_time = start_date

    # ID Mapping for specific meals
    SOUP_IDS = [1, 2, 3]
    STARTER_IDS = [4, 5, 6, 7, 8]
    SALAD_IDS = [9, 10, 11]
    BURGER_IDS = [12, 13, 14, 15]
    PIZZA_IDS = [16, 17, 18, 19]
    KEBAB_IDS = [20, 21, 22, 23, 24, 25]
    MAIN_IDS = [26, 27, 28, 29]
    PASTA_IDS = [30, 31, 32]
    DESSERT_IDS = [33, 34, 35, 36, 37, 38]
    DRINK_IDS = [39, 40, 41, 42, 43, 44, 45, 46]
    BK_IDS = [47, 48, 49]

    while current_time < end_date:
        if 2 <= current_time.hour < 8:
            current_time += timedelta(hours=1)
            continue
        
        order_count = random.randint(1, 4)
        if 8 <= current_time.hour < 11: order_count = random.randint(5, 12) # BF
        if 12 <= current_time.hour < 14: order_count = random.randint(12, 25) # Lunch
        if 19 <= current_time.hour < 22: order_count = random.randint(15, 35) # Dinner
        
        for _ in range(order_count):
            order_id = next(id_gen)
            cust_name, cust_phone, _ = random.choice(CUSTOMERS)
            table_num = f"Masa {random.randint(1, 25)}"
            weather = random.choice(WEATHERS)
            temp = random.randint(8, 26)
            
            items = []
            if 8 <= current_time.hour < 11:
                items.append((random.choice(BK_IDS), 2))
                items.append((46, random.randint(2, 6))) # Tea
            elif 12 <= current_time.hour < 14:
                # Burgers or Pizza or Pasta
                choice = random.random()
                if choice < 0.4: # Burger Combo
                    items.append((random.choice(BURGER_IDS), 1))
                    items.append((6, 1)) # Fries
                    items.append((39, 1)) # Cola
                elif choice < 0.7: # Pizza
                    items.append((random.choice(PIZZA_IDS), 1))
                    items.append((41, 1)) # Lemonade
                else: # Pasta
                    items.append((random.choice(PASTA_IDS), 1))
                    items.append((40, 1)) # Ayran
            elif 19 <= current_time.hour < 22:
                # Kebab or Main
                people = random.randint(2, 5)
                items.append((random.choice(SOUP_IDS), people))
                items.append((random.choice(KEBAB_IDS + MAIN_IDS), people))
                if random.random() < 0.7: items.append((random.choice(SALAD_IDS), 1))
                if random.random() < 0.5: items.append((random.choice(DESSERT_IDS), 2))
                items.append((random.choice(DRINK_IDS), people))
            else: # Snacking
                items.append((random.choice(DRINK_IDS), 1))
                if random.random() < 0.4: items.append((random.choice(DESSERT_IDS + STARTER_IDS), 1))

            total_amount = 0
            order_items_to_insert = []
            for prod_id, qty in items:
                price = [p[3] for p in PRODUCTS if p[0] == prod_id][0]
                total_amount += price * qty
                order_items_to_insert.append((order_id, prod_id, qty, price))

            created_at = current_time + timedelta(minutes=random.randint(1, 55))
            cursor.execute("""
                INSERT INTO Orders (Id, TableNumber, CustomerName, CustomerPhone, TotalAmount, Status, CreatedAt, WeatherCondition, Temperature, PaidAmount)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
            """, (order_id, table_num, cust_name, cust_phone, total_amount, 6, created_at.strftime('%Y-%m-%d %H:%M:%S'), weather, temp, str(total_amount)))
            
            for oi in order_items_to_insert:
                cursor.execute("""
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Note, ItemStatus)
                    VALUES (?, ?, ?, ?, ?, ?)
                """, (oi[0], oi[1], oi[2], str(oi[3]), "", 0))

        current_time += timedelta(hours=1)
        if random.random() < 0.1: conn.commit()

    conn.commit()
    conn.close()
    print("✅ Success: Database enriched with ~60 high-quality products and 3000+ transactions.")

if __name__ == "__main__":
    enrich()
