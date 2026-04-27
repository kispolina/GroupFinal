using BLL.Services;

namespace Identity_Provider.Middleware
{
    public class BlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public BlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext ctx, ITokenBlacklistService blacklist)
        {
           
            var token = ctx.Request.Headers.Authorization
                .ToString().Replace("Bearer ", "");

            
            if (!string.IsNullOrEmpty(token) && blacklist.Contains(token))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsJsonAsync(new { Error = "Token has been revoked." });
                return;
            }

            await _next(ctx);
        }
    }
}