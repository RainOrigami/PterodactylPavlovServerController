using Fluxor;

namespace PterodactylPavlovServerController.Stores;

[FeatureState]
public record PageTitleState
{
    public string Title { get; init; } = "Loading...";
}

public class PageTitleSetAction
{
    public PageTitleSetAction(string title)
    {
        this.Title = title;
    }

    public string Title { get; }
}

public class PageTitleReducers
{
    [ReducerMethod]
    public static PageTitleState OnPageTitleSet(PageTitleState pageTitleState, PageTitleSetAction pageTitleSetAction)
    {
        return pageTitleState with
        {
            Title = pageTitleSetAction.Title,
        };
    }
}
