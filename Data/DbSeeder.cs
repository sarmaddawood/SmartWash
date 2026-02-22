using Microsoft.AspNetCore.Identity;

namespace SmartWash.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            
            string[] roles = { "Admin", "Staff", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Create a default Admin user and link a CustomerProfile
            string adminEmail = "admin@smartwash.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            
            if (existingAdmin == null)
            {
                var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");

                    var profile = new Models.CustomerProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = admin.Id,
                        FullName = "System Administrator",
                        Email = adminEmail,
                        PhoneNumber = "N/A",
                        Address = "System Core"
                    };
                    context.CustomerProfiles.Add(profile);
                    await context.SaveChangesAsync();
                }
            }

            // Sanitize and fix any manually inserted/corrupted plaintext passwords in the database
            var allUsers = userManager.Users.ToList();
            foreach (var user in allUsers)
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    try
                    {
                        Convert.FromBase64String(user.PasswordHash);
                    }
                    catch (FormatException)
                    {
                        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, user.PasswordHash);
                        await userManager.UpdateAsync(user);
                    }
                }
                else
                {
                    user.PasswordHash = userManager.PasswordHasher.HashPassword(user, "Password123!");
                    await userManager.UpdateAsync(user);
                }
            }

            // Seed default prices
            if (!context.ServicePrices.Any())
            {
                context.ServicePrices.AddRange(new List<Models.ServicePrice>
                {
                    new Models.ServicePrice { Id = Guid.NewGuid(), ServiceName = "Wash", PricePerUnit = 50.00m },
                    new Models.ServicePrice { Id = Guid.NewGuid(), ServiceName = "Iron", PricePerUnit = 30.00m },
                    new Models.ServicePrice { Id = Guid.NewGuid(), ServiceName = "Dry Clean", PricePerUnit = 150.00m }
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
