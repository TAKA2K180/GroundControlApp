using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Main.Models;

public sealed class UserManagementListItem
{
    public UserManagementListItem(UserDto user)
    {
        User = user;
    }

    public UserDto User { get; }

    public Guid Id => User.Id;

    public string DisplayName => $"{User.FirstName} {User.LastName}";

    public string Detail => $"{User.EmployeeNumber} - {User.Email} - {RoleDisplay}";

    public string RoleDisplay => User.Role switch
    {
        1 => "Admin",
        2 => "Manager",
        3 => "Cashier",
        4 => "Barista",
        _ => "Unknown"
    };

    public string RateDisplay => $"PHP {User.HourlyRate:N2}/hr";

    public string StatusDisplay => User.IsActive ? "Active" : "Inactive";
}
