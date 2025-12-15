using GymApp.Web.Data;
using GymApp.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LİSTELEME
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers
                                         .Include(t => t.Gyms)      // DÜZELTME: Gym -> Gyms (Çoğul)
                                         .Include(t => t.Services)
                                         .Where(t => !t.IsDeleted)
                                         .ToListAsync();
            return View(trainers);
        }

        // EKLEME SAYFASI (GET)
        [HttpGet]
        public IActionResult Create()
        {
            // Dropdown değil, Checkbox listesi için tüm salonları gönderiyoruz
            ViewBag.Gyms = _context.Gyms.Where(g => !g.IsDeleted).ToList();

            // Hizmetleri de gönderiyoruz (View tarafında JS ile filtreleyeceğiz)
            ViewBag.Services = _context.Services.Include(s => s.Gym).Where(s => !s.IsDeleted).ToList();

            return View();
        }

        // EKLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trainer trainer, int[] selectedGymIds, int[] selectedServiceIds)
        {
            ModelState.Remove("Gyms");
            ModelState.Remove("Services");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {
                // 1. SALONLARI EKLE
                if (selectedGymIds != null)
                {
                    var gymsToAdd = await _context.Gyms
                                                  .Where(g => selectedGymIds.Contains(g.Id))
                                                  .ToListAsync();
                    trainer.Gyms = gymsToAdd;
                }

                // 2. HİZMETLERİ EKLE (Güvenlik Kontrolü ile)
                if (selectedServiceIds != null)
                {
                    // Sadece seçilen salonlara ait olan hizmetleri filtrele!
                    // Bu mantık "sonradan yapılacak değişikliklerde düzelmesi"ni sağlar.
                    var validServices = await _context.Services
                                                      .Where(s => selectedServiceIds.Contains(s.Id) &&
                                                                  selectedGymIds.Contains(s.GymId)) // <-- KRİTİK KONTROL
                                                      .ToListAsync();
                    trainer.Services = validServices;
                }

                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa listeleri geri yükle
            ViewBag.Gyms = _context.Gyms.Where(g => !g.IsDeleted).ToList();
            ViewBag.Services = _context.Services.Include(s => s.Gym).Where(s => !s.IsDeleted).ToList();
            return View(trainer);
        }

        // DÜZENLEME (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var trainer = await _context.Trainers
                                        .Include(t => t.Gyms)      // Salonları çek
                                        .Include(t => t.Services)  // Hizmetleri çek
                                        .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return NotFound();

            ViewBag.Gyms = await _context.Gyms.Where(g => !g.IsDeleted).ToListAsync();
            ViewBag.Services = await _context.Services.Include(s => s.Gym).Where(s => !s.IsDeleted).ToListAsync();

            // Seçili olanları işaretlemek için ID listeleri
            ViewBag.SelectedGymIds = trainer.Gyms.Select(g => g.Id).ToList();
            ViewBag.SelectedServiceIds = trainer.Services.Select(s => s.Id).ToList();

            return View(trainer);
        }

        // DÜZENLEME (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Trainer trainer, int[] selectedGymIds, int[] selectedServiceIds)
        {
            ModelState.Remove("Gyms");
            ModelState.Remove("Services");
            ModelState.Remove("Appointments");

            if (ModelState.IsValid)
            {
                var existingTrainer = await _context.Trainers
                                                    .Include(t => t.Gyms)
                                                    .Include(t => t.Services)
                                                    .FirstOrDefaultAsync(t => t.Id == trainer.Id);

                if (existingTrainer != null)
                {
                    existingTrainer.FullName = trainer.FullName;
                    existingTrainer.Specialty = trainer.Specialty;
                    existingTrainer.UpdatedDate = DateTime.Now;

                    // 1. SALONLARI GÜNCELLE
                    existingTrainer.Gyms.Clear();
                    if (selectedGymIds != null)
                    {
                        var gyms = await _context.Gyms.Where(g => selectedGymIds.Contains(g.Id)).ToListAsync();
                        existingTrainer.Gyms = gyms;
                    }

                    // 2. HİZMETLERİ GÜNCELLE (Filtreli)
                    existingTrainer.Services.Clear();
                    if (selectedServiceIds != null)
                    {
                        // Eğer kullanıcı bir salonu çıkardıysa, o salona ait hizmet de otomatik düşer.
                        // Çünkü burada "selectedGymIds.Contains(s.GymId)" kontrolü yapıyoruz.
                        var services = await _context.Services
                                                     .Where(s => selectedServiceIds.Contains(s.Id) &&
                                                                 selectedGymIds.Contains(s.GymId))
                                                     .ToListAsync();
                        existingTrainer.Services = services;
                    }

                    _context.Trainers.Update(existingTrainer);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }

            // Hata durumu
            ViewBag.Gyms = _context.Gyms.Where(g => !g.IsDeleted).ToList();
            ViewBag.Services = _context.Services.Include(s => s.Gym).Where(s => !s.IsDeleted).ToList();
            ViewBag.SelectedGymIds = selectedGymIds.ToList();
            ViewBag.SelectedServiceIds = selectedServiceIds.ToList();

            return View(trainer);
        }
        // SİLME İŞLEMİ (Soft Delete)
        // GET ile çalışır (Linke tıklayınca siler)
        public async Task<IActionResult> Delete(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);

            if (trainer != null)
            {
                // Veritabanından silmiyoruz, sadece görünmez yapıyoruz.
                trainer.IsDeleted = true;
                trainer.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}