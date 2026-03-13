using System.Text.Json;

namespace WebApplication1.Services
{
    public class PexelsImageProvider : IImageProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PexelsImageProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetImageUrlAsync(string category, string word)
        {
            if (string.Equals(category, "food", StringComparison.OrdinalIgnoreCase))
                return "";

            var apiKey = _configuration["Pexels:ApiKey"];

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.pexels.com/v1/search?query={Uri.EscapeDataString(word)}&per_page=1");

            request.Headers.Add("Authorization", apiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return "";

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var photos = doc.RootElement.GetProperty("photos");
            if (photos.GetArrayLength() == 0)
                return "";

            return photos[0].GetProperty("src").GetProperty("medium").GetString() ?? "";
        }
    }
}