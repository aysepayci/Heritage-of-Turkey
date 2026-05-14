using Heritage_of_Turkey.Models;
using Microsoft.AspNetCore.Identity;

namespace Heritage_of_Turkey.Data
{
    public static class IdentitySeedData
    {
        private const string AdminRole = "Admin";
        private const string UserRole = "User";

        private const string AdminEmail = "admin@heritageofturkey.com";
        private const string AdminPassword = "Admin123!";

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await CreateRoleIfNotExistsAsync(roleManager, AdminRole);
            await CreateRoleIfNotExistsAsync(roleManager, UserRole);
            await CreateDefaultAdminUserAsync(userManager);
        }

        private static async Task CreateRoleIfNotExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task CreateDefaultAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminUser = await userManager.FindByEmailAsync(AdminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, AdminPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Default admin user could not be created: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
    }
}
