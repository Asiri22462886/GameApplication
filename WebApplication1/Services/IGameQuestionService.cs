using WordGame.Models;

namespace WordGame.Services
{
    public interface IGameQuestionService
    {
        Task<GameQuestionDto> GenerateQuestionAsync(string category, string playerMode, string? previousWord = null);
    }
}
