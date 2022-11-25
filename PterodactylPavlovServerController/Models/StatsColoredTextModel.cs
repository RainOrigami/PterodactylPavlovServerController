namespace PterodactylPavlovServerController.Models;

public class StatsColoredTextModel
{
    public StatsColoredTextModel(string text, string colorClass)
    {
        this.Text = text;
        this.ColorClass = colorClass;
    }

    public string Text { get; }
    public string ColorClass { get; }
}
