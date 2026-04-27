using DAL.Repository;
using Domain.Models.DTOs;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenBlacklistService blacklistService) 
        : IAuthService
    {
        public async Task<(bool Success, string Message, AuthResponce? responce)> LoginAsync(LoginRequset loginRequset)
        {
            var user = await userRepository.GetByUsernameAsync(loginRequset.UserName);
            if (user == null)
                return (false, "User not found", null);

            // ✅ BCrypt перевірка — відповідає вимозі "паролі зберігаються у вигляді хешів"
            if (!BCrypt.Net.BCrypt.Verify(loginRequset.Password, user.PasswordHash))
                return (false, "Invalid password", null);

            var responce = await GenerateAuthResponceAsync(user);
            return (true, "Login successful", responce);
        }

        public async Task<(bool Success, string Message)> LogoutAsync(string refreshToken, string accessToken)
        {
            var tokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null)
                return (false, "Invalid refresh token");


            if (tokenEntity.IsRevoked || !tokenEntity.IsActive)
                return (false, "Refresh token already revoked");

            blacklistService.Add(accessToken);

            await refreshTokenRepository.RevokeAllUserTokenAsync(tokenEntity.UserId);
            await refreshTokenRepository.SaveChangeAsync();
            return (true, "Logout successful");
        }

        public async Task<(bool Success, string Message, AuthResponce? responce)> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken);


            if (refreshTokenEntity == null)
                return (false, "Invalid refresh token", null);

            if (refreshTokenEntity.IsExpired || !refreshTokenEntity.IsActive)
                return (false, "Refresh token is expired or revoked", null);

            refreshTokenEntity.IsRevoked = true;
            await refreshTokenRepository.UpdateAsync(refreshTokenEntity);
            await refreshTokenRepository.SaveChangeAsync();

            var responce = await GenerateAuthResponceAsync(refreshTokenEntity.User);
            return (true, "Token refreshed successfully", responce);
        }

        public async Task<AuthResponce> GenerateAuthResponceAsync(User user)
        {
            var jwtToken = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };

            await refreshTokenRepository.AddAsync(refreshTokenEntity);
            await refreshTokenRepository.SaveChangeAsync();

            return new AuthResponce
            {
                UserName = user.Name,
                Email = user.Email,
                Role = user.Role,
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<(bool Success, string Message, AuthResponce? responce)> RegisterAsync(RegisterRequset register)
        {
            if (await userRepository.AnyAsync(x => x.Name == register.UserName))
                return (false, "User already exists", null);

            if (await userRepository.AnyAsync(x => x.Email == register.EmailAddress))
                return (false, "Email already exists", null);

            var user = new User
            {
                Name = register.UserName,
                Email = register.EmailAddress,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                Role = register.Role,
                CreatedAt = DateTime.UtcNow
            };

            await userRepository.AddAsync(user);
            await userRepository.SaveChangeAsync();

            var responce = await GenerateAuthResponceAsync(user);
            return (true, "User registered successfully", responce);
        }
    }
}
