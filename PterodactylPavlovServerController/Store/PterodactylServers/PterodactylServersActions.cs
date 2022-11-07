using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Store.Servers
{
    public class PterodactylServersLoadAction { }

    public class PterodactylServersSetAction
    {
        public PterodactylServerModel[] Servers { get; }

        public PterodactylServersSetAction(PterodactylServerModel[] servers)
        {
            this.Servers = servers;
        }
    }
}
