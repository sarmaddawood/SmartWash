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

            // Create a default Admin user
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string adminEmail = "admin@smartwash.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed default prices
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
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
