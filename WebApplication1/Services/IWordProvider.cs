namespace WordGame.Services
{
    public interface IWordProvider
    {
        Task<List<string>> GetWordsAsync(string category, int count);
    }
}
