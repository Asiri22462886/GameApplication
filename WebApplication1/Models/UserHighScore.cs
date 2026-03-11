namespace WebApplication1.Models
{
    public class UserHighScore
    {

        public int Id { get; set; }
        public string UserId { get; set; }
        public int HighestScore { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
