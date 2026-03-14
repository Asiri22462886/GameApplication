using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = user.Id;

            var highestScore = await _context.UserHighScores
                .Where(x => x.UserId == userId)
                .Select(x => x.HighestScore)
                .FirstOrDefaultAsync();

            var gamesPlayed = await _context.UserHighScores
                .CountAsync(x => x.UserId == userId);

            var recentGames = await _context.UserHighScores
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.HighestScore)
                .Take(5)
                .Select(x => new RecentGameItemViewModel
                {
                    Score = x.HighestScore,
                    PlayedAt = x.UpdatedAt,
                    ResultText = x.HighestScore >= 10 ? "Great run" : "Keep practicing"
                })
                .ToListAsync();

            var model = new DashboardViewModel
            {
                UserName = user.UserName ?? "Player",
                HighestScore = highestScore,
                GamesPlayed = gamesPlayed,
                CurrentRank = GetRank(highestScore),
                RecentGames = recentGames
            };

            return View(model);
        }

        

        private string GetRank(int highestScore)
        {
            if (highestScore >= 50) return "Master";
            if (highestScore >= 30) return "Pro";
            if (highestScore >= 15) return "Intermediate";
            return "Beginner";
        }


        public IActionResult Privacy()
        {
            return View();
        }
    
    }

}