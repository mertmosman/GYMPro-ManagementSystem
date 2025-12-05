using System;
namespace GymApp.Web.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now; // Kayıt anı
        public DateTime? UpdatedDate { get; set; } // Güncelleme anı
        public bool IsDeleted { get; set; } = false; // Soft Delete (Silindi mi?)
    }
}
