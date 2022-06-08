
namespace HeimdallMini.Infrastructure.Services.Tokens
{
    public interface ITokenService
    {
        string CreateLoginToken(string username, DateTime loginTime);
        Task<TokenService.TokenData?> Validate(string token);
    }
}