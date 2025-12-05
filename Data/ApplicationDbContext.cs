using System.Collections.Generic;
using System.Reflection.Emit;
using GymApp.Web.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Web.Data
{
    // IdentityDbContext<AppUser> kullanıyoruz çünkü kendi User sınıfımız var
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<GymSchedule> GymSchedules { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Trainer <-> Service Çoka-Çok İlişkisi
            builder.Entity<Trainer>()
                .HasMany(t => t.Services)
                .WithMany(s => s.Trainers)
                .UsingEntity<Dictionary<string, object>>(
                    "TrainerServices",
                    j => j.HasOne<Service>().WithMany().HasForeignKey("ServicesId").OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Trainer>().WithMany().HasForeignKey("TrainersId").OnDelete(DeleteBehavior.Restrict)
                );

            // Para Birimi Hassasiyet Ayarları (Decimal Precision)
            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            // --- YENİ EKLENEN AYAR ---
            builder.Entity<Appointment>()
                .Property(a => a.PaidPrice)
                .HasColumnType("decimal(18,2)");
            // -------------------------

            // Silme Davranışları (Restrict)
            builder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}