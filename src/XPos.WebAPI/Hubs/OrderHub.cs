using Microsoft.AspNetCore.SignalR;

namespace XPos.WebAPI.Hubs;

public class OrderHub : Hub
{
    // İstemciler (Mobil/Desktop Kasa) bu metoda abone olacak.
    // Client'lar direkt buraya mesaj atmaz, genelde API üzerinden tetiklenir.
    // Ancak gerekirse buraya metodlar eklenebilir.
    
    public async Task SendOrderUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveOrderUpdate", message);
    }
}
