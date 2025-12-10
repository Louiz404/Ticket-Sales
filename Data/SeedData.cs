using Microsoft.AspNetCore.Identity;

namespace TicketSales.Data
{
    public class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. criar os perfis
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Cliente"))
                await roleManager.CreateAsync(new IdentityRole("Cliente"));

            // 2. criar o usuário admin
            if (await userManager.FindByEmailAsync("admin@ticket.com") == null)
            {
                var admin = new IdentityUser
                {
                    UserName = "admin@ticket.com",
                    Email = "admin@ticket.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Teste123@");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");

                }
                else
                {
                    var erros = string.Join(", ", result.Errors.Select(e => e.Description));

                    throw new Exception($"Erro ao criar Admin: {erros}");
                }
            }

        }
    }
}
