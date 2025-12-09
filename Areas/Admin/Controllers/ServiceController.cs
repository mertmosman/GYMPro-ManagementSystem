using GymApp.Web.Data;
using GymApp.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Dropdown (SelectListItem) için gerekli
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Listeleme
        public async Task<IActionResult> Index()
        {
            // Hizmetleri getirirken bağlı olduğu Salonu da (Gym) getiriyoruz (Include)
            var services = await _context.Services
                                         .Include(s => s.Gym)
                                         .Where(s => !s.IsDeleted)
                                         .ToListAsync();
            return View(services);
        }

        // Ekleme Sayfası (GET)
        [HttpGet]
        public IActionResult Create()
        {
            // Dropdown için salonları ViewBag'e atıyoruz
            ViewBag.Gyms = new SelectList(_context.Gyms.Where(g => !g.IsDeleted), "Id", "Name");
            return View();
        }

        // Kaydetme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            // Validasyon sırasında Gym nesnesini sormasın, sadece GymId yeterli
            ModelState.Remove("Gym");
            ModelState.Remove("Trainers");

            if (ModelState.IsValid)
            {
                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata olursa dropdown'ı tekrar doldur
            ViewBag.Gyms = new SelectList(_context.Gyms.Where(g => !g.IsDeleted), "Id", "Name");
            return View(service);
        }

        // GÜNCELLEME SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            // Dropdown'ı doldururken, mevcut salonun seçili gelmesini sağlıyoruz (service.GymId)
            ViewBag.Gyms = new SelectList(_context.Gyms.Where(g => !g.IsDeleted), "Id", "Name", service.GymId);

            return View(service);
        }

        // GÜNCELLEMEYİ KAYDET (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service service)
        {
            // Validasyon engellerini kaldır
            ModelState.Remove("Gym");
            ModelState.Remove("Trainers");

            if (ModelState.IsValid)
            {
                var existingService = await _context.Services.FindAsync(service.Id);
                if (existingService != null)
                {
                    existingService.Name = service.Name;
                    existingService.DurationMinutes = service.DurationMinutes;
                    existingService.Price = service.Price;
                    existingService.GymId = service.GymId; // Salonu da değiştirebilir
                    existingService.UpdatedDate = DateTime.Now;

                    _context.Services.Update(existingService);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa dropdown'ı tekrar doldur
            ViewBag.Gyms = new SelectList(_context.Gyms.Where(g => !g.IsDeleted), "Id", "Name", service.GymId);
            return View(service);
        }

        // Silme İşlemi
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}