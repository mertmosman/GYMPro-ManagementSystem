using GymApp.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// SDK Kütüphaneleri
using Google.GenAI;
using Google.GenAI.Types;

namespace GymApp.Web.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly IConfiguration _configuration;

        public AIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlan(AIRequestViewModel model)
        {
            try
            {
                string aiResponse = await GetGeminiResponseWithSDK(model);
                ViewBag.Plan = aiResponse;
            }
            catch (Exception ex)
            {
                // Hata detayını görmek için ex.ToString() kullanıyoruz
                ViewBag.Plan = $"<div class='alert alert-danger'>Hata: {ex.Message}</div>";
            }
            return View("Index", model);
        }

        private async Task<string> GetGeminiResponseWithSDK(AIRequestViewModel model)
        {
            string apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("API Key bulunamadı.");

            // 1. DÜZELTME: Sınıf adı 'GoogleGenAIClient' değil, sadece 'Client'
            var client = new Client(apiKey: apiKey);

            string promptText = $@"
                Sen uzman bir spor antrenörüsün.
                Kullanıcı: {model.Age} yaş, {model.Gender}, {model.Height}cm, {model.Weight}kg. Hedef: {model.Goal}.
                
                GÖREVİN:
                1. Resim varsa analiz et.
                2. Haftalık antrenman programı hazırla.
                3. Beslenme tavsiyeleri ver.
                4. Cevabı HTML formatında (sadece div, ul, li, strong etiketleri) ver. Markdown kullanma.
            ";

            // 2. Parçaları (Parts) Oluştur
            var parts = new List<Part>
            {
                new Part { Text = promptText }
            };

            // 3. Resim İşleme
            if (model.Image != null && model.Image.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await model.Image.CopyToAsync(ms);
                    byte[] imageBytes = ms.ToArray(); // Byte dizisine çevir

                    parts.Add(new Part
                    {
                        InlineData = new Blob
                        {
                            MimeType = model.Image.ContentType,
                            Data = imageBytes // DÜZELTME: Burası byte[] kabul eder
                        }
                    });
                }
            }

            // 4. İsteği Gönder
            // DÜZELTME: contents parametresi 'List<Content>' bekler.
            // DÜZELTME: 'void' hatasını önlemek için await sonucunu değişkene atıyoruz.
            var response = await client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                contents: new List<Content>
                {
                    new Content { Parts = parts }
                }
            );

            // 5. Cevabı Al
            // Google.GenAI yapısına göre cevabı güvenli şekilde çekiyoruz
            if (response?.Candidates != null && response.Candidates.Count > 0)
            {
                var firstCandidate = response.Candidates[0];
                if (firstCandidate.Content?.Parts != null && firstCandidate.Content.Parts.Count > 0)
                {
                    string resultText = firstCandidate.Content.Parts[0].Text;
                    return resultText.Replace("```html", "").Replace("```", "");
                }
            }

            return "Yapay zekadan cevap alınamadı.";
        }
    }
}