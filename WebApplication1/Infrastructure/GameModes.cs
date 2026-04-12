namespace WordGame.Infrastructure
{
    public static class GameModes
    {
        public const string Child = "child";
        public const string Adult = "adult";

        public static readonly IReadOnlyDictionary<string, string[]> Categories =
            new Dictionary<string, string[]>
            {
                [Child] = new[] { "animals", "fruits" },
                [Adult] = new[] { "animals", "fruits", "objects", "vegetables" }
            };

        public static bool IsValid(string? mode) =>
            string.Equals(mode, Child, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mode, Adult, StringComparison.OrdinalIgnoreCase);

        public static string Normalize(string? mode) =>
            string.Equals(mode, Adult, StringComparison.OrdinalIgnoreCase) ? Adult : Child;

        public static string DisplayName(string? mode) =>
            string.Equals(Normalize(mode), Adult, StringComparison.OrdinalIgnoreCase) ? "Adult" : "Child";

        public static int GetInitialLives(string? mode) =>
            string.Equals(Normalize(mode), Child, StringComparison.OrdinalIgnoreCase) ? 4 : 3;

        public static (int MinLength, int MaxLength) GetWordLengthRange(string? mode) =>
            string.Equals(Normalize(mode), Adult, StringComparison.OrdinalIgnoreCase) ? (7, 10) : (4, 6);

        public static string[] GetAllowedCategories(string? mode) =>
            Categories[Normalize(mode)];
    }
}
