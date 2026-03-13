using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class GameQuestionService : IGameQuestionService
    {
        private readonly IEnumerable<IWordProvider> _wordProviders;
        private static readonly Random _random = new Random();

        public GameQuestionService(IEnumerable<IWordProvider> wordProviders)
        {
            _wordProviders = wordProviders;
        }

        public async Task<GameQuestionDto> GenerateQuestionAsync(string category)
        {
            category = string.IsNullOrWhiteSpace(category) ? "animals" : category.ToLower();

            var words = new List<string>();

            foreach (var provider in _wordProviders)
            {
                var result = await provider.GetWordsAsync(category, 40);
                if (result != null && result.Any())
                {
                    words.AddRange(result);
                }
            }

            words = words
                .Where(IsValidWord)
                .Select(ToTitleCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (words.Count == 0)
                throw new Exception($"No valid words found for category '{category}'.");

            var word = words[_random.Next(words.Count)];
            var maskedWord = MaskWord(word, out char missingLetter);

            var letters = BuildLetterOptions(missingLetter);

            return new GameQuestionDto
            {
                Category = category,
                OriginalWord = word,
                MaskedWord = maskedWord,
                MissingLetter = missingLetter.ToString().ToLower(),
                Options = letters
            };
        }

        private static List<GameOptionDto> BuildLetterOptions(char correctLetter)
        {
            correctLetter = char.ToLower(correctLetter);

            var used = new HashSet<char> { correctLetter };

            while (used.Count < 3)
            {
                char randomLetter = (char)('a' + _random.Next(0, 26));
                used.Add(randomLetter);
            }

            return used
                .Select(letter => new GameOptionDto
                {
                    Letter = letter.ToString(),
                    ImageUrl = $"/images/letters/{letter}.png",
                    IsCorrect = letter == correctLetter
                })
                .OrderBy(x => Guid.NewGuid())
                .ToList();
        }

        private static string MaskWord(string word, out char missingLetter)
        {
            int index = word.Length <= 4
                ? _random.Next(0, word.Length)
                : _random.Next(1, word.Length - 1);

            missingLetter = word[index];
            return word.Remove(index, 1).Insert(index, "_");
        }

        private static bool IsValidWord(string? word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            word = word.Trim();

            if (word.Length < 4 || word.Length > 10)
                return false;

            if (word.Contains(' '))
                return false;

            if (!word.All(char.IsLetter))
                return false;

            return true;
        }

        private static string ToTitleCase(string word)
        {
            word = word.Trim().ToLower();
            return char.ToUpper(word[0]) + word[1..];
        }
    }
}