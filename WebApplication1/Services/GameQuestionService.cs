using WordGame.Infrastructure;
using WordGame.Models;

namespace WordGame.Services
{
    public class GameQuestionService : IGameQuestionService
    {
        private readonly IEnumerable<IWordProvider> _wordProviders;
        private static readonly Random _random = new Random();

        public GameQuestionService(IEnumerable<IWordProvider> wordProviders)
        {
            _wordProviders = wordProviders;
        }

        public async Task<GameQuestionDto> GenerateQuestionAsync(string category, string playerMode, string? previousWord = null)
        {
            category = string.IsNullOrWhiteSpace(category) ? "animals" : category.ToLower();
            playerMode = GameModes.Normalize(playerMode);

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
                .Where(word => IsValidWord(word, playerMode))
                .Select(ToTitleCase)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (words.Count == 0)
                throw new Exception($"No valid words found for category '{category}' in '{playerMode}' mode.");

            var selectionPool = words;
            if (!string.IsNullOrWhiteSpace(previousWord) && words.Count > 1)
            {
                var nonRepeatedWords = words
                    .Where(word => !string.Equals(word, previousWord, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (nonRepeatedWords.Count > 0)
                {
                    selectionPool = nonRepeatedWords;
                }
            }

            var word = selectionPool[_random.Next(selectionPool.Count)];
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

        private static bool IsValidWord(string? word, string playerMode)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            word = word.Trim();
            var (minLength, maxLength) = GameModes.GetWordLengthRange(playerMode);

            if (word.Length < minLength || word.Length > maxLength)
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
