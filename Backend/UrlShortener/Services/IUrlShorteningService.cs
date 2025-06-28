namespace UrlShortener.API.Services
{
    public interface IUrlShorteningService
    {
        string GenerateShortCode(string originalUrl);
    }
}
