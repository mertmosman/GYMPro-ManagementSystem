using System.ComponentModel.DataAnnotations;

namespace GymApp.Web.ViewModels
{
    public class AIRequestViewModel
    {
        // --- YENİ EKLENEN: Fotoğraf Yükleme ---
        [Display(Name = "Vücut Fotoğrafınız (İsteğe Bağlı)")]
        public IFormFile? Image { get; set; }
        // -------------------------------------

        [Display(Name = "Yaşınız")]
        [Required]
        public int Age { get; set; }

        [Display(Name = "Boy (cm)")]
        [Required]
        public int Height { get; set; }

        [Display(Name = "Kilo (kg)")]
        [Required]
        public int Weight { get; set; }

        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; }

        [Display(Name = "Hedef")]
        public string Goal { get; set; }
    }
}