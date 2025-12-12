using GymApp.Web.Data;
using GymApp.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Controllers
{
    [Authorize(Roles = "Member,Admin")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Randevularım
        public async Task<IActionResult> MyList()
        {
            var user = await _userManager.GetUserAsync(User);
            var appointments = await _context.Appointments
                                             .Include(a => a.Trainer)
                                             .Include(a => a.Service)
                                             .Where(a => a.AppUserId == user.Id && !a.IsDeleted)
                                             .OrderByDescending(a => a.AppointmentDate)
                                             .ToListAsync();
            return View(appointments);
        }

        // 2. Randevu Alma Sayfası
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Gyms = new SelectList(_context.Gyms.Where(g => !g.IsDeleted), "Id", "Name");
            return View();
        }

        // --- API METOTLARI ---
        [HttpGet]
        public IActionResult GetServicesByGym(int gymId)
        {
            var services = _context.Services
                                   .Where(s => s.GymId == gymId && !s.IsDeleted)
                                   .Select(s => new { s.Id, s.Name, s.Price, s.DurationMinutes })
                                   .ToList();
            return Json(services);
        }

        [HttpGet]
        public IActionResult GetTrainersByService(int serviceId)
        {
            var trainers = _context.Trainers
                                   .Where(t => t.Services.Any(s => s.Id == serviceId) && !t.IsDeleted)
                                   .Select(t => new { t.Id, t.FullName, t.Specialty })
                                   .ToList();
            return Json(trainers);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int trainerId, int serviceId, DateTime date)
        {
            var service = await _context.Services
                                        .Include(s => s.Gym).ThenInclude(g => g.Schedules)
                                        .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null) return BadRequest("Hizmet bulunamadı.");

            var dayOfWeek = date.DayOfWeek;
            var schedule = service.Gym.Schedules.FirstOrDefault(s => s.Day == dayOfWeek);

            if (schedule == null || schedule.IsClosed)
            {
                return Json(new { message = "Salon bu tarihte kapalı." });
            }

            var existingAppointments = await _context.Appointments
                                                     .Where(a => a.TrainerId == trainerId
                                                              && a.AppointmentDate.Date == date.Date
                                                              && !a.IsDeleted)
                                                     .ToListAsync();

            var availableSlots = new List<string>();
            TimeSpan currentInfo = schedule.OpeningTime;
            TimeSpan closingTime = schedule.ClosingTime;
            int duration = service.DurationMinutes;

            while (currentInfo.Add(TimeSpan.FromMinutes(duration)) <= closingTime)
            {
                var slotStart = currentInfo;
                var slotEnd = currentInfo.Add(TimeSpan.FromMinutes(duration));

                bool isBooked = existingAppointments.Any(a =>
                    (slotStart >= a.AppointmentDate.TimeOfDay && slotStart < a.AppointmentDate.TimeOfDay.Add(TimeSpan.FromMinutes(a.DurationMinutes)))
                    ||
                    (slotEnd > a.AppointmentDate.TimeOfDay && slotEnd <= a.AppointmentDate.TimeOfDay.Add(TimeSpan.FromMinutes(a.DurationMinutes)))
                );

                if (!isBooked)
                {
                    if (date.Date > DateTime.Now.Date || (date.Date == DateTime.Now.Date && slotStart > DateTime.Now.TimeOfDay))
                    {
                        availableSlots.Add(slotStart.ToString(@"hh\:mm"));
                    }
                }
                currentInfo = currentInfo.Add(TimeSpan.FromMinutes(duration));
            }

            return Json(availableSlots);
        }

        // --- 3. KAYIT İŞLEMİ (GÜNCELLENEN KISIM) ---
        [HttpPost]
        // times: Frontend'den gelen çoklu saat listesi
        public async Task<IActionResult> Create(int trainerId, int serviceId, DateTime date, List<TimeSpan> times)
        {
            var user = await _userManager.GetUserAsync(User);
            var service = await _context.Services.FindAsync(serviceId);

            // Seçilen her saat için ayrı bir randevu oluşturuyoruz
            foreach (var time in times)
            {
                DateTime appointmentDateTime = date.Date.Add(time);

                var appointment = new Appointment
                {
                    AppUserId = user.Id,
                    TrainerId = trainerId,
                    ServiceId = serviceId,
                    AppointmentDate = appointmentDateTime,
                    Status = "Beklemede",
                    CreatedDate = DateTime.Now,

                    // Snapshot: Her bir randevu kendi seans fiyatıyla ve süresiyle kaydedilir
                    PaidPrice = service.Price,
                    DurationMinutes = service.DurationMinutes
                };

                _context.Appointments.Add(appointment);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyList));
        }
    }
}