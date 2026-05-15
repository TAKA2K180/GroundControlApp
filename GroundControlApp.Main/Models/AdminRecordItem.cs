namespace GroundControlApp.Main.Models;

public sealed class AdminRecordItem
{
    public AdminRecordItem(string primary, string secondary, string amount, string status)
    {
        Primary = primary;
        Secondary = secondary;
        Amount = amount;
        Status = status;
    }

    public string Primary { get; }

    public string Secondary { get; }

    public string Amount { get; }

    public string Status { get; }
}
