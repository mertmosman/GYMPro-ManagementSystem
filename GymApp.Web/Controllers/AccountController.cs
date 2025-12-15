using GymApp.Web.Entities;
using GymApp.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager,
                                 SignInManager<AppUser> signInManager,
                                 RoleManager<IdentityRole> roleManager) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager; 
        }

        // --- LOGIN (GİRİŞ) ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Önce kullanıcıyı email ile bul
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    // --- EKLENEN KONTROL: KULLANICI SİLİNMİŞ Mİ? ---
                    if (user.IsDeleted)
                    {
                        // Kullanıcı var ama silinmiş. Girişi engelle.
                        ModelState.AddModelError("", "Bu hesap silinmiştir. Lütfen yönetimle iletişime geçiniz.");
                        return View(model);
                    }
                    // ------------------------------------------------

                    // 2. Şifreyi kontrol et (Silinmemişse buraya geçer)
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Index", "Gym", new { area = "Admin" });
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Email veya şifre hatalı.");
            }
            return View(model);
        }

        // --- REGISTER (KAYIT) ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Bu email ile kayıtlı biri var mı?
                var existingUser = await _userManager.FindByEmailAsync(model.Email);

                if (existingUser != null)
                {
                    // SENARYO A: Kullanıcı var ama SİLİNMİŞ (IsDeleted = true)
                    // Çözüm: Hesabı tekrar aktif et ve bilgilerini güncelle.
                    if (existingUser.IsDeleted)
                    {
                        // Bilgileri güncelle
                        existingUser.IsDeleted = false; // Geri aç
                        existingUser.FullName = model.FullName;
                        existingUser.BirthDate = model.BirthDate;
                        existingUser.Gender = model.Gender;

                        // Şifreyi Güncelle (Eskisini sil, yenisini ekle)
                        var removePassResult = await _userManager.RemovePasswordAsync(existingUser);
                        if (removePassResult.Succeeded)
                        {
                            var addPassResult = await _userManager.AddPasswordAsync(existingUser, model.Password);
                            if (!addPassResult.Succeeded)
                            {
                                // Şifre kurallara uymazsa hata ver
                                foreach (var error in addPassResult.Errors)
                                    ModelState.AddModelError("", error.Description);
                                return View(model);
                            }
                        }

                        // Değişiklikleri kaydet
                        await _userManager.UpdateAsync(existingUser);

                        // Silinmiş rolü varsa tekrar ekle (Garanti olsun)
                        if (!await _userManager.IsInRoleAsync(existingUser, "Member"))
                        {
                            await _userManager.AddToRoleAsync(existingUser, "Member");
                        }

                        // Giriş yap ve yönlendir
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }

                    // SENARYO B: Kullanıcı var ve SİLİNMEMİŞ (Aktif)
                    else
                    {
                        ModelState.AddModelError("", "Bu email adresi zaten kullanımda.");
                        return View(model);
                    }
                }

                // SENARYO C: Kullanıcı hiç yok -> SIFIR KAYIT (Eski kodumuz)
                var newUser = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Member"))
                        await _roleManager.CreateAsync(new IdentityRole("Member"));

                    await _userManager.AddToRoleAsync(newUser, "Member");
                    await _signInManager.SignInAsync(newUser, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // --- LOGOUT (ÇIKIŞ) ---
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // ÇÖZÜM BURADA:
            // new { area = "" } diyerek Admin alanından tamamen çıkıp
            // ana dizindeki Home controller'ı bulmasını sağlıyoruz.
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // --- ERİŞİM ENGELİ ---
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}