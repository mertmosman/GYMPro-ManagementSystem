namespace GymApp.Web.Entities
{
    public class Appointment : BaseEntity
    {
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending"; // Onay Bekliyor, Onaylandı, İptal

        // Hizmetin fiyatı değişse bile, o randevunun alındığı fiyattır.
        public decimal PaidPrice { get; set; }

        // Standart hizmet süresinden farklı olabilir (Çoklu slot seçilirse)
        public int DurationMinutes { get; set; }

        // Kim aldı? (AppUser Id string türündedir)
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        // Kime aldı?
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        // Hangi hizmet?
        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
