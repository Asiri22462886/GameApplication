using System.Text.Json;

namespace WebApplication1.Services
{
    public class SpoonacularImageProvider : IImageProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SpoonacularImageProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetImageUrlAsync(string category, string word)
        {
            if (!string.Equals(category, "food", StringComparison.OrdinalIgnoreCase))
                return "";

            var apiKey = _configuration["Spoonacular:ApiKey"];
            var url = $"https://api.spoonacular.com/food/ingredients/autocomplete?query={Uri.EscapeDataString(word)}&number=1&metaInformation=true&apiKey={apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return "";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var arr = doc.RootElement;
            if (arr.GetArrayLength() == 0)
                return "";

            var imageName = arr[0].GetProperty("image").GetString();
            if (string.IsNullOrWhiteSpace(imageName))
                return "";

            return $"https://img.spoonacular.com/ingredients_250x250/{imageName}";
        }
    }
}