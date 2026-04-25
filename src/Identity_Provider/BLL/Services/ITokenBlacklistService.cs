using System;

namespace BLL.Services
{
    public interface ITokenBlacklistService
    {
        void Add(string token);
        bool Contains(string token);
    }
}
