namespace PterodactylPavlovServerController.Models;

public class StatsImageModel
{
    public StatsImageModel(string imageUrl, string imageAlt)
    {
        this.ImageUrl = imageUrl;
        this.ImageAlt = imageAlt;
    }

    public string ImageUrl { get; }
    public string ImageAlt { get; }
}
