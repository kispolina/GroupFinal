using Microsoft.Extensions.Caching.Memory;
using System;

namespace BLL.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;

        public TokenBlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Add(string token)
        {
           
            _cache.Set(token, true, TimeSpan.FromHours(24));
        }

        public bool Contains(string token)
        {
           
            return _cache.TryGetValue(token, out _);
        }
    }
}
