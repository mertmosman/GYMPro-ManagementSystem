using GymApp.Web.Entities;
using Microsoft.AspNetCore.Identity;

namespace GymApp.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            var userManager = service.GetService<UserManager<AppUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            // 1. ROLLERİ KONTROL ET VE OLUŞTUR
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. ADMIN KULLANCISINI OLUŞTUR
            var adminEmail = "b241210350@sakarya.edu.tr"; // Numaranı buraya yaz
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Sistem Yöneticisi",
                    EmailConfirmed = true,
                    // Zorunlu alanları dolduruyoruz (Hata almamak için)
                    BirthDate = DateTime.Now,
                    Gender = "Belirtilmemiş"
                };

                // Şifre: sau
                var result = await userManager.CreateAsync(adminUser, "sau");

                if (result.Succeeded)
                {
                    // Admin rolünü ata
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}