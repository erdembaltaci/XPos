namespace XPos.Shared.Entities;

public class WeatherLog
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Condition { get; set; } = string.Empty;
    public double Temperature { get; set; }
}
