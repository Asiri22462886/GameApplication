namespace WebApplication1.Models
{
    public class GameHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Word { get; set; }
        public string DisplayWord { get; set; }
        public string SelectedOption { get; set; }
        public bool IsCorrect { get; set; }
        public int ScoreAfterRound { get; set; }
        public int LivesAfterRound { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}
