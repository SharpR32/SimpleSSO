using HeimdallMini.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace HeimdallMini.Infrastructure.Services.Tokens
{
    public class TokenService : ITokenService
    {
        protected readonly TimeSpan _longevity;
        protected readonly IMemoryCache _cache;
        protected readonly LoginContext _dbContext;

        protected const char SEPARATOR = '-';

        public struct TokenData
        {
            internal TokenData(string username) => Username = username;
            public string Username { get; private set; }
        }

        public TokenService(IConfiguration config, IMemoryCache cache, LoginContext dbContext)
        {
            _longevity = config.GetSection("TokenLongevity").Get<TimeSpan>();
            _cache = cache;
            _dbContext = dbContext;
        }

        public string CreateLoginToken(string username, DateTime loginTime)
        {
            var token = CreateToken(username, loginTime);

            _cache.Set(token, username, _longevity);

            return string.Join(SEPARATOR, username, token);
        }

        protected static string CreateToken(string username, DateTime loginTime)
        {
            using var sha = SHA256.Create();
            var tokenData = new string[] { username, loginTime.ToString() };
            var hash = sha.ComputeHash(
                tokenData.Select(x => Encoding.UTF8.GetBytes(x))
                    .Aggregate(Enumerable.Empty<byte>(), (prev, current) => prev.Concat(current))
                    .ToArray());
            var token = Convert.ToBase64String(hash);

            return token;
        }

        public async Task<TokenData?> Validate(string token)
        {
            // No token
            if (string.IsNullOrEmpty(token)) return null;

            var tokenValues = token.Split(SEPARATOR);

            //Too short
            if (tokenValues.Length != 2)
                return null;

            var username = _cache.Get<string>(tokenValues[1]);

            //Not present in cache
            if (string.IsNullOrEmpty(username))
            {
                var loginDate = await _dbContext.Users
                    .Where(x => x.UserName == tokenValues[0])
                    .SelectMany(x => x.Logins)
                    .OrderByDescending(x => x.Created)
                    .Select(x => x.Created)
                    .FirstOrDefaultAsync();

                var challenger = CreateToken(tokenValues[0], loginDate);

                //Tokens are not the same
                if (!tokenValues[1].Equals(challenger, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                _cache.Set(challenger, tokenValues[0], _longevity);
            }

            return new TokenData(username);
        }
    }
}