namespace PterodactylPavlovServerController.Models;

public class PingLimiterModel
{
    public bool Enabled { get; set; }
    public int Threshold { get; set; }
    public int MeasureTime { get; set; }
}
