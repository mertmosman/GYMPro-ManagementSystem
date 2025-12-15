using GymApp.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Controllers
{
    // Bu etiketler buranın bir API olduğunu belirtir
    [Route("api/[controller]")]
    [ApiController]
    public class GymApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GymApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Tüm Salonları Getir
        // İstek: GET /api/gymapi/gyms
        [HttpGet("gyms")]
        public IActionResult GetGyms()
        {
            var gyms = _context.Gyms
                               .Where(g => !g.IsDeleted)
                               .Select(g => new {
                                   g.Id,
                                   g.Name,
                                   g.Address,
                                   TrainerCount = g.Trainers.Count
                               })
                               .ToList();
            return Ok(gyms);
        }

        // 2. Tüm Antrenörleri Getir (Filtreleme Özellikli - LINQ)
        // İstek: GET /api/gymapi/trainers?specialty=Yoga
        [HttpGet("trainers")]
        public IActionResult GetTrainers([FromQuery] string? specialty)
        {
            var query = _context.Trainers
                                .Where(t => !t.IsDeleted)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(specialty))
            {
                // LINQ ile filtreleme
                query = query.Where(t => t.Specialty.Contains(specialty));
            }

            var trainers = query.Select(t => new {
                t.Id,
                t.FullName,
                t.Specialty,
                ServiceCount = t.Services.Count
            }).ToList();

            return Ok(trainers);
        }

        // 3. İstatistik Getir
        // İstek: GET /api/gymapi/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = new
            {
                TotalMembers = await _context.Users.CountAsync(),
                TotalAppointments = await _context.Appointments.CountAsync(),
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == "Beklemede")
            };
            return Ok(stats);
        }
    }
}