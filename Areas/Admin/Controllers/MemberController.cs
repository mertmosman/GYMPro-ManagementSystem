using GymApp.Web.Entities;
using GymApp.Web.ViewModels; // RegisterViewModel'i burada da kullanabiliriz veya yeni yapabiliriz
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MemberController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public MemberController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // 1. LİSTELEME
        public async Task<IActionResult> Index()
        {
            // Silinmemiş kullanıcıları getir
            var users = await _userManager.Users
                                          .Where(u => !u.IsDeleted)
                                          .ToListAsync();
            return View(users);
        }

        // 2. EKLEME (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. EKLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            // Admin panelinden eklerken ConfirmPassword zorunluluğunu kaldırabilirsin 
            // ama RegisterViewModel kullanıyorsak aynen uymalıyız.
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    BirthDate = model.BirthDate,
                    Gender = model.Gender,
                    EmailConfirmed = true // Admin eklediği için onaylı sayalım
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Member");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // 4. DÜZENLEME (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // 5. DÜZENLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUser model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email; // Email değişirse username de değişsin
                user.BirthDate = model.BirthDate;
                user.Gender = model.Gender;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // 6. SİLME (Soft Delete + Kick User)
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (User.Identity.Name == user.UserName)
                {
                    return RedirectToAction(nameof(Index));
                }

                user.IsDeleted = true;

                // EKLENEN KISIM: SecurityStamp'i güncelle
                // Bu işlem, kullanıcının mevcut oturum çerezini geçersiz kılar.
                // Kullanıcı bir sonraki tıklamasında otomatik olarak çıkış yapmış olur.
                await _userManager.UpdateSecurityStampAsync(user);

                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}