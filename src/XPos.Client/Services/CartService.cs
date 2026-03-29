using System.Net.Http.Json;
using XPos.Shared.DTOs;

namespace XPos.Client.Services;

public class CartItem
{
    public ProductDto Product { get; set; } = new();
    public int Quantity { get; set; }
    public string Note { get; set; } = string.Empty; // Müşteri notu
}

public class CartService
{
    public List<CartItem> Items { get; private set; } = new();
    public string? TableNumber { get; set; }
    public event Action? OnChange;

    public void AddToCart(ProductDto product)
    {
        var existingItem = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            Items.Add(new CartItem { Product = product, Quantity = 1 });
        }
        NotifyStateChanged();
    }

    public void RemoveFromCart(ProductDto product)
    {
        var item = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (item != null)
        {
            Items.Remove(item);
            NotifyStateChanged();
        }
    }

    public void IncreaseQuantity(ProductDto product)
    {
        var item = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (item != null)
        {
            item.Quantity++;
            NotifyStateChanged();
        }
    }

    public void DecreaseQuantity(ProductDto product)
    {
        var item = Items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (item != null)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                Items.Remove(item);
            }
            NotifyStateChanged();
        }
    }

    public void Clear()
    {
        Items.Clear();
        NotifyStateChanged();
    }

    public async Task<bool> SubmitOrder(string tableNumber, HttpClient http)
    {
        if (!Items.Any()) return false;

        var orderDto = new XPos.Shared.DTOs.CreateOrderDto
        {
            TableNumber = tableNumber,
            Items = Items.Select(i => new XPos.Shared.DTOs.CreateOrderItemDto
            {
                ProductId = i.Product.Id,
                Quantity = i.Quantity,
                UnitPrice = i.Product.Price,
                Note = i.Note
            }).ToList()
        };

        var response = await http.PostAsJsonAsync("api/orders", orderDto);
        if (response.IsSuccessStatusCode)
        {
            Clear();
            return true;
        }
        return false;
    }

    public decimal TotalPrice => Items.Sum(i => i.Product.Price * i.Quantity);

    private void NotifyStateChanged() => OnChange?.Invoke();
}
