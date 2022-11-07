namespace PterodactylPavlovServerController.Pages.Rcon
{
    public partial class Server
    {
        //[Inject]
        //public PterodactylService PterodactylService { get; set; }

        //[Inject]
        //public PavlovRconService PavlovRconService { get; set; }

        //[Inject]
        //public PavlovServerService PavlovServerService { get; set; }

        //[Inject]
        //public SteamWorkshopService SteamWorkshopService { get; set; }

        //[Parameter]
        //public string ServerId { get; set; }

        //private PterodactylServerModel? pterodactylServerModel;
        //private ServerInfoModel? serverInfoModel;
        //private string? serverNameFromGameIni;

        //private MapDetailModel? mapDetailModel;

        //protected override void OnInitialized()
        //{
        //    Task.Run(initialise);
        //}

        //private async Task initialise()
        //{
        //    pterodactylServerModel = await Task.Run(() => PterodactylService.GetServers().FirstOrDefault(s => s.ServerId == ServerId));

        //    await InvokeAsync(() => StateHasChanged());

        //    serverInfoModel = await Task.Run(() => PavlovRconService.GetServerInfo(ServerId));

        //    if (serverInfoModel == null)
        //    {
        //        serverNameFromGameIni = await Task.Run(() => PavlovServerService.GetServerName(ServerId));
        //        await InvokeAsync(() => StateHasChanged());
        //        return;
        //    }

        //    await InvokeAsync(() => StateHasChanged());

        //    if (serverInfoModel.MapLabel.StartsWith("UGC"))
        //    {
        //        mapDetailModel = await Task.Run(() => SteamWorkshopService.GetMapDetail(long.Parse(serverInfoModel.MapLabel.Substring(3))));

        //        await InvokeAsync(() => StateHasChanged());
        //    }
        //}
    }
}
