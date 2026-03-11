using System.Net.Http.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class WordApiService : IWordApiService
    {
        private readonly HttpClient _httpClient;

        public WordApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetSimpleWordAsync()
        {
            var url = "https://api.datamuse.com/words?sp=?????&max=20";
            var result = await _httpClient.GetFromJsonAsync<List<DatamuseWord>>(url);

            if (result == null || result.Count == 0)
                return "apple";

            var filtered = result
                .Where(x => !string.IsNullOrWhiteSpace(x.word))
                .Select(x => x.word.ToLower())
                .Where(w => w.All(char.IsLetter))
                .Where(w => w.Length >= 4 && w.Length <= 7)
                .ToList();

            if (filtered.Count == 0)
                return "apple";

            var random = new Random();
            return filtered[random.Next(filtered.Count)];
        }
    }
}