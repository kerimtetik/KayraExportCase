using Auth.Infrastructure.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Services;

public class RoleSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        foreach (var roleName in AppRoles.All)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Role seeding failed for '{roleName}': {errors}");
            }
        }
    }
}
