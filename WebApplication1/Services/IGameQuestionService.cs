using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IGameQuestionService
    {
        Task<GameQuestionDto> GenerateQuestionAsync(string category);
    }
}
