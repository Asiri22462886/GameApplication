namespace WebApplication1.Models
{
    public class MissingLetterViewModelcs
    {
        public string FullWord { get; set; }
        public string DisplayWord { get; set; }
        public int MissingIndex { get; set; }

        public string CorrectImage { get; set; }
        public string WrongImage1 { get; set; }
        public string WrongImage2 { get; set; }

        public int Score { get; set; }
        public int Lives { get; set; }

        public string CorrectOption { get; set; }
        public string WrongOption1 { get; set; }
        public string WrongOption2 { get; set; }

        public string Message { get; set; }
    }
}
