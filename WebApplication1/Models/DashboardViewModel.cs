namespace WebApplication1.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = "";
        public int HighestScore { get; set; }
        public int GamesPlayed { get; set; }
        public string CurrentRank { get; set; } = "Beginner";
        public List<RecentGameItemViewModel> RecentGames { get; set; } = new();
    }

    public class RecentGameItemViewModel
    {
        public int Score { get; set; }
        public DateTime PlayedAt { get; set; }
        public string ResultText { get; set; } = "";
    }
}