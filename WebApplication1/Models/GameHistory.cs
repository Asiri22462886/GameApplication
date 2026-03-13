using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class GameHistory
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = "";

        public string Category { get; set; } = "";

        public string OriginalWord { get; set; } = "";

        public string MaskedWord { get; set; } = "";

        public string MissingLetter { get; set; } = "";

        public string SelectedLetter { get; set; } = "";

        public bool IsCorrect { get; set; }

        public int ScoreAfterRound { get; set; }

        public int LivesAfterRound { get; set; }

        public DateTime PlayedAt { get; set; } = DateTime.Now;
    }
}
