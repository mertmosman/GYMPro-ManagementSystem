using GymApp.Web.Data;
using GymApp.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GymController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GymController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Liste
        public async Task<IActionResult> Index()
        {
            var gyms = await _context.Gyms
                                     .Where(x => !x.IsDeleted)
                                     .Include(g => g.Schedules) // Programları da çekelim (tabloda göstermek istersen)
                                     .ToListAsync();
            return View(gyms);
        }

        // GET: Ekleme Sayfası
        [HttpGet]
        public IActionResult Create()
        {
            var gym = new Gym();
            gym.Schedules = new List<GymSchedule>();

            // Kullanıcıya boş form göstermek yerine, 7 günü varsayılan olarak listeye ekliyoruz.
            // Böylece ekranda 7 tane satır çıkacak.
            var days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                           .OrderBy(d => d == DayOfWeek.Sunday ? 7 : (int)d); // Pzt'den başlasın diye sıraladık

            foreach (var day in days)
            {
                gym.Schedules.Add(new GymSchedule
                {
                    Day = day,
                    OpeningTime = new TimeSpan(9, 0, 0), // Varsayılan 09:00
                    ClosingTime = new TimeSpan(22, 0, 0), // Varsayılan 22:00
                    IsClosed = false
                });
            }

            return View(gym);
        }

        // POST: Kaydetme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Gym gym)
        {
            // ÇÖZÜM BU SATIR:
            // Schedules içindeki 'Gym' nesnesi boş olduğu için hata veriyor, bu hatayı siliyoruz.
            ModelState.Remove("Schedules");
            // Veya daha garantisi: Tüm schedule hatalarını temizle (zaten biz oluşturduk)
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Schedules")).ToList())
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                // ... (Kayıt kodları aynı kalacak) ...
                _context.Gyms.Add(gym);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(gym);
        }

        // GET: Düzenleme Sayfası
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Include(x => x.Schedules) ÇOK ÖNEMLİ. Bunu yapmazsak saatler gelmez.
            var gym = await _context.Gyms
                                    .Include(g => g.Schedules)
                                    .FirstOrDefaultAsync(g => g.Id == id);

            if (gym == null) return NotFound();

            // Eğer eski kayıtlardan dolayı schedule yoksa, oluşturup gönderelim (Hata almamak için)
            if (gym.Schedules == null || !gym.Schedules.Any())
            {
                gym.Schedules = new List<GymSchedule>();
                var days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>()
                          .OrderBy(d => d == DayOfWeek.Sunday ? 7 : (int)d);
                foreach (var day in days)
                {
                    gym.Schedules.Add(new GymSchedule { Day = day, GymId = gym.Id });
                }
            }
            else
            {
                // Var olanları Pazartesi'den başlayacak şekilde sıralayalım
                gym.Schedules = gym.Schedules.OrderBy(d => d.Day == DayOfWeek.Sunday ? 7 : (int)d.Day).ToList();
            }

            return View(gym);
        }

        // POST: Güncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Gym gym)
        {
            if (ModelState.IsValid)
            {
                var existingGym = await _context.Gyms
                                                .Include(g => g.Schedules)
                                                .FirstOrDefaultAsync(x => x.Id == gym.Id);

                if (existingGym != null)
                {
                    // 1. Ana Bilgileri Güncelle
                    existingGym.Name = gym.Name;
                    existingGym.Address = gym.Address;
                    existingGym.UpdatedDate = DateTime.Now;

                    // 2. Saatleri Güncelle
                    // Formdan gelen liste ile veritabanındaki listeyi eşleştiriyoruz
                    foreach (var formSchedule in gym.Schedules)
                    {
                        var dbSchedule = existingGym.Schedules.FirstOrDefault(s => s.Day == formSchedule.Day);
                        if (dbSchedule != null)
                        {
                            dbSchedule.OpeningTime = formSchedule.OpeningTime;
                            dbSchedule.ClosingTime = formSchedule.ClosingTime;
                            dbSchedule.IsClosed = formSchedule.IsClosed;
                        }
                    }

                    _context.Gyms.Update(existingGym);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gym);
        }

        // GET: Silme (Bunu GET yaptım ki link ile çalışsın, JS form post ile uğraşma)
        public async Task<IActionResult> Delete(int id)
        {
            var gym = await _context.Gyms.FindAsync(id);
            if (gym != null)
            {
                gym.IsDeleted = true; // Soft Delete
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}