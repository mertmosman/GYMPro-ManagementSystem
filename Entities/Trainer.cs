using System.Collections.Generic;

namespace GymApp.Web.Entities
{
    public class Trainer : BaseEntity
    {
        public Trainer()
        {
            Services = new List<Service>();
            Gyms = new List<Gym>(); // Artık birden fazla salonu olabilir
            Appointments = new List<Appointment>();
        }

        public string FullName { get; set; }
        public string Specialty { get; set; } // Uzmanlık Alanı (Örn: Kas Gelişimi)

        public List<Gym> Gyms { get; set; }

        public ICollection<Service> Services { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}