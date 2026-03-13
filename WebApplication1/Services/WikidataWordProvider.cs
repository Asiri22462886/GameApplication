using System.Text.Json;
using System.Web;

namespace WebApplication1.Services
{
    public class WikidataWordProvider : IWordProvider
    {
        private readonly HttpClient _httpClient;

        public WikidataWordProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetWordsAsync(string category, int count)
        {
            var sparql = BuildQuery(category, count * 5);
            if (string.IsNullOrWhiteSpace(sparql))
                return new List<string>();

            var url = "https://query.wikidata.org/sparql?format=json&query=" + HttpUtility.UrlEncode(sparql);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "GameApp/1.0");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var bindings = doc.RootElement
                .GetProperty("results")
                .GetProperty("bindings");

            var words = new List<string>();

            foreach (var row in bindings.EnumerateArray())
            {
                var word = row.GetProperty("label").GetProperty("value").GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(word))
                    continue;

                if (word.Contains(' '))
                    continue;

                if (word.Length < 4 || word.Length > 10)
                    continue;

                if (!word.All(char.IsLetter))
                    continue;

                words.Add(ToTitleCase(word));
            }

            return words
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        private static string BuildQuery(string category, int limit)
        {
            return category.ToLower() switch
            {
                "animals" => $@"
SELECT DISTINCT ?label WHERE {{
  VALUES ?item {{
    wd:Q140 wd:Q144 wd:Q146 wd:Q726 wd:Q7378 wd:Q5113 wd:Q25324 wd:Q25265
    wd:Q10811 wd:Q12204 wd:Q25378 wd:Q33602 wd:Q25376 wd:Q28038 wd:Q185044
  }}
  ?item rdfs:label ?label .
  FILTER(LANG(?label) = ""en"")
}}
LIMIT {limit}",

                "fruits" => $@"
SELECT DISTINCT ?label WHERE {{
  VALUES ?item {{
    wd:Q89 wd:Q503 wd:Q13191 wd:Q196 wd:Q1560 wd:Q8072 wd:Q15803 wd:Q41291
    wd:Q10978 wd:Q14975 wd:Q34807 wd:Q47092
  }}
  ?item rdfs:label ?label .
  FILTER(LANG(?label) = ""en"")
}}
LIMIT {limit}",

                "vegetables" => $@"
SELECT DISTINCT ?label WHERE {{
  VALUES ?item {{
    wd:Q81 wd:Q90 wd:Q10998 wd:Q11004 wd:Q42057 wd:Q165437 wd:Q5090 wd:Q34998
    wd:Q33901 wd:Q31338 wd:Q34108 wd:Q7533
  }}
  ?item rdfs:label ?label .
  FILTER(LANG(?label) = ""en"")
}}
LIMIT {limit}",

                "objects" => $@"
SELECT DISTINCT ?label WHERE {{
  VALUES ?item {{
    wd:Q987767 wd:Q132137 wd:Q260521 wd:Q484677 wd:Q2101 wd:Q5300
    wd:Q11472 wd:Q193231 wd:Q3249263 wd:Q171509
  }}
  ?item rdfs:label ?label .
  FILTER(LANG(?label) = ""en"")
}}
LIMIT {limit}",

                _ => ""
            };
        }

        private static bool IsSimpleCommonWord(string word)
        {
            var banned = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "animal", "animals", "object", "objects", "organism", "species",
        "taxonomy", "clade", "ecdysozoa", "opiliones", "cafeteria",
        "canidae", "felidae", "bovidae", "equidae", "cervidae",
        "suidae", "hominidae", "muridae", "ursidae", "mustelidae"
    };

            if (banned.Contains(word))
                return false;

            // remove many scientific-looking words
            if (word.EndsWith("idae", StringComparison.OrdinalIgnoreCase))
                return false;

            if (word.EndsWith("inae", StringComparison.OrdinalIgnoreCase))
                return false;

            if (word.EndsWith("oidea", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private static string ToTitleCase(string value)
        {
            return char.ToUpper(value[0]) + value[1..].ToLower();
        }
    }
}