using GymApp.Web.Data;
using GymApp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GymApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Veritabanýndan verileri çekip ViewBag ile sayfaya taþýyoruz

            // 1. Ýlk 3 Spor Salonunu Getir
            ViewBag.Gyms = await _context.Gyms
                                         .Where(g => !g.IsDeleted)
                                         .Take(3)
                                         .ToListAsync();

            // 2. Ýlk 4 Antrenörü Getir
            ViewBag.Trainers = await _context.Trainers
                                             .Where(t => !t.IsDeleted)
                                             .Take(4)
                                             .ToListAsync();

            // 3. Ýstatistikleri (Toplam sayýlarý) Getir
            // Anonim obje (new { ... }) olarak gönderiyoruz
            ViewBag.Stats = new
            {
                TotalGyms = await _context.Gyms.CountAsync(),
                TotalTrainers = await _context.Trainers.CountAsync()
            };

            return View();
        }

        // Hata Sayfasý (Proje oluþturulurken otomatik gelir)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}