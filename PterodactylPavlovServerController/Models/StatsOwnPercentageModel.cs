namespace PterodactylPavlovServerController.Models;

public class StatsOwnPercentageModel
{
    public StatsOwnPercentageModel(string text, double percentage)
    {
        this.Text = text;
        this.Percentage = percentage;
    }

    public string Text { get; }
    public double Percentage { get; }
}
