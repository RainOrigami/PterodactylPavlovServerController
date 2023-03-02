using PavlovVR_Rcon.Models.Pavlov;

namespace PterodactylPavlovServerController.Models;

public class WarmupRoundModel
{
    public bool UseWarmupRound { get; set; } = false;
    public List<Item> Items { get; set; } = new();
}
