using Microsoft.AspNetCore.SignalR.Client;
using XPos.Shared.DTOs;

namespace XPos.Mobile.Services;

public class OrderStateService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    public event Action<OrderDto>? OnOrderReceived;
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public OrderStateService()
    {
        // Android/iOS/Windows localhost handling
        string baseAddress = DeviceInfo.Platform == DevicePlatform.Android 
            ? "http://10.0.2.2:5029/orderHub" 
            : "http://localhost:5029/orderHub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(baseAddress)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<OrderDto>("OrderReceived", (order) =>
        {
            OnOrderReceived?.Invoke(order);
        });
    }

    public async Task StartConnection()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
