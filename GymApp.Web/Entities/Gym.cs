using System;
using System.Collections.Generic; // BU SATIR ŞART! Yoksa List hatası verir.

namespace GymApp.Web.Entities
{
    public class Gym : BaseEntity
    {
        // Constructor (Yapıcı Metot)
        // Bu sınıf new'lendiğinde listeler de boş olarak oluşsun diye yapıyoruz.
        public Gym()
        {
            Schedules = new List<GymSchedule>();
            Trainers = new List<Trainer>(); // List olarak başlattık
            Services = new List<Service>();
        }

        public List<Trainer> Trainers { get; set; } // ICollection yerine List yaptık
        public string Name { get; set; }
        public string Address { get; set; }

        // Çalışma Programı (List türünde olmalı ki View'da [i] ile erişebilelim)
        public List<GymSchedule> Schedules { get; set; }

        // Diğer ilişkiler (ICollection kalabilir ama List yapmak daha güvenlidir)
        public ICollection<Service> Services { get; set; }
    }
}