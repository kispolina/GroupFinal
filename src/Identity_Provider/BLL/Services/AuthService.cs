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
    public class AuthService(IUserRepository userRepository, IJwtService service, IRefreshTokenRepository refreshTokenRepository) : IAuthService
    {

        public async Task<(bool Success, string Message, AuthResponce? responce)> LoginAsync(LoginRequset loginRequset)
        {
            var user = await userRepository.GetByUsernameAsync(loginRequset.UserName);
            if (user == null)
            {
                return (false, "User not found", null);
            }

            if (!BCrypt.Net.BCrypt.Verify(loginRequset.Password, user.PasswordHash))
            {
                return (false, "Invalid password", null);
            }

            var responce = await GenerateAuthResponceAsync(user);

            return (true, "Login successful", responce);

        }

        public async Task<(bool Success, string Message)> LogoutAsync(string refreshToken)
        {
            var tokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null)
            {
                return (false, "invalid refresh token");
            }
            if(tokenEntity.isRevoked && !tokenEntity.IsActive)
            {
                return (false, "Refresh token already revoked");
            }
            await refreshTokenRepository.RevokeAllUserTokenAsync(tokenEntity.UserId);
            await refreshTokenRepository.SaveChangeAsync();
            return (true, "Logout successful");
        }

        public async Task<(bool Success, string Message, AuthResponce? responce)> RefreshTokenAsync(string refreshToken)
        {
            var refreshTokenEntity = await refreshTokenRepository.GetByTokenAsync(refreshToken);
            if(refreshTokenEntity == null && refreshTokenEntity.IsExpired && !refreshTokenEntity.IsActive)
            {
                return (false, "Invalid refresh token", null);
            }
            refreshTokenEntity.isRevoked = true;
            await refreshTokenRepository.UpdateAsync(refreshTokenEntity);
            await refreshTokenRepository.SaveChangeAsync();

            var responce = await GenerateAuthResponceAsync(refreshTokenEntity.User);
            return(true, "Token refreshed successfully",  responce);
        }
        public async Task<AuthResponce> GenerateAuthResponceAsync(User user)
        {
            var jwttoken = service.GenerateToken(user);
            var refreshToken = service.GenerateRefreshToken();

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
                AccessToken = jwttoken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }
        public async Task<(bool Success, string Message, AuthResponce? responce)> RegisterAsync(RegisterRequset register)
        {
            if (await userRepository.AnyAsync(x => x.Name == register.UserName))
            {
                return (false, "User already exists", null);
            }

            if (await userRepository.AnyAsync(x => x.Email == register.EmailAddress))
            {
                return (false, "Email already exists", null);
            }

            var user = new User
            {
                Name = register.UserName,
                Email = register.EmailAddress,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                Role = "User",
            };

            await userRepository.AddAsync(user);
            await userRepository.SaveChangeAsync();
            var responce = await GenerateAuthResponceAsync(user);
            return (true, "User registered successfully", responce);
        }

    }
}
