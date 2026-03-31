using XPos.Shared.DTOs;
using XPos.Application.Interfaces;
using XPos.Domain.Interfaces;
using XPos.Shared.Entities;
using XPos.Shared.Enums;
using Microsoft.Extensions.Configuration;

namespace XPos.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IGenericRepository<Table> _tableRepository;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IWeatherService _weatherService;
    private readonly double _lat;
    private readonly double _lon;

    public OrderService(
        IOrderRepository orderRepository,
        IGenericRepository<Table> tableRepository,
        IGenericRepository<Product> productRepository,
        IWeatherService weatherService,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _tableRepository = tableRepository;
        _productRepository = productRepository;
        _weatherService = weatherService;
        _lat = configuration.GetValue<double>("Location:Latitude", 39.9208);
        _lon = configuration.GetValue<double>("Location:Longitude", 32.8541);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByTableAsync(string tableNumber)
    {
        var orders = await _orderRepository.GetOrdersByTableWithItemsAsync(tableNumber);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        Console.WriteLine($"[OrderService] CreateOrderAsync - Masa: '{createOrderDto.TableNumber}', Ürün Sayısı: {createOrderDto.Items.Count}");

        // Masaya ait aktif (ödenmemiş veya iptal edilmemiş) siparişleri getir (Items dahil)
        var allTableOrders = await _orderRepository.GetOrdersByTableWithItemsAsync(createOrderDto.TableNumber);
        var activeOrders = allTableOrders
            .Where(o => o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

        Console.WriteLine($"[OrderService] Bulunan aktif sipariş sayısı: {activeOrders.Count}");

        // Stok Kontrolü
        foreach (var item in createOrderDto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null && !product.IsAvailable)
            {
                throw new InvalidOperationException($"Ürün stokta yok: {product.Name}");
            }
        }

        if (activeOrders.Any())
        {
            var existingOrder = activeOrders.First();
            Console.WriteLine($"[OrderService] Var olan siparişe ekleniyor. ID: {existingOrder.Id}");

            // Var olan siparişe ekle
            foreach (var itemDto in createOrderDto.Items)
            {
                existingOrder.Items.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    Note = itemDto.Note ?? string.Empty,
                    ItemStatus = OrderStatus.Pending
                });
            }

            // Toplam tutarı tüm kalemler üzerinden yeniden hesapla
            existingOrder.TotalAmount = existingOrder.Items.Sum(i => i.Quantity * i.UnitPrice);
            
            // Eğer yeni bir sipariş gelirse, tüm siparişin durumunu tekrar "Pending" (Onay Bekliyor) yapıyoruz
            existingOrder.Status = OrderStatus.Pending;

            await _orderRepository.UpdateAsync(existingOrder);
            return MapToDto(existingOrder);
        }
        else
        {
            Console.WriteLine($"[OrderService] Yeni sipariş oluşturuluyor.");
            // Yeni sipariş oluştur
            var order = new Order
            {
                TableNumber = createOrderDto.TableNumber,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                Items = createOrderDto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Note = i.Note ?? string.Empty,
                    ItemStatus = OrderStatus.Pending
                }).ToList()
            };

            // Hava durumu verisini appsettings'ten okunan koordinatlarla çek
            var (condition, temp) = await _weatherService.GetCurrentWeatherAsync(_lat, _lon);
            order.WeatherCondition = condition;
            order.Temperature = temp;

            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);
            await _orderRepository.AddAsync(order);

            // Masayı dolu yap
            var tables = await _tableRepository.FindAsync(t => t.TableNumber == createOrderDto.TableNumber);
            var table = tables.FirstOrDefault();
            if (table != null)
            {
                table.Status = TableStatus.Occupied;
                await _tableRepository.UpdateAsync(table);
            }

            return MapToDto(order);
        }
    }

    public async Task UpdateOrderStatusAsync(int id, int statusId)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order != null)
        {
            order.Status = (OrderStatus)statusId;
            
            // Eğer sipariş durumu değişiyorsa, tüm kalemlerin durumunu da güncelle (örn: Onay Bekliyor -> Bekliyor)
            // Ancak sadece "hazır/teslim" gibi ilerlemelerde değil, başlangıç aşamasındaki toplu değişimlerde yapalım.
            // AwaitingApproval -> Pending geçişinde items update şart.
            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Cancelled)
            {
                foreach(var item in order.Items)
                {
                    item.ItemStatus = order.Status;
                }
            }
            
            await _orderRepository.UpdateAsync(order);
        }
    }

    public async Task UpdateOrderItemStatusAsync(int orderItemId, int statusId)
    {
        // Doğrudan DB'den ilgili siparişi çek (tüm siparişleri taramak yerine)
        var order = await _orderRepository.GetOrderByItemIdAsync(orderItemId);
        if (order == null) return;

        var item = order.Items.FirstOrDefault(i => i.Id == orderItemId);
        if (item != null)
        {
            ((XPos.Shared.Entities.OrderItem)item).ItemStatus = (OrderStatus)statusId;
            await _orderRepository.UpdateAsync(order);
        }
    }

    public async Task<IEnumerable<OrderItemDto>> GetActiveItemsByStationAsync(int stationId)
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        var result = new List<OrderItemDto>();

        foreach (var order in orders)
        {
            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Cancelled)
                continue;

            foreach (XPos.Shared.Entities.OrderItem item in order.Items)
            {
                if (item.Product != null && item.Product.Category != null && item.Product.Category.StationId == stationId)
                {
                    if (item.ItemStatus != OrderStatus.Delivered && item.ItemStatus != OrderStatus.Ready && item.ItemStatus != OrderStatus.AwaitingApproval)
                    {
                        result.Add(new OrderItemDto
                        {
                            Id = item.Id,
                            OrderId = item.OrderId,
                            TableNumber = order.TableNumber,
                            ProductId = item.ProductId,
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Note = item.Note,
                            StationStatus = item.ItemStatus
                        });
                    }
                }
            }
        }
        return result;
    }

    public async Task AddPaymentAsync(int id, decimal amount)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order != null)
        {
            order.PaidAmount += amount;
            
            // Eğer ödenen tutar toplam tutara eşit veya fazlaysa durumu ödendi yap
            if (order.PaidAmount >= order.TotalAmount)
            {
                order.Status = OrderStatus.Paid;
                
                // Masayı da boşalt
                var tables = await _tableRepository.FindAsync(t => t.TableNumber == order.TableNumber);
                var table = tables.FirstOrDefault();
                if (table != null)
                {
                    table.Status = TableStatus.Available;
                    await _tableRepository.UpdateAsync(table);
                }
            }
            
            await _orderRepository.UpdateAsync(order);
        }
    }

    public async Task PayOrderItemsAsync(int orderId, List<int> orderItemIds)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order == null) return;

        bool anyChange = false;

        foreach (var itemId in orderItemIds)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == itemId);
            if (item != null && item.ItemStatus != OrderStatus.Paid && item.ItemStatus != OrderStatus.Cancelled)
            {
                // Kalem tutarını ödenen tutara ekle
                decimal itemTotal = item.Quantity * item.UnitPrice;
                order.PaidAmount += itemTotal;
                
                // Kalem durumunu güncelle
                item.ItemStatus = OrderStatus.Paid;
                anyChange = true;
            }
        }

        if (anyChange)
        {
            // Eğer tüm kalemler ödendiyse (veya iptal edildiyse), siparişi kapat
            bool allPaid = order.Items.All(i => i.ItemStatus == OrderStatus.Paid || i.ItemStatus == OrderStatus.Cancelled);
            
            // Veya ödenen tutar toplamı donarsa (bazen küsurat farkı olabilir, toleranslı kontrol gerekebilir ama şimdilik >=)
            if (allPaid || order.PaidAmount >= order.TotalAmount)
            {
                order.Status = OrderStatus.Paid;
                
                // Masayı da boşalt
                var tables = await _tableRepository.FindAsync(t => t.TableNumber == order.TableNumber);
                var table = tables.FirstOrDefault();
                if (table != null)
                {
                    table.Status = TableStatus.Available;
                    await _tableRepository.UpdateAsync(table);
                }
            }

            await _orderRepository.UpdateAsync(order);
        }
    }

    public async Task<OrderDto?> GetActiveOrderForTableAsync(string tableNumber)
    {
        var allTableOrders = await _orderRepository.GetOrdersByTableWithItemsAsync(tableNumber);
        var activeOrders = allTableOrders
            .Where(o => o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.CreatedAt) // En eski olanı ana sipariş yapalım
            .ToList();

        if (!activeOrders.Any()) return null;

        var primaryOrder = activeOrders.First();

        if (activeOrders.Count > 1)
        {
            // Birden fazla aktif sipariş var, birleştirelim
            for (int i = 1; i < activeOrders.Count; i++)
            {
                var otherOrder = activeOrders[i];
                
                // Kalemleri kopyala
                foreach (var item in otherOrder.Items)
                {
                    primaryOrder.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Note = item.Note,
                        ItemStatus = item.ItemStatus
                    });
                }

                // Ödenmiş tutarı ekle
                primaryOrder.PaidAmount += otherOrder.PaidAmount;
                
                // Eğer birleştirilen siparişlerden herhangi biri "Onay Bekliyor" ise, ana siparişi "Onay Bekliyor" yap
                if (otherOrder.Status == OrderStatus.Pending)
                {
                    primaryOrder.Status = OrderStatus.Pending;
                }

                // Diğer siparişi sildir (Repository üzerinden)
                await _orderRepository.DeleteAsync(otherOrder.Id);
            }

            // Toplam tutarı tüm kalemler üzerinden yeniden hesapla
            primaryOrder.TotalAmount = primaryOrder.Items.Sum(i => i.Quantity * i.UnitPrice);
            
            await _orderRepository.UpdateAsync(primaryOrder);
        }

        return MapToDto(primaryOrder);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            TableNumber = order.TableNumber,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            PaidAmount = order.PaidAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                TableNumber = order.TableNumber,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "Bilinmeyen Ürün",
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Note = i.Note,
                StationStatus = i.ItemStatus
            }).ToList()
        };
    }
}
