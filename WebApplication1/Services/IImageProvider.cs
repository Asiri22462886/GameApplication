namespace WebApplication1.Services
{
    public interface IImageProvider
    {
        Task<string> GetImageUrlAsync(string category, string word);
    }
}
