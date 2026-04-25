using Domain.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, AuthResponce? responce)> RegisterAsync(RegisterRequset register);
        Task<(bool Success, string Message, AuthResponce? responce)> LoginAsync(LoginRequset loginRequset);
        Task<(bool Success, string Message, AuthResponce? responce)> RefreshTokenAsync(string refreshToken);
        Task<(bool Success, string Message)> LogoutAsync(string refreshToken);
    }
}
