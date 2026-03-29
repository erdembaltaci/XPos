namespace XPos.Shared.Entities;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Mutfak, Bar
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 9100; // Varsayılan yazıcı portu
}
