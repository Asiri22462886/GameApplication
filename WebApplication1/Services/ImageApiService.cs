using System.Net.Http.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class ImageApiService : IImageApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ImageApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetImageForWordAsync(string word)
        {
            try
            {
                var apiKey = _configuration["Pexels:ApiKey"];

                if (string.IsNullOrWhiteSpace(apiKey))
                    return "/images/dummy-correct.jpg";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);

                var url = $"https://api.pexels.com/v1/search?query={Uri.EscapeDataString(word)}&per_page=1";

                var result = await _httpClient.GetFromJsonAsync<PexelsSearchResponse>(url);
                var photo = result?.photos?.FirstOrDefault();

                return photo?.src?.medium ?? "/images/dummy-correct.jpg";
            }
            catch
            {
                return "/images/dummy-correct.jpg";
            }
        }
    }
}