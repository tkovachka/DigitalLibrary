using LibraryApplication.Domain.Identity;
using LibraryApplication.Repository.Data;
using LibraryApplication.Repository.Implementation;
using LibraryApplication.Repository.Interface;
using LibraryApplication.Service.API;
using LibraryApplication.Service.Implementation;
using LibraryApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<LibraryApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

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

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Books}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }
}