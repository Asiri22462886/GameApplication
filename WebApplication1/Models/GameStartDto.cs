namespace WordGame.Models
{
    public class GameStartDto
    {
        public int Score { get; set; }
        public int Lives { get; set; }
        public string PlayerMode { get; set; } = "";
        public string PlayerModeDisplay { get; set; } = "";
        public List<string> Categories { get; set; } = new();
    }
}
