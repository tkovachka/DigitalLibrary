using LibraryApplication.Domain.Identity;
using Microsoft.AspNetCore.Identity;

public static class IdentitySeed
{
    public static async Task SeedRolesAndAdminAsync(UserManager<LibraryApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. Check if admin user exists
        var adminEmail = "admin@digitallibrary.com"; // can be configurable
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            Console.WriteLine("No admin account found. Please enter a secure password for the admin:");
            string password;
            while (true)
            {
                password = ReadPasswordFromConsole();
                var passwordValidator = new PasswordValidator<LibraryApplicationUser>();
                var result = await passwordValidator.ValidateAsync(userManager, null, password);
                if (result.Succeeded) break;
                Console.WriteLine(string.Join("\n", result.Errors.Select(e => e.Description)));
            }

            adminUser = new LibraryApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Admin",
                Surname = "Account",
                CreationDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            var createResult = await userManager.CreateAsync(adminUser, password);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("Admin account created successfully!");
            }
            else
            {
                Console.WriteLine("Failed to create admin account:");
                foreach (var error in createResult.Errors)
                    Console.WriteLine(error.Description);
            }
        }
    }

    private static string ReadPasswordFromConsole()
    {
        string password = "";
        ConsoleKeyInfo info;
        do
        {
            info = Console.ReadKey(true);
            if (info.Key != ConsoleKey.Enter)
            {
                password += info.KeyChar;
                Console.Write("*");
            }
        } while (info.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return password;
    }
}

