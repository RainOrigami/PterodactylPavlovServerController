namespace PterodactylPavlovServerController.Models;

public class StatsSinceDateModel
{
    public StatsSinceDateModel(string text, DateTime since)
    {
        this.Text = text;
        this.Since = since;
    }

    public string Text { get; }
    public DateTime Since { get; }
}
