namespace WebApplication1.Models
{
    public class GameQuestionDto
    {
        public string Category { get; set; } = "";
        public string OriginalWord { get; set; } = "";
        public string MaskedWord { get; set; } = "";
        public string MissingLetter { get; set; } = "";
        public List<GameOptionDto> Options { get; set; } = new();
    }
}
