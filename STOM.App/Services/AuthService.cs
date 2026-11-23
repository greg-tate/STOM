using STOM.App.Models;

namespace STOM.App.Services;

public class AuthService
{
    private string _currentRole = "Scorer";
    private string _currentUser = "Demo User";

    public string CurrentRole 
    { 
        get => _currentRole;
        private set
        {
            if (_currentRole != value)
            {
                _currentRole = value;
                OnRoleChanged?.Invoke(this, value);
                Console.WriteLine($"[DEBUG] AuthService: Role changed to {value}");
            }
        }
    }

    public string CurrentUser 
    { 
        get => _currentUser;
        private set => _currentUser = value;
    }

    public event EventHandler<string>? OnRoleChanged;

    public Task LoginAsync(string username, string role)
    {
        CurrentUser = username;
        CurrentRole = role;
        Console.WriteLine($"[DEBUG] AuthService: Login - User: {username}, Role: {role}");
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        CurrentUser = "Demo User";
        CurrentRole = "Scorer";
        Console.WriteLine($"[DEBUG] AuthService: Logout - Reset to Scorer");
        return Task.CompletedTask;
    }

    public bool IsInRole(string role) => CurrentRole == role;
}
