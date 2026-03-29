namespace XPos.Shared.Enums;

public enum OrderStatus
{
    Pending = 0,    // Bekliyor
    Confirmed = 1,  // Onaylandı
    Preparing = 2,  // Hazırlanıyor
    Ready = 3,      // Hazır
    Delivered = 4,  // Teslim Edildi
    Paid = 5,       // Ödendi
    Cancelled = 6,  // İptal
    AwaitingApproval = 10 // Onay Bekleyen (QR Siparişleri)
}
