using GymApp.Web.Data;
using GymApp.Web.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabaný Baðlantýsý (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity (Üyelik Sistemi) Ayarlarý
// AppUser sýnýfýmýzý ve IdentityRole sýnýfýný kullanacaðýmýzý belirtiyoruz.
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Geliþtirme kolaylýðý için þifre kurallarýný basitleþtiriyoruz
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3; // En az 3 karakter yeterli
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. MVC Servisleri
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- HTTP Request Pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot klasörünü açar (CSS/JS için)

app.UseRouting();

// 4. Yetkilendirme Sýralamasý (Önce kimlik doðrula, sonra yetki ver)
app.UseAuthentication();
app.UseAuthorization();

// 5. Rota Ayarlarý (Route Map)
// Admin Paneli için Area Rotasý
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Standart Kullanýcý Rotasý
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Seeder'ý çalýþtýr (Admin ve Rolleri oluþturur)
        await GymApp.Web.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Hata: " + ex.Message);
    }
}

app.Run();