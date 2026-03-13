using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class UserHighScore
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = "";

        public int HighestScore { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
