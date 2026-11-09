using STOM.App.Models;

namespace STOM.App.Services;

public class AuthService
{
    public string CurrentRole { get; private set; } = "Scorer";
    public string CurrentUser { get; private set; } = "Demo User";

    public Task LoginAsync(string username, string role)
    {
        CurrentUser = username;
        CurrentRole = role;
        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        CurrentRole = "Scorer";
        CurrentUser = "Demo User";
        return Task.CompletedTask;
    }

    public bool IsInRole(string role) => CurrentRole == role;
}
