using System.Text.Json;

namespace WebApplication1.Services
{
    public class FoodWordProvider : IWordProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FoodWordProvider(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<string>> GetWordsAsync(string category, int count)
        {
            if (!string.Equals(category, "food", StringComparison.OrdinalIgnoreCase))
                return new List<string>();

            var apiKey = _configuration["Spoonacular:ApiKey"];
            var seeds = new[] { "a", "b", "c", "m", "p", "t" };
            var random = new Random();
            var seed = seeds[random.Next(seeds.Length)];

            var url = $"https://api.spoonacular.com/food/ingredients/search?query={seed}&number=30&apiKey={apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var results = doc.RootElement.GetProperty("results");

            var words = new List<string>();

            foreach (var item in results.EnumerateArray())
            {
                if (!item.TryGetProperty("name", out var nameProp))
                    continue;

                var name = nameProp.GetString()?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (name.Contains(",") || name.Contains("(") || name.Contains(")"))
                    continue;

                if (name.Contains("juice") || name.Contains("sauce") || name.Contains("powder"))
                    continue;

                if (name.Length < 5 || name.Length > 12)
                    continue;

                if (!name.All(char.IsLetter))
                    continue;

                words.Add(ToTitleCase(name));
            }

            return words
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        private static string ToTitleCase(string value)
        {
            return char.ToUpper(value[0]) + value[1..].ToLower();
        }
    }
}