using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrustGuard.Application.Interfaces;
using TrustGuard.Application.Services;
using TrustGuard.Domain.Entities;
using TrustGuard.Infrastructure.Persistence;
using TrustGuard.Infrastructure.Repositories;
using TrustGuard.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
        b => b.MigrationsAssembly("TrustGuard.Web")));

// Налаштування Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // ВМИКАЄМО обов'язкове підтвердження пошти для логіну
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

// Реєстрація твоїх сервісів
builder.Services.AddHttpClient<IMlService, FastApiMlService>();
builder.Services.AddScoped<INewsCheckRepository, NewsCheckRepository>();
builder.Services.AddScoped<INewsCheckService, NewsCheckService>();
builder.Services.AddScoped<IEmailSender, EmailService>();
builder.Services.AddScoped<IFileParserService, FileParserService>();
builder.Services.AddScoped<IDomainRepository, DomainRepository>();
builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<IDomainTrustRepository, DomainTrustRepository>();
builder.Services.AddScoped<IDomainScoringService, DomainScoringService>();

// ДОДАНО: Реєстрація парсера посилань
builder.Services.AddScoped<IUrlParserService, UrlParserService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ОБОВ'ЯЗКОВО: спочатку Authentication, потім Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();