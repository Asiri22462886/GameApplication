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

        // Checks whether the selected mode is one of the modes supported by the game.
        public static bool IsValid(string? mode) =>
            string.Equals(mode, Child, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mode, Adult, StringComparison.OrdinalIgnoreCase);

        // Converts mode input into the exact lowercase value used in session and comparisons.
        public static string Normalize(string? mode) =>
            string.Equals(mode, Adult, StringComparison.OrdinalIgnoreCase) ? Adult : Child;

        // Returns the friendly mode name shown in the interface.
        public static string DisplayName(string? mode) =>
            string.Equals(Normalize(mode), Adult, StringComparison.OrdinalIgnoreCase) ? "Adult" : "Child";

        // Gives child mode one extra life while keeping adult mode more challenging.
        public static int GetInitialLives(string? mode) =>
            string.Equals(Normalize(mode), Child, StringComparison.OrdinalIgnoreCase) ? 4 : 3;

        // Defines the word length difficulty for child and adult players.
        public static (int MinLength, int MaxLength) GetWordLengthRange(string? mode) =>
            string.Equals(Normalize(mode), Adult, StringComparison.OrdinalIgnoreCase) ? (7, 10) : (4, 6);

        // Lists the categories available for the selected player mode.
        public static string[] GetAllowedCategories(string? mode) =>
            Categories[Normalize(mode)];
    }
}
