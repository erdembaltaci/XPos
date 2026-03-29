using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BCrypt.Net;
using XPos.Infrastructure.Persistence;
using XPos.Shared.Entities;

namespace XPos.WebAPI;

public static class DbSeeder
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<XPosDbContext>();

        // Force Clear (Optional - Dev only)
        // context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seed Products & Categories
        if (!context.Products.Any())
        {
            var stations = new List<Station>
            {
                new Station { Name = "Mutfak", IpAddress = "192.168.1.100", Port = 9100 },
                new Station { Name = "Bar", IpAddress = "192.168.1.101", Port = 9100 }
            };
            
            if (!context.Stations.Any()) 
            {
                context.Stations.AddRange(stations);
                await context.SaveChangesAsync();
            }
            else
            {
                stations = await context.Stations.ToListAsync();
            }

            var categories = new List<Category>
            {
                new Category { Name = "Başlangıçlar", ImageUrl = "https://images.unsplash.com/photo-1541592103048-b860a13a4676?w=400", StationId = stations[0].Id },
                new Category { Name = "Salatalar", ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400", StationId = stations[0].Id },
                new Category { Name = "Burgerler", ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400", StationId = stations[0].Id },
                new Category { Name = "Pizzalar", ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400", StationId = stations[0].Id },
                new Category { Name = "Ana Yemekler", ImageUrl = "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=400", StationId = stations[0].Id },
                new Category { Name = "Makarnalar", ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=400", StationId = stations[0].Id },
                new Category { Name = "İçecekler", ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400", StationId = stations[1].Id },
                new Category { Name = "Tatlılar", ImageUrl = "https://images.unsplash.com/photo-1563729784474-d77dbb933a9e?w=400", StationId = stations[0].Id }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var products = new List<Product>
            {
                // ── Başlangıçlar ──
                new Product { Name = "Mercimek Çorbası", Description = "Geleneksel kırmızı mercimek çorbası, limon ve kıtır ekmek ile", Price = 75, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1547592166-23acbe346499?w=600&h=450&fit=crop" },
                new Product { Name = "Humus Tabağı", Description = "Nohut ezmesi, zeytinyağı, kırmızı toz biber ve taze pide", Price = 110, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1577805947697-89e18249d767?w=600&h=450&fit=crop" },
                new Product { Name = "Patates Kızartması", Description = "Çıtır patates, özel baharat karışımı, ranch ve BBQ dip sos", Price = 90, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&h=450&fit=crop" },
                new Product { Name = "Soğan Halkası", Description = "Çıtır kaplamalı 8 adet soğan halkası, acılı mayo sos", Price = 95, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1639024471283-03518883512d?w=600&h=450&fit=crop" },
                new Product { Name = "Kanat (8 Adet)", Description = "Izgara veya kızarmış tavuk kanatları, seçtiğiniz sos ile", Price = 140, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1527477396000-e27163b4bcd9?w=600&h=450&fit=crop" },
                new Product { Name = "Bruschetta", Description = "Kızarmış İtalyan ekmeği, domates, fesleğen, zeytinyağı", Price = 105, CategoryId = categories[0].Id, ImageUrl = "https://images.unsplash.com/photo-1572695157366-5e585ab2b69f?w=600&h=450&fit=crop" },

                // ── Salatalar ──
                new Product { Name = "Sezar Salata", Description = "Marul, parmesan, kruton, ızgara tavuk ve sezar sos", Price = 160, CategoryId = categories[1].Id, ImageUrl = "https://images.unsplash.com/photo-1550304943-4f24f54ddde9?w=600&h=450&fit=crop" },
                new Product { Name = "Akdeniz Salata", Description = "Roka, nar taneleri, ceviz, keçi peyniri, nar ekşili sos", Price = 145, CategoryId = categories[1].Id, ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&h=450&fit=crop" },
                new Product { Name = "Ton Balıklı Salata", Description = "Ton balığı, mısır, cherry domates, salatalık, zeytinyağı", Price = 170, CategoryId = categories[1].Id, ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=600&h=450&fit=crop" },
                new Product { Name = "Yunan Salatası", Description = "Domates, salatalık, yeşil biber, beyaz peynir, zeytin", Price = 130, CategoryId = categories[1].Id, ImageUrl = "https://images.unsplash.com/photo-1515543237350-b3eea1ec8082?w=600&h=450&fit=crop" },

                // ── Burgerler ──
                new Product { Name = "Klasik Cheeseburger", Description = "180gr dana köfte, cheddar, marul, domates, turşu", Price = 250, CategoryId = categories[2].Id, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&h=450&fit=crop" },
                new Product { Name = "Double Smash Burger", Description = "2x100gr smash köfte, amerikan peynir, karamelize soğan", Price = 290, CategoryId = categories[2].Id, ImageUrl = "https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=600&h=450&fit=crop" },
                new Product { Name = "Tavuk Burger", Description = "Çıtır pane tavuk, coleslaw, ranch sos, brioche ekmeği", Price = 230, CategoryId = categories[2].Id, ImageUrl = "https://images.unsplash.com/photo-1525164286253-04e68b9d94c6?w=600&h=450&fit=crop" },
                new Product { Name = "BBQ Bacon Burger", Description = "200gr köfte, bacon, cheddar, BBQ sos, soğan halkası", Price = 310, CategoryId = categories[2].Id, ImageUrl = "https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?w=600&h=450&fit=crop" },
                new Product { Name = "Mushroom Swiss Burger", Description = "180gr köfte, sote mantar, İsviçre peyniri, truffle mayo", Price = 280, CategoryId = categories[2].Id, ImageUrl = "https://images.unsplash.com/photo-1572802419224-296b0aeee15d?w=600&h=450&fit=crop" },

                // ── Pizzalar ──
                new Product { Name = "Margherita", Description = "San Marzano domates, taze mozzarella, fesleğen yaprakları", Price = 200, CategoryId = categories[3].Id, ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=600&h=450&fit=crop" },
                new Product { Name = "Pepperoni Pizza", Description = "Bol pepperoni, mozzarella, domates sosu, acı pul biber", Price = 240, CategoryId = categories[3].Id, ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=600&h=450&fit=crop" },
                new Product { Name = "Karışık Pizza", Description = "Sucuk, sosis, mantar, biber, mısır, zeytin, mozzarella", Price = 260, CategoryId = categories[3].Id, ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=600&h=450&fit=crop" },
                new Product { Name = "Dört Peynirli Pizza", Description = "Mozzarella, gorgonzola, parmesan, ricotta, bal damlası", Price = 250, CategoryId = categories[3].Id, ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600&h=450&fit=crop" },
                new Product { Name = "BBQ Tavuklu Pizza", Description = "BBQ soslu tavuk, mısır, soğan, mozzarella peyniri", Price = 245, CategoryId = categories[3].Id, ImageUrl = "https://images.unsplash.com/photo-1595708684082-a173bb3a06c5?w=600&h=450&fit=crop" },

                // ── Ana Yemekler ──
                new Product { Name = "Izgara Köfte", Description = "200gr dana kıyma köfte, pilav, közlenmiş biber ve piyaz", Price = 280, CategoryId = categories[4].Id, ImageUrl = "https://images.unsplash.com/photo-1529042410759-befb1204b468?w=600&h=450&fit=crop" },
                new Product { Name = "Dana Antrikot", Description = "250gr antrikot, fırın patates, kuşkonmaz ve tereyağı sos", Price = 520, CategoryId = categories[4].Id, ImageUrl = "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=600&h=450&fit=crop" },
                new Product { Name = "Tavuk Şiş", Description = "Marine tavuk göğsü, baharatlar, bulgur pilavı, köz biber", Price = 240, CategoryId = categories[4].Id, ImageUrl = "https://images.unsplash.com/photo-1532550907401-a500c9a57435?w=600&h=450&fit=crop" },
                new Product { Name = "Somon Izgara", Description = "Norveç somonu, limon tereyağı sos, mevsim sebze garnitür", Price = 450, CategoryId = categories[4].Id, ImageUrl = "https://images.unsplash.com/photo-1485921325833-c519f76c4927?w=600&h=450&fit=crop" },
                new Product { Name = "Tavuk Wrap", Description = "Izgara tavuk, marul, domates, ranch sos, lavaş ekmeği", Price = 195, CategoryId = categories[4].Id, ImageUrl = "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?w=600&h=450&fit=crop" },

                // ── Makarnalar ──
                new Product { Name = "Fettuccine Alfredo", Description = "Kremalı parmesan sos, taze fesleğen, karabiber", Price = 210, CategoryId = categories[5].Id, ImageUrl = "https://images.unsplash.com/photo-1645112411341-6c4fd215305ad?w=600&h=450&fit=crop" },
                new Product { Name = "Penne Arrabbiata", Description = "Acılı domates sosu, sarımsak, taze maydanoz", Price = 190, CategoryId = categories[5].Id, ImageUrl = "https://images.unsplash.com/photo-1563379926898-05f4575a45d8?w=600&h=450&fit=crop" },
                new Product { Name = "Bolonez Spagetti", Description = "Dana kıymalı ragu sos, parmesan rendesi, fesleğen", Price = 220, CategoryId = categories[5].Id, ImageUrl = "https://images.unsplash.com/photo-1622973536968-3ead9e780960?w=600&h=450&fit=crop" },
                new Product { Name = "Karidesli Makarna", Description = "Karides, sarımsak, beyaz şarap sosu, cherry domates", Price = 270, CategoryId = categories[5].Id, ImageUrl = "https://images.unsplash.com/photo-1473093295043-cdd812d0e601?w=600&h=450&fit=crop" },

                // ── İçecekler ──
                new Product { Name = "Türk Kahvesi", Description = "Çifte kavrulmuş, lokum eşliğinde servis", Price = 65, CategoryId = categories[6].Id, ImageUrl = "https://images.unsplash.com/photo-1514432324607-a09d9b4aefda?w=600&h=450&fit=crop" },
                new Product { Name = "Ev Yapımı Limonata", Description = "Taze sıkılmış limon, nane yaprakları ve buz parçacıkları", Price = 75, CategoryId = categories[6].Id, ImageUrl = "https://images.unsplash.com/photo-1556881286-fc6915169c8b?w=600&h=450&fit=crop" },
                new Product { Name = "Latte", Description = "Espresso, buharlanmış süt, latte art, seramik fincanda", Price = 90, CategoryId = categories[6].Id, ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=600&h=450&fit=crop" },
                new Product { Name = "Taze Portakal Suyu", Description = "Sıkma portakal suyu, 300ml, buz ile servis", Price = 70, CategoryId = categories[6].Id, ImageUrl = "https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?w=600&h=450&fit=crop" },
                new Product { Name = "Mojito", Description = "Taze nane, lime, soda ve şeker şurubu, buz ile", Price = 110, CategoryId = categories[6].Id, ImageUrl = "https://images.unsplash.com/photo-1551538827-9c037cb4f32a?w=600&h=450&fit=crop" },
                new Product { Name = "Çay", Description = "Taze demleme Rize çayı, ince belli bardakta", Price = 25, CategoryId = categories[6].Id, ImageUrl = "https://images.unsplash.com/photo-1571934811356-5cc061b6821f?w=600&h=450&fit=crop" },
                new Product { Name = "Cappuccino", Description = "Espresso, köpüklü süt, tarçın serpintisi", Price = 85, CategoryId = categories[6].Id, ImageUrl = "https://images.unsplash.com/photo-1572442388796-11668a67e53d?w=600&h=450&fit=crop" },

                // ── Tatlılar ──
                new Product { Name = "San Sebastian Cheesecake", Description = "Karamelize üst, kremalı iç, taze çilek dekorasyonu", Price = 180, CategoryId = categories[7].Id, ImageUrl = "https://images.unsplash.com/photo-1508737027454-e6454ef45afd?w=600&h=450&fit=crop" },
                new Product { Name = "Fırın Sütlaç", Description = "Toprak güveçte, üzeri altın renginde kızarmış", Price = 95, CategoryId = categories[7].Id, ImageUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600&h=450&fit=crop" },
                new Product { Name = "Tiramisu", Description = "İtalyan maskarpone kreması, espresso, kakao tozu", Price = 160, CategoryId = categories[7].Id, ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=600&h=450&fit=crop" },
                new Product { Name = "Çikolatalı Brownie", Description = "Sıcak servis, ceviz parçaları, vanilya dondurma topuyla", Price = 150, CategoryId = categories[7].Id, ImageUrl = "https://images.unsplash.com/photo-1564355808539-22fda35bed7e?w=600&h=450&fit=crop" },
                new Product { Name = "Profiterol", Description = "Çikolata soslu mini profiterol, fındık kırıntıları", Price = 140, CategoryId = categories[7].Id, ImageUrl = "https://images.unsplash.com/photo-1587314168485-3236d6710814?w=600&h=450&fit=crop" },
                new Product { Name = "Waffle", Description = "Belçika waffle, çikolata sos, muz, çilek, dondurma", Price = 165, CategoryId = categories[7].Id, ImageUrl = "https://images.unsplash.com/photo-1562376552-0d160a2f238d?w=600&h=450&fit=crop" }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
            
            // Masalar
            if (!context.Tables.Any())
            {
                var tablesToSeed = Enumerable.Range(1, 10).Select(i => 
                {
                    var t = new Table 
                    { 
                        TableNumber = $"Masa {i}", 
                        Capacity = i <= 8 ? 4 : 6
                    };
                    t.SecretToken = Guid.NewGuid();
                    t.QRCodeUrl = $"http://localhost:5079/?token={t.SecretToken}";
                    return t;
                }).ToList();
                
                context.Tables.AddRange(tablesToSeed);
                await context.SaveChangesAsync();
            }

            // Seed Staff (BCrypt hashed)
            if (!context.Staffs.Any())
            {
                context.Staffs.AddRange(new List<Staff>
                {
                    new Staff
                    {
                        Name = "Admin", Surname = "User", Phone = "0000",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Role = "Admin", IsActive = true
                    },
                    new Staff
                    {
                        Name = "Yardımcı", Surname = "Admin", Phone = "5550",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("sub123"),
                        Role = "SubAdmin", IsActive = true
                    },
                    new Staff
                    {
                        Name = "Ali", Surname = "Yılmaz", Phone = "5551234567",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                        Role = "Waiter", IsActive = true
                    }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
