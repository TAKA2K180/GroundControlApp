using GroundControlApp.Main.Models;
using GroundControlApp.Main.ViewModels;
using GroundControlApp.Data.DTOs;

namespace GroundControlApp.Main.Services;

public sealed class AppIdentitySession : ObservableObject
{
    private static readonly Guid TemporaryAdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private Guid? currentUserId;
    private string displayName = "Guest";
    private AppRole role = AppRole.Guest;

    public Guid? CurrentUserId
    {
        get => currentUserId;
        private set => SetProperty(ref currentUserId, value);
    }

    public string DisplayName
    {
        get => displayName;
        private set => SetProperty(ref displayName, value);
    }

    public AppRole Role
    {
        get => role;
        private set
        {
            if (SetProperty(ref role, value))
            {
                OnPropertyChanged(nameof(RoleName));
                OnPropertyChanged(nameof(IsSignedIn));
            }
        }
    }

    public string RoleName => Role.ToString();

    public bool IsSignedIn => CurrentUserId is not null;

    public bool CanAccess(AppRole requiredRole)
    {
        return Role >= requiredRole;
    }

    public void SignInTemporaryAdmin()
    {
        CurrentUserId = TemporaryAdminUserId;
        DisplayName = "Admin User";
        Role = AppRole.Admin;
        OnPropertyChanged(nameof(IsSignedIn));
    }

    public void SignIn(UserDto user)
    {
        CurrentUserId = user.Id;
        DisplayName = $"{user.FirstName} {user.LastName}";
        Role = MapRole(user.Role);
        OnPropertyChanged(nameof(IsSignedIn));
    }

    public void SignOut()
    {
        CurrentUserId = null;
        DisplayName = "Guest";
        Role = AppRole.Guest;
        OnPropertyChanged(nameof(IsSignedIn));
    }

    private static AppRole MapRole(int role)
    {
        return role switch
        {
            1 => AppRole.Admin,
            2 => AppRole.Manager,
            3 => AppRole.Cashier,
            4 => AppRole.Barista,
            _ => AppRole.Guest
        };
    }
}
