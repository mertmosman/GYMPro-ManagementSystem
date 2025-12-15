using Microsoft.AspNetCore.Identity;
using System;

namespace GymApp.Web.Entities
{
    // IdentityUser zaten Id, Email, PasswordHash, PhoneNumber içerir.
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; } // Yapay zeka diyet hesabı için yaş
        public string Gender { get; set; }      // Yapay zeka için cinsiyet
        public bool IsDeleted { get; set; } = false;
    }
}
