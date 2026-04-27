using BLL.Services;
using Domain.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Identity_Provider.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthService authService,
        ITokenBlacklistService blacklistService,
        ILogger<AuthController> logger) : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequset loginRequest)
        {
            logger.LogInformation("Login attempt for user: {Username}", loginRequest.UserName);

            var res = await authService.LoginAsync(loginRequest);
            if (!res.Success)
            {
                logger.LogWarning("Login failed for user: {Username}. Reason: {Message}", loginRequest.UserName, res.Message);
                return Unauthorized(new { Message = res.Message });
            }
            return Ok(res.responce);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequset register)
        {
            logger.LogInformation("Register attempt for user: {Username}", register.UserName);

            var res = await authService.RegisterAsync(register);
            if (!res.Success)
            {
                logger.LogWarning("Registration failed for user: {Username}. Reason: {Message}", register.UserName, res.Message);
                return BadRequest(new { Message = res.Message });
            }
            return Ok(res.responce);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var res = await authService.RefreshTokenAsync(refreshTokenRequest.RefreshToken);
            if (!res.Success)
            {
                return Unauthorized(new { Message = res.Message });
            }
            return Ok(res.responce);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var authHeader = Request.Headers[HeaderNames.Authorization].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var jwtToken = authHeader.Substring("Bearer ".Length).Trim();
                blacklistService.Add(jwtToken);
            }

            var res = await authService.LogoutAsync(refreshTokenRequest.RefreshToken);
            if (!res.Success)
            {
                return BadRequest(new { Message = res.Message });
            }
            return Ok(new { Message = res.Message });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminEndpoint()
        {
            return Ok(new { Message = "Welcome, Admin! You have access to secured resources." });
        }

        [Authorize]
        [HttpGet("user-only")]
        public IActionResult UserEndpoint()
        {
            return Ok(new { Message = "Welcome! Any authenticated user can see this." });
        }
    }
}
