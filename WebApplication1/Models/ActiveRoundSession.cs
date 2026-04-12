namespace WordGame.Models
{
    public class ActiveRoundSession
    {
        public string Category { get; set; } = "";
        public string OriginalWord { get; set; } = "";
        public string MaskedWord { get; set; } = "";
        public string MissingLetter { get; set; } = "";
    }
}
