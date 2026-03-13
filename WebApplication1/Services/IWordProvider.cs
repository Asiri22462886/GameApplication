namespace WebApplication1.Services
{
    public interface IWordProvider
    {
        Task<List<string>> GetWordsAsync(string category, int count);
    }
}
