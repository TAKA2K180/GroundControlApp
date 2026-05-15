namespace GroundControlApp.Main.Models;

public sealed class AdminTaskItem
{
    public AdminTaskItem(string title, string description, string actionText)
    {
        Title = title;
        Description = description;
        ActionText = actionText;
    }

    public string Title { get; }

    public string Description { get; }

    public string ActionText { get; }
}
