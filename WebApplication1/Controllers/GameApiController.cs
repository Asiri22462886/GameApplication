using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameApiController : ControllerBase
    {
        private readonly IGameQuestionService _gameQuestionService;
        private readonly ApplicationDbContext _dbcontext;

        public GameApiController(IGameQuestionService gameQuestionService,  ApplicationDbContext dbContext)
        {
            _gameQuestionService = gameQuestionService;
            _dbcontext = dbContext;
        }

        [HttpGet("question")]
        public async Task<IActionResult> GetQuestion(string category = "animals")
        {
            try
            {
                var result = await _gameQuestionService.GenerateQuestionAsync(category);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Could not generate question.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] AnswerRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            bool isCorrect = string.Equals(
                request.SelectedLetter,
                request.MissingLetter,
                StringComparison.OrdinalIgnoreCase);

            int score = request.Score;
            int lives = request.Lives;

            if (isCorrect)
                score++;
            else
                lives--;

            _dbcontext.GameHistories.Add(new GameHistory
            {
                UserId = userId,
                Category = request.Category,
                OriginalWord = request.OriginalWord,
                MaskedWord = request.MaskedWord,
                MissingLetter = request.MissingLetter,
                SelectedLetter = request.SelectedLetter,
                IsCorrect = isCorrect,
                ScoreAfterRound = score,
                LivesAfterRound = lives,
                PlayedAt = DateTime.Now
            });

            var existingHighScore = _dbcontext.UserHighScores
                .FirstOrDefault(x => x.UserId == userId);

            if (existingHighScore == null)
            {
                _dbcontext.UserHighScores.Add(new UserHighScore
                {
                    UserId = userId,
                    HighestScore = score,
                    UpdatedAt = DateTime.Now
                });
            }
            else if (score > existingHighScore.HighestScore)
            {
                existingHighScore.HighestScore = score;
                existingHighScore.UpdatedAt = DateTime.Now;
            }

            await _dbcontext.SaveChangesAsync();

            return Ok(new
            {
                isCorrect,
                score,
                lives,
                gameOver = lives <= 0
            });
        }
    }
}