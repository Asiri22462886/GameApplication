namespace WordGame.Models
{
    public class GamePageViewModel
    {
        public string PlayerMode { get; set; } = "";
        public string PlayerModeDisplay { get; set; } = "";
        public int InitialLives { get; set; }
        public List<string> Categories { get; set; } = new();
    }
}
