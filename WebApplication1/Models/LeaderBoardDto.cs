namespace WordGame.Models
{
    public class LeaderBoardDto
    {
        public string UserName { get; set; } = "";
        public int HighestScore { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
