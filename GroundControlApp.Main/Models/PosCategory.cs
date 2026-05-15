namespace GroundControlApp.Main.Models;

public sealed class PosCategory
{
    public PosCategory(string name, bool isSelected = false)
    {
        Name = name;
        IsSelected = isSelected;
    }

    public string Name { get; }

    public bool IsSelected { get; }

    public string BackgroundColor => IsSelected ? "#14213D" : "#F4F6F8";

    public string TextColor => IsSelected ? "#FFFFFF" : "#344054";
}
