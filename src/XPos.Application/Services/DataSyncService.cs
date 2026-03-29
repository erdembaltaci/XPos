using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPos.Application.Interfaces;
using XPos.Domain.Interfaces;
using XPos.Shared.Entities;
using XPos.Shared.Enums;

namespace XPos.Application.Services;

public class DataSyncService : IDataSyncService
{
    private readonly IOrderRepository _orderRepository;

    public DataSyncService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<string> ExportOrdersToCsvAsync(string filePath)
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        var csv = new StringBuilder();
        csv.AppendLine("siparis_id,qr_masa_id,tarih_saat,gun,saat,ay,hafta_sonu,ozel_gun,yas_grubu,kisi_sayisi,hava_durumu,sicaklik_c,toplam_tutar,ikram_var,iptal_var,urun_sayisi,siparis_icerigi");

        foreach (var order in orders)
        {
            var content = string.Join(", ", order.Items.Select(i => $"{i.Quantity} {i.Product?.Name ?? "Ürün"}"));
            if (content.Contains(",")) content = $"\"{content}\"";

            var dayOfWeek = order.CreatedAt.DayOfWeek.ToString();
            var isWeekend = (order.CreatedAt.DayOfWeek == DayOfWeek.Saturday || order.CreatedAt.DayOfWeek == DayOfWeek.Sunday) ? 1 : 0;

            csv.AppendLine($"{order.Id},{order.TableNumber},{order.CreatedAt:yyyy-MM-dd HH:mm},{dayOfWeek},{order.CreatedAt.Hour},{order.CreatedAt.Month},{isWeekend},,25-34,2,{(string.IsNullOrEmpty(order.WeatherCondition) ? "Güneşli" : order.WeatherCondition)},{(order.Temperature ?? 20)},{order.TotalAmount.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)},0,0,{order.Items.Count},{content}");
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task<string> ExportBasketDataToCsvAsync(string filePath)
    {
        var orders = await _orderRepository.GetAllOrdersWithItemsAsync();
        var csv = new StringBuilder();
        
        foreach (var order in orders)
        {
            var items = order.Items.Select(i => i.Product?.Name ?? "Ürün").ToList();
            if (items.Any())
            {
                csv.AppendLine(string.Join(",", items));
            }
        }

        await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
        return filePath;
    }

    public async Task SyncAllDataToMlServiceAsync()
    {
        // Workspace root Discovery (Simple heuristic)
        string baseDir = AppContext.BaseDirectory;
        string mlDataPath = "";

        // Try to find ml_data folder by going up levels
        string current = baseDir;
        for (int i = 0; i < 10; i++)
        {
            string testPath = Path.Combine(current, "ml_data");
            if (Directory.Exists(testPath))
            {
                mlDataPath = testPath;
                break;
            }
            current = Path.GetDirectoryName(current);
            if (current == null) break;
        }

        if (string.IsNullOrEmpty(mlDataPath))
        {
            // Fallback to hardcoded path if discovery fails
            mlDataPath = @"c:\Users\erdem\OneDrive\Masaüstü\XPos\ml_data";
        }

        if (!Directory.Exists(mlDataPath)) Directory.CreateDirectory(mlDataPath);

        await ExportOrdersToCsvAsync(Path.Combine(mlDataPath, "orders_summary.csv"));
        await ExportBasketDataToCsvAsync(Path.Combine(mlDataPath, "order_items.csv"));
    }
}
