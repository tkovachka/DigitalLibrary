using LibraryApplication.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryApplication.Web.Areas.Identity.Pages.AdminSetup
{
    public class CreateAdminModel : PageModel
    {
        private readonly UserManager<LibraryApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateAdminModel(UserManager<LibraryApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool CanRegisterAdmin { get; set; } = true;

        public async Task OnGetAsync()
        {
            // Disable page if an admin already exists
            CanRegisterAdmin = !await AdminExistsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || Password != ConfirmPassword)
            {
                if (Password != ConfirmPassword)
                    ModelState.AddModelError("", "Passwords do not match");
                return Page();
            }

            if (await AdminExistsAsync())
            {
                CanRegisterAdmin = false;
                return Page();
            }

            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var adminUser = new LibraryApplicationUser
            {
                UserName = Email,
                Email = Email,
                Name = "Admin",
                Surname = "Account",
                CreationDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            var result = await _userManager.CreateAsync(adminUser, Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                return Page();
            }

            await _userManager.AddToRoleAsync(adminUser, "Admin");

            // Optionally, redirect to login page
            return RedirectToPage("/Index");
        }

        private async Task<bool> AdminExistsAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
                return false;

            var usersInAdminRole = await _userManager.GetUsersInRoleAsync("Admin");
            return usersInAdminRole.Any();
        }
    }
}
