using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class GameController : Controller
    {
        private readonly IWordApiService _wordApiService;
        private readonly IImageApiService _imageApiService;
        private readonly ApplicationDbContext _context;

        public GameController(IWordApiService wordApiService, IImageApiService ImageApiServise, ApplicationDbContext context)
        {
            _wordApiService = wordApiService;
            _imageApiService = ImageApiServise;
            _context = context;
        }

        public async Task<IActionResult> Play(int score = 0, int lives = 3, string message = "")
        {
            if (lives <= 0)
            {
                await SaveHighScoreAsync(score);
                ViewBag.FinalScore = score;
                return View("GameOver");
            }

            var word = await _wordApiService.GetSimpleWordAsync();

            if (string.IsNullOrWhiteSpace(word))
                word = "apple";

            var rnd = new Random();
            int missingIndex = rnd.Next(0, word.Length);

            string displayWord = word.Remove(missingIndex, 1).Insert(missingIndex, "_");
            string correctImage = await _imageApiService.GetImageForWordAsync(word);

            var options = new List<(string Image, string OptionType)>
    {
        (correctImage, "correct"),
        ("/images/dummy1.jpg", "wrong1"),
        ("/images/dummy2.jpg", "wrong2")
    };

            options = options.OrderBy(x => rnd.Next()).ToList();

            var model = new MissingLetterViewModelcs
            {
                FullWord = word,
                DisplayWord = displayWord,
                MissingIndex = missingIndex,

                CorrectImage = options[0].Image,
                WrongImage1 = options[1].Image,
                WrongImage2 = options[2].Image,

                CorrectOption = options[0].OptionType,
                WrongOption1 = options[1].OptionType,
                WrongOption2 = options[2].OptionType,

                Score = score,
                Lives = lives,
                Message = message
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CheckAnswer(
            string fullWord,
            string displayWord,
            string selectedOption,
            int score,
            int lives)
        {
            bool isCorrect = selectedOption == "correct";

            if (isCorrect)
                score++;
            else
                lives--;

            await SaveHistoryAsync(fullWord, displayWord, selectedOption, isCorrect, score, lives);

            if (lives <= 0)
            {
                await SaveHighScoreAsync(score);
                ViewBag.FinalScore = score;
                return View("GameOver");
            }

            string message = isCorrect ? "Correct answer!" : "Wrong answer! Life reduced.";

            return RedirectToAction("Play", new
            {
                score = score,
                lives = lives,
                message = message
            });
        }

        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var history = _context.GameHistories
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PlayedAt)
                .ToList();

            return View(history);
        }

        public IActionResult HighScore()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var highScore = _context.UserHighScores
                .FirstOrDefault(x => x.UserId == userId);

            return View(highScore);
        }

        private async Task SaveHighScoreAsync(int score)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return;

            var existing = _context.UserHighScores
                .FirstOrDefault(x => x.UserId == userId);

            if (existing == null)
            {
                var newHighScore = new UserHighScore
                {
                    UserId = userId,
                    HighestScore = score,
                    UpdatedAt = DateTime.Now
                };

                _context.UserHighScores.Add(newHighScore);
            }
            else if (score > existing.HighestScore)
            {
                existing.HighestScore = score;
                existing.UpdatedAt = DateTime.Now;
                _context.UserHighScores.Update(existing);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SaveHistoryAsync(
            string fullWord,
            string displayWord,
            string selectedOption,
            bool isCorrect,
            int score,
            int lives)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return;

            var history = new GameHistory
            {
                UserId = userId,
                Word = fullWord,
                DisplayWord = displayWord,
                SelectedOption = selectedOption,
                IsCorrect = isCorrect,
                ScoreAfterRound = score,
                LivesAfterRound = lives,
                PlayedAt = DateTime.Now
            };

            _context.GameHistories.Add(history);
            await _context.SaveChangesAsync();
        }
    }
}