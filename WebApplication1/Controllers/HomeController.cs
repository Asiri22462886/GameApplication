using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordGame.Data;
using WordGame.Infrastructure;
using WordGame.Models;

namespace WordGame.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        // Receives the database context and Identity user manager through dependency injection.
        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // player's dashboard.
        public async Task<IActionResult> Index()
        {
            if (!HasPlayerMode())
            {
                return RedirectToAction(nameof(SelectMode));
            }

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

            var gamesPlayed = await _context.GameHistories
                .CountAsync(x => x.UserId == userId);

            var recentGames = await _context.GameHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PlayedAt)
                .Take(5)
                .Select(x => new RecentGameItemViewModel
                {
                    Score = x.ScoreAfterRound,
                    PlayedAt = x.PlayedAt,
                    ResultText = x.IsCorrect ? "Correct answer" : "Wrong answer"
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

            ViewBag.PlayerMode = GameModes.DisplayName(HttpContext.Session.GetString(GameSessionKeys.PlayerMode));

            return View(model);
        }

        // Shows the page where the player chooses child or adult.
        [HttpGet]
        
        public IActionResult SelectMode()
        {
            return View(new PlayerModeSelectionViewModel
            {
                SelectedMode = HttpContext.Session.GetString(GameSessionKeys.PlayerMode) ?? ""
            });
        }

        // Saves the selected player mode in session and prepares a fresh game state.
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public IActionResult SelectMode(PlayerModeSelectionViewModel model)
        {
            if (!GameModes.IsValid(model.SelectedMode))
            {
                ModelState.AddModelError(nameof(model.SelectedMode), "Please choose Child or Adult mode.");
                return View(model);
            }

            var normalizedMode = GameModes.Normalize(model.SelectedMode);
            HttpContext.Session.SetString(GameSessionKeys.PlayerMode, normalizedMode);
            HttpContext.Session.SetObject(GameSessionKeys.GameState, new Models.GameSessionState
            {
                Score = 0,
                Lives = GameModes.GetInitialLives(normalizedMode)
            });
            HttpContext.Session.Remove(GameSessionKeys.ActiveRound);
            HttpContext.Session.Remove(GameSessionKeys.LastServedWord);

            return RedirectToAction(nameof(Index));
        }

        // Displays the rules.
        public IActionResult Privacy()
        {
            return View();
        }

        // Checks whether the current browser session already has a valid player mode.
        private bool HasPlayerMode()
        {
            return GameModes.IsValid(HttpContext.Session.GetString(GameSessionKeys.PlayerMode));
        }

        // show the player's highest score into a simple rank.
        private static string GetRank(int highestScore)
        {
            if (highestScore >= 50) return "Master";
            if (highestScore >= 30) return "Pro";
            if (highestScore >= 15) return "Intermediate";
            return "Beginner";
        }
    }
}
