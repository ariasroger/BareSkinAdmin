
using Microsoft.AspNetCore.Identity;

namespace AdminBareSkin.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration)
        {
            using var scope = services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<IdentityUser>>();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "SuperAdmin", "Dueño" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                        throw new Exception("No se pudo crear el rol: " + role);
                }
            }

            var accounts = new[]
            {
                new
                {
                    Email = "Roger8425@gmail.com",
                    Role = "SuperAdmin",
                    PasswordKey = "AdminUsers:Roger:Password"
                },
                new
                {
                    Email = "Fau@gmail.com",
                    Role = "Dueño",
                    PasswordKey = "AdminUsers:Royner:Password"
                },
                new
                {
                    Email = "Maria@gmail.com",
                    Role = "Dueño",
                    PasswordKey = "AdminUsers:Maria:Password"
                }
            };

            foreach (var account in accounts)
            {
                var user = await userManager.FindByEmailAsync(account.Email);

                if (user == null)
                {
                    var password = configuration[account.PasswordKey];

                    if (string.IsNullOrWhiteSpace(password))
                        throw new Exception(
                            $"Falta configurar la contraseña para {account.Email}");

                    user = new IdentityUser
                    {
                        UserName = account.Email,
                        Email = account.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ",
                            result.Errors.Select(e => e.Description));

                        throw new Exception(
                            $"No se pudo crear {account.Email}: {errors}");
                    }
                }

                if (!await userManager.IsInRoleAsync(user, account.Role))
                {
                    var result = await userManager.AddToRoleAsync(
                        user, account.Role);

                    if (!result.Succeeded)
                        throw new Exception(
                            $"No se pudo asignar el rol a {account.Email}");
                }
            }
        }
    }
}
