using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using GroundControlApp.Data.DTOs;
using GroundControlApp.Data.Interfaces;
using GroundControlApp.Main.Models;

namespace GroundControlApp.Main.ViewModels;

public sealed class UserManagementViewModel : ObservableObject
{
    private readonly IUserService userService;
    private readonly List<UserDto> users = [];
    private Guid? editingUserId;
    private string employeeNumber = string.Empty;
    private string firstName = string.Empty;
    private string lastName = string.Empty;
    private string email = string.Empty;
    private string hourlyRateText = "0";
    private string pin = string.Empty;
    private string resetPin = string.Empty;
    private string pageStatus = "Ready";
    private UserRoleOption? selectedRole;
    private bool isActive = true;

    public UserManagementViewModel(IUserService userService)
    {
        this.userService = userService;
        UserItems = [];
        Roles =
        [
            new UserRoleOption(1, "Admin"),
            new UserRoleOption(2, "Manager"),
            new UserRoleOption(3, "Cashier"),
            new UserRoleOption(4, "Barista")
        ];

        selectedRole = Roles.First(role => role.Value == 3);
        BackCommand = new AsyncRelayCommand(() => Shell.Current.GoToAsync(".."));
        AddUserCommand = new RelayCommand(StartAddUser);
        EditUserCommand = new RelayCommand(parameter =>
        {
            if (parameter is UserManagementListItem item)
            {
                LoadEditor(item.User);
            }
        });
        SaveUserCommand = new AsyncRelayCommand(() => SaveUserAsync());
        ResetPinCommand = new AsyncRelayCommand(() => ResetPinAsync());
    }

    public ObservableCollection<UserManagementListItem> UserItems { get; }

    public ObservableCollection<UserRoleOption> Roles { get; }

    public ICommand BackCommand { get; }

    public ICommand AddUserCommand { get; }

    public ICommand EditUserCommand { get; }

    public ICommand SaveUserCommand { get; }

    public ICommand ResetPinCommand { get; }

    public string EmployeeNumber
    {
        get => employeeNumber;
        set => SetProperty(ref employeeNumber, value);
    }

    public string FirstName
    {
        get => firstName;
        set => SetProperty(ref firstName, value);
    }

    public string LastName
    {
        get => lastName;
        set => SetProperty(ref lastName, value);
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string HourlyRateText
    {
        get => hourlyRateText;
        set => SetProperty(ref hourlyRateText, value);
    }

    public string Pin
    {
        get => pin;
        set => SetProperty(ref pin, value);
    }

    public string ResetPin
    {
        get => resetPin;
        set => SetProperty(ref resetPin, value);
    }

    public UserRoleOption? SelectedRole
    {
        get => selectedRole;
        set => SetProperty(ref selectedRole, value);
    }

    public bool IsActive
    {
        get => isActive;
        set => SetProperty(ref isActive, value);
    }

    public string PageStatus
    {
        get => pageStatus;
        private set => SetProperty(ref pageStatus, value);
    }

    public string EditorTitle => editingUserId is null ? "Create user" : "Edit user";

    public bool IsEditing => editingUserId is not null;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            PageStatus = "Loading users...";
            var loadedUsers = await userService.GetUsersAsync(cancellationToken);
            users.Clear();
            users.AddRange(loadedUsers.OrderBy(user => user.LastName).ThenBy(user => user.FirstName));
            RefreshUserItems();
            PageStatus = $"Loaded {users.Count} users.";

            if (editingUserId is Guid id && users.FirstOrDefault(user => user.Id == id) is { } activeUser)
            {
                LoadEditor(activeUser);
            }
            else if (editingUserId is null)
            {
                StartAddUser();
            }
        }
        catch (Exception ex)
        {
            PageStatus = $"Users unavailable: {ex.Message}";
        }
    }

    private void StartAddUser()
    {
        editingUserId = null;
        EmployeeNumber = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        HourlyRateText = "0";
        Pin = string.Empty;
        ResetPin = string.Empty;
        SelectedRole = Roles.First(role => role.Value == 3);
        IsActive = true;
        RefreshEditorState();
    }

    private void LoadEditor(UserDto user)
    {
        editingUserId = user.Id;
        EmployeeNumber = user.EmployeeNumber;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Email = user.Email;
        HourlyRateText = user.HourlyRate.ToString("0.##", CultureInfo.InvariantCulture);
        Pin = string.Empty;
        ResetPin = string.Empty;
        SelectedRole = Roles.FirstOrDefault(role => role.Value == user.Role) ?? Roles.First();
        IsActive = user.IsActive;
        RefreshEditorState();
    }

    private async Task SaveUserAsync(CancellationToken cancellationToken = default)
    {
        if (!ValidateEditor(out var hourlyRate))
        {
            return;
        }

        try
        {
            PageStatus = editingUserId is null ? "Creating user..." : "Saving user...";
            var request = new SaveUserDto(
                EmployeeNumber.Trim(),
                FirstName.Trim(),
                LastName.Trim(),
                Email.Trim(),
                SelectedRole?.Value ?? 3,
                hourlyRate,
                IsActive,
                string.IsNullOrWhiteSpace(Pin) ? null : Pin);

            var savedUser = editingUserId is Guid id
                ? await userService.UpdateUserAsync(id, request, cancellationToken)
                : await userService.CreateUserAsync(request, cancellationToken);

            editingUserId = savedUser.Id;
            PageStatus = $"{savedUser.FirstName} {savedUser.LastName} saved.";
            await LoadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            PageStatus = $"Save failed: {ex.Message}";
        }
    }

    private async Task ResetPinAsync(CancellationToken cancellationToken = default)
    {
        if (editingUserId is not Guid id)
        {
            PageStatus = "Select a user before resetting PIN.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ResetPin))
        {
            PageStatus = "Enter a new PIN.";
            return;
        }

        try
        {
            PageStatus = "Resetting PIN...";
            var user = await userService.ResetPinAsync(id, new ResetUserPinDto(ResetPin), cancellationToken);
            ResetPin = string.Empty;
            PageStatus = $"{user.FirstName} {user.LastName} PIN reset.";
            await LoadAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            PageStatus = $"PIN reset failed: {ex.Message}";
        }
    }

    private bool ValidateEditor(out decimal hourlyRate)
    {
        hourlyRate = 0;

        if (string.IsNullOrWhiteSpace(EmployeeNumber) ||
            string.IsNullOrWhiteSpace(FirstName) ||
            string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(Email))
        {
            PageStatus = "Employee number, name, and email are required.";
            return false;
        }

        if (editingUserId is null && string.IsNullOrWhiteSpace(Pin))
        {
            PageStatus = "PIN is required for new users.";
            return false;
        }

        if (!decimal.TryParse(HourlyRateText, NumberStyles.Number, CultureInfo.InvariantCulture, out hourlyRate) ||
            hourlyRate < 0)
        {
            PageStatus = "Enter a valid hourly rate.";
            return false;
        }

        return true;
    }

    private void RefreshUserItems()
    {
        UserItems.Clear();

        foreach (var user in users)
        {
            UserItems.Add(new UserManagementListItem(user));
        }
    }

    private void RefreshEditorState()
    {
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(IsEditing));
    }
}
