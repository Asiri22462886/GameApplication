namespace WordGame.Services
{
    public interface IImageProvider
    {
        Task<string> GetImageUrlAsync(string category, string word);
    }
}
