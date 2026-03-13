namespace WebApplication1.Models
{
    public class AnswerRequestDto
    {
        public string Category { get; set; } = "";
        public string OriginalWord { get; set; } = "";
        public string MaskedWord { get; set; } = "";
        public string MissingLetter { get; set; } = "";
        public string SelectedLetter { get; set; } = "";
        public int Score { get; set; }
        public int Lives { get; set; }
    }
}
