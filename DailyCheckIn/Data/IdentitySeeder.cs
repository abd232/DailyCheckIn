using DailyCheckIn.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace DailyCheckIn.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            // Define roles
            string[] roles = { "SuperAdmin", "Admin", "Employee" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                }
            }

            // Optional: Seed default admin
            const string adminName = "admin";
            const string adminEmail = "admin@example.com";
            const string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByNameAsync(adminName);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminName,
                    Email = adminEmail,
                    ArabicName = "Admin",
                    Name = "Admin",
                    StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    HourlyRate = 8,
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
                else
                {
                    throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
