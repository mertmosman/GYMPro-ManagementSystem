using GymApp.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜM RANDEVULARI LİSTELE
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.AppUser)  // Randevuyu alan üyeyi getir
                .Include(a => a.Trainer)  // Eğitmeni getir
                .Include(a => a.Service)  // Hizmeti getir
                .OrderByDescending(a => a.CreatedDate) // En yeni en üstte
                .ToListAsync();

            return View(appointments);
        }

        // 2. ONAYLA (Approve)
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Onaylandı";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 3. REDDET (Reject)
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Reddedildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}