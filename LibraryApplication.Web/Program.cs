using LibraryApplication.Domain.Identity;
using LibraryApplication.Repository.Data;
using LibraryApplication.Repository.Implementation;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.API;
using LibraryApplication.Service.Implementation;
using LibraryApplication.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
internal class Program
{
    public class NullEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Do nothing, just return completed task, implement later 
            return Task.CompletedTask;
        }
    }
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<LibraryApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();

        builder.Services.AddSingleton<IEmailSender, NullEmailSender>();

        builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        builder.Services.AddRazorPages();

        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddTransient<ICategoryService, CategoryService>();
        builder.Services.AddTransient<IPublisherService, PublisherService>();
        builder.Services.AddTransient<IAuthorService, AuthorService>();
        builder.Services.AddTransient<IBookService, BookService>();
        builder.Services.AddTransient<ILoanService, LoanService>();
        builder.Services.AddTransient<IReservationService, ReservationService>();
        builder.Services.AddHttpClient<GoogleBooksClient>();
        builder.Services.AddTransient<GoogleBookImporter>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Ensure database is seeded with roles
        void SeedRolesAndRedirect()
        {
            using var scope = app.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<LibraryApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            Task.Run(async () =>
            {
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }).GetAwaiter().GetResult();
        }

        SeedRolesAndRedirect();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Books}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.MapGet("/", async (UserManager<LibraryApplicationUser> userManager) =>
        {
            var admins = await userManager.GetUsersInRoleAsync("Admin");
            if (!admins.Any())
            {
                return Results.Redirect("/Identity/AdminSetup/CreateAdmin");
            }
            return Results.Redirect("/Books/Index");
        });

        app.Run();
    }
}