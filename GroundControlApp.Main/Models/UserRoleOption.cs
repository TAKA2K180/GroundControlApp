namespace GroundControlApp.Main.Models;

public sealed class UserRoleOption
{
    public UserRoleOption(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public int Value { get; }

    public string Name { get; }
}
