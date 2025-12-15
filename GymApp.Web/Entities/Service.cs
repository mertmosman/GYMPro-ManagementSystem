namespace GymApp.Web.Entities
{
    public class Service : BaseEntity
    {
        public string Name { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }

        // İlişkiler
        public int GymId { get; set; }
        public Gym? Gym { get; set; }

        // Çoka-Çok İlişki: Bu hizmeti veren antrenörler
        public ICollection<Trainer>? Trainers { get; set; }
    }
}
