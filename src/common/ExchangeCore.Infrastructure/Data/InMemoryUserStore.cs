using Microsoft.AspNetCore.Identity;

namespace ExchangeCore.Infrastructure.Data;

public class InMemoryUserStore : IUserStore<IdentityUser>,
                                 IUserPasswordStore<IdentityUser>,
                                 IUserRoleStore<IdentityUser>
{
    private readonly List<IdentityUser> _users = new();
    private readonly List<IdentityRole> _roles = new();
    private readonly List<IdentityUserRole<string>> _userRoles = new();

    public InMemoryUserStore()
    {
        // Initialize with your static users
        var adminUser = new IdentityUser
        {
            Id = "1",
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAECeaVgDLALH52ZCNEAiQ01d5SUuOjMF9nCDZD7cic07MrczK0JODxeXx3qr8UVgJEA==" // Hashed "Admin@123"
        };

        var appUser = new IdentityUser
        {
            Id = "2",
            UserName = "user",
            NormalizedUserName = "USER",
            Email = "user@example.com",
            NormalizedEmail = "USER@EXAMPLE.COM",
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEP4IsNMG6VR5ngLJjW+rXbC6SFTVQcJJgxv9k+BtqUsz/fetCtAHVRSeaAlbzgyxfw==" // Hashed "User@123"
        };

        _users.Add(adminUser);
        _users.Add(appUser);

        // Initialize roles
        var adminRole = new IdentityRole { Id = "1", Name = "adminUser", NormalizedName = "ADMINUSER" };
        var userRole = new IdentityRole { Id = "2", Name = "appUser", NormalizedName = "APPUSER" };
        
        _roles.Add(adminRole);
        _roles.Add(userRole);

        // Assign roles
        _userRoles.Add(new IdentityUserRole<string> { UserId = "1", RoleId = "1" }); // admin is adminUser
        _userRoles.Add(new IdentityUserRole<string> { UserId = "2", RoleId = "2" }); // user is appUser
    }

    // Implement all required interface methods
    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.Add(user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        var found = _users.FirstOrDefault(u => u.Id == user.Id);
        if (found is not null)
        {
            _users.Remove(found);
            return Task.FromResult(IdentityResult.Success);
        }
        return Task.FromResult(IdentityResult.Failed());
    }

    public Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.Id == userId));
    }

    public Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName));
    }

    public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing is not null)
        {
            existing.UserName = user.UserName;
            existing.NormalizedUserName = user.NormalizedUserName;
            existing.Email = user.Email;
            existing.NormalizedEmail = user.NormalizedEmail;
            existing.PasswordHash = user.PasswordHash;
            return Task.FromResult(IdentityResult.Success);
        }
        return Task.FromResult(IdentityResult.Failed());
    }

    public Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task AddToRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = _roles.FirstOrDefault(r => r.NormalizedName == roleName.ToUpper());
        if (role is not null)
        {
            _userRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        }
        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = _roles.FirstOrDefault(r => r.NormalizedName == roleName.ToUpper());
        if (role is not null)
        {
            var userRole = _userRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
            if (userRole is not null)
            {
                _userRoles.Remove(userRole);
            }
        }
        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        var roleIds = _userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
        var roles = _roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();
        return Task.FromResult<IList<string>>(roles);
    }

    public Task<bool> IsInRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = _roles.FirstOrDefault(r => r.NormalizedName == roleName.ToUpper());
        if (role is not null)
        {
            return Task.FromResult(_userRoles.Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id));
        }
        return Task.FromResult(false);
    }

    public Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var role = _roles.FirstOrDefault(r => r.NormalizedName == roleName.ToUpper());
        if (role is not null)
        {
            var userIds = _userRoles.Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToList();
            var users = _users.Where(u => userIds.Contains(u.Id)).ToList();
            return Task.FromResult<IList<IdentityUser>>(users);
        }
        return Task.FromResult<IList<IdentityUser>>(new List<IdentityUser>());
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}

public class InMemoryRoleStore : IRoleStore<IdentityRole>
{
    private readonly List<IdentityRole> _roles = new();

    public InMemoryRoleStore()
    {
        // Initialize with your roles
        _roles.Add(new IdentityRole { Id = "1", Name = "adminUser", NormalizedName = "ADMINUSER" });
        _roles.Add(new IdentityRole { Id = "2", Name = "appUser", NormalizedName = "APPUSER" });
    }

    // Implement all required IRoleStore methods
    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.Add(role);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        var found = _roles.FirstOrDefault(r => r.Id == role.Id);
        if (found is not null)
        {
            _roles.Remove(found);
            return Task.FromResult(IdentityResult.Success);
        }
        return Task.FromResult(IdentityResult.Failed());
    }

    public Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_roles.FirstOrDefault(r => r.Id == roleId));
    }

    public Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return Task.FromResult(_roles.FirstOrDefault(r => r.NormalizedName == normalizedRoleName));
    }

    public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.NormalizedName);
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id);
    }

    public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Name);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        var existing = _roles.FirstOrDefault(r => r.Id == role.Id);
        if (existing is not null)
        {
            existing.Name = role.Name;
            existing.NormalizedName = role.NormalizedName;
            return Task.FromResult(IdentityResult.Success);
        }
        return Task.FromResult(IdentityResult.Failed());
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}