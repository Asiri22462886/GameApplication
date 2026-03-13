using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;

namespace WebApplication1.Controllers
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
            return View();
        }

        public IActionResult GameOver(int finalScore = 0)
        {
            ViewBag.FinalScore = finalScore;
            return View();
        }

        public IActionResult History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var history = _dbContext.GameHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PlayedAt)
                .ToList();
            if(history.Count == 0)
            {
                
            }
            return View(history);
        }

        [Authorize]
        public async Task<IActionResult> HighScores()
        {
            var scores = await (
                            from hs in _dbContext.UserHighScores
                            join u in _dbContext.Users on hs.UserId equals u.Id
                            orderby hs.HighestScore descending
                            select new WebApplication1.Models.LeaderBoardDto
                            {
                                UserName = u.UserName ?? "",
                                HighestScore = hs.HighestScore,
                                UpdatedAt = hs.UpdatedAt
                            }
                        ).ToListAsync();

            return View(scores);
        }
    }
}