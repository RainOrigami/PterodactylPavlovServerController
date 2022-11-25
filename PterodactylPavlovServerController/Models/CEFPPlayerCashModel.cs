using PavlovStatsReader.Models;

namespace PterodactylPavlovServerController.Models;

public class CEFPPlayerCashModel : CBaseStats
{
    public CEFPPlayerCashModel(ulong uniqueId, int cash)
    {
        this.UniqueId = uniqueId;
        this.Cash = cash;
    }

    public ulong UniqueId { get; }
    public int Cash { get; }
}
