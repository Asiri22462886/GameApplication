namespace WebApplication1.Services
{
    public interface IImageApiService
    {
        Task<string> GetImageForWordAsync(string word);
    }
}
