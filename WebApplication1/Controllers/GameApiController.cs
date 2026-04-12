using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WordGame.Data;
using WordGame.Infrastructure;
using WordGame.Models;
using WordGame.Services;

namespace WordGame.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/game")]
    public class GameApiController : ControllerBase
    {
        private readonly IGameQuestionService _gameQuestionService;
        private readonly ApplicationDbContext _dbContext;

        public GameApiController(IGameQuestionService gameQuestionService, ApplicationDbContext dbContext)
        {
            _gameQuestionService = gameQuestionService;
            _dbContext = dbContext;
        }

        [HttpPost("start")]
        public IActionResult StartGame()
        {
            var playerMode = HttpContext.Session.GetString(GameSessionKeys.PlayerMode);
            if (!GameModes.IsValid(playerMode))
            {
                return BadRequest(new { message = "Select a player mode before starting the game." });
            }

            var normalizedMode = GameModes.Normalize(playerMode);
            var state = new GameSessionState
            {
                Score = 0,
                Lives = GameModes.GetInitialLives(normalizedMode)
            };

            HttpContext.Session.SetObject(GameSessionKeys.GameState, state);
            HttpContext.Session.Remove(GameSessionKeys.ActiveRound);
            HttpContext.Session.Remove(GameSessionKeys.LastServedWord);

            return Ok(new GameStartDto
            {
                Score = state.Score,
                Lives = state.Lives,
                PlayerMode = normalizedMode,
                PlayerModeDisplay = GameModes.DisplayName(normalizedMode),
                Categories = GameModes.GetAllowedCategories(normalizedMode).ToList()
            });
        }

        [HttpGet("question")]
        public async Task<IActionResult> GetQuestion(string category = "animals")
        {
            try
            {
                var playerMode = HttpContext.Session.GetString(GameSessionKeys.PlayerMode);
                if (!GameModes.IsValid(playerMode))
                {
                    return BadRequest(new { message = "Player mode is required." });
                }

                var normalizedMode = GameModes.Normalize(playerMode);
                var allowedCategories = GameModes.GetAllowedCategories(normalizedMode);
                category = string.IsNullOrWhiteSpace(category) ? allowedCategories[0] : category.Trim().ToLower();

                if (!allowedCategories.Contains(category, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Category is not available for the selected mode." });
                }

                var state = HttpContext.Session.GetObject<GameSessionState>(GameSessionKeys.GameState);
                if (state == null)
                {
                    state = new GameSessionState
                    {
                        Score = 0,
                        Lives = GameModes.GetInitialLives(normalizedMode)
                    };
                    HttpContext.Session.SetObject(GameSessionKeys.GameState, state);
                }

                if (state.Lives <= 0)
                {
                    return Conflict(new { message = "Game is over. Start a new session to continue." });
                }

                var lastServedWord = HttpContext.Session.GetString(GameSessionKeys.LastServedWord);
                var result = await _gameQuestionService.GenerateQuestionAsync(category, normalizedMode, lastServedWord);
                HttpContext.Session.SetObject(GameSessionKeys.ActiveRound, new ActiveRoundSession
                {
                    Category = result.Category,
                    OriginalWord = result.OriginalWord,
                    MaskedWord = result.MaskedWord,
                    MissingLetter = result.MissingLetter
                });
                HttpContext.Session.SetString(GameSessionKeys.LastServedWord, result.OriginalWord);

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
            {
                return Unauthorized();
            }

            var round = HttpContext.Session.GetObject<ActiveRoundSession>(GameSessionKeys.ActiveRound);
            if (round == null)
            {
                return Conflict(new { message = "No active round found. Load a question first." });
            }

            var playerMode = GameModes.Normalize(HttpContext.Session.GetString(GameSessionKeys.PlayerMode));
            var state = HttpContext.Session.GetObject<GameSessionState>(GameSessionKeys.GameState) ??
                        new GameSessionState
                        {
                            Score = 0,
                            Lives = GameModes.GetInitialLives(playerMode)
                        };

            var selectedLetter = request.SelectedLetter?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(selectedLetter) || selectedLetter.Length != 1)
            {
                return BadRequest(new { message = "Please choose a valid letter." });
            }

            bool isCorrect = string.Equals(
                selectedLetter,
                round.MissingLetter,
                StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                state.Score++;
            }
            else
            {
                state.Lives--;
            }

            _dbContext.GameHistories.Add(new GameHistory
            {
                UserId = userId,
                Category = round.Category,
                OriginalWord = round.OriginalWord,
                MaskedWord = round.MaskedWord,
                MissingLetter = round.MissingLetter,
                SelectedLetter = selectedLetter,
                IsCorrect = isCorrect,
                ScoreAfterRound = state.Score,
                LivesAfterRound = state.Lives,
                PlayedAt = DateTime.Now
            });

            var existingHighScore = await _dbContext.UserHighScores
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (existingHighScore == null)
            {
                _dbContext.UserHighScores.Add(new UserHighScore
                {
                    UserId = userId,
                    HighestScore = state.Score,
                    UpdatedAt = DateTime.Now
                });
            }
            else if (state.Score > existingHighScore.HighestScore)
            {
                existingHighScore.HighestScore = state.Score;
                existingHighScore.UpdatedAt = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();

            HttpContext.Session.SetObject(GameSessionKeys.GameState, state);
            HttpContext.Session.Remove(GameSessionKeys.ActiveRound);

            return Ok(new
            {
                isCorrect,
                score = state.Score,
                lives = state.Lives,
                gameOver = state.Lives <= 0
            });
        }
    }
}
