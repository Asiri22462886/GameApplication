using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WordGame.Data;
using WordGame.Infrastructure;
using WordGame.Models;

namespace WordGame.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    public class GameController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public GameController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            if (!HasPlayerMode())
            {
                return RedirectToAction("SelectMode", "Home");
            }

            var playerMode = GameModes.Normalize(HttpContext.Session.GetString(GameSessionKeys.PlayerMode));

            HttpContext.Session.SetObject(GameSessionKeys.GameState, new Models.GameSessionState
            {
                Score = 0,
                Lives = GameModes.GetInitialLives(playerMode)
            });
            HttpContext.Session.Remove(GameSessionKeys.ActiveRound);
            HttpContext.Session.Remove(GameSessionKeys.LastServedWord);

            var model = new GamePageViewModel
            {
                PlayerMode = playerMode,
                PlayerModeDisplay = GameModes.DisplayName(playerMode),
                InitialLives = GameModes.GetInitialLives(playerMode),
                Categories = GameModes.GetAllowedCategories(playerMode).ToList()
            };

            return View(model);
        }

        public IActionResult GameOver(int finalScore = 0)
        {
            ViewBag.FinalScore = finalScore;
            return View();
        }

        public IActionResult History()
        {
            if (!HasPlayerMode())
            {
                return RedirectToAction("SelectMode", "Home");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var history = _dbContext.GameHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PlayedAt)
                .ToList();

            return View(history);
        }

        public async Task<IActionResult> HighScores()
        {
            if (!HasPlayerMode())
            {
                return RedirectToAction("SelectMode", "Home");
            }

            var scores = await (
                from hs in _dbContext.UserHighScores
                join u in _dbContext.Users on hs.UserId equals u.Id
                orderby hs.HighestScore descending
                select new Models.LeaderBoardDto
                {
                    UserName = u.UserName ?? "",
                    HighestScore = hs.HighestScore,
                    UpdatedAt = hs.UpdatedAt
                }
            ).ToListAsync();

            return View(scores);
        }

        private bool HasPlayerMode()
        {
            return GameModes.IsValid(HttpContext.Session.GetString(GameSessionKeys.PlayerMode));
        }
    }
}
