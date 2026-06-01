using ContactsAPI.Application.Auth;
using ContactsAPI.Application.Helper;
using ContactsAPI.Data;
using ContactsAPI.Entities;
using ContactsAPI.Models;
using ContactsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginRequest = ContactsAPI.Application.Auth.LoginRequest;

namespace ContactsAPI.API.Controllers.Auth
{
    /// <summary>
    /// Handles authentication: login (JWT generation), token refresh, and logout (token revocation).
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    [AllowAnonymous]
    public class AuthController(IConfiguration config, ContactsDbContext context, IAuthService authService) : ControllerBase
    {
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health() => Ok(new { status = "Healthy" });

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = authService.AuthenticateUser(request);
            if (user == null)
                return Unauthorized(new { error = "Invalid username or password." });

            if (Enum.TryParse<UserStatus>(user.Status, true, out var status))
            {
                if (status == UserStatus.Inactive)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        error = "Your account is inactive. Please contact support to reactivate."
                    });
                }

                if (status == UserStatus.Suspended)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        error = "Your account has been suspended. Contact an administrator for assistance."
                    });
                }
            }

            var claims = new List<Claim>
            {
                new("sub", user.UserId.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Role)
            };

            var jwtKey = config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is not configured."),
                audience: config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is not configured."),
                claims: claims,
                expires: PhilippineTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = TokenHelper.GenerateRefreshToken();
            context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                Expires = PhilippineTime.Now.AddDays(7),
                IsRevoked = false
            });
            context.SaveChanges();

            // Set JWT as HTTP-Only cookie (immune to XSS — JavaScript cannot read this)
            Response.Cookies.Append("access_token", jwtString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = PhilippineTime.Now.AddMinutes(30),
                Path = "/"
            });

            // Set Refresh Token as HTTP-Only cookie (only sent to auth endpoints)
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = PhilippineTime.Now.AddDays(7),
                Path = "/api/v1/auth"
            });

            return Ok(new
            {
                message = "Login successful",
                username = user.Username,
                role = user.Role,
                userId = user.UserId,
                // Also return tokens in body for backward compatibility (Swagger/Postman)
                token = jwtString,
                refreshToken
            });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest? request = null)
        {
            // Read refresh token from cookie first, fall back to request body (Swagger/Postman)
            var refreshTokenValue = Request.Cookies["refresh_token"] ?? request?.RefreshToken;
            if (string.IsNullOrEmpty(refreshTokenValue))
                return Unauthorized(new { error = "No refresh token provided." });

            var storedToken = context.RefreshTokens
                .FirstOrDefault(rt => rt.Token == refreshTokenValue && !rt.IsRevoked);

            if (storedToken == null || storedToken.Expires < PhilippineTime.Now)
                return Unauthorized();

            var user = context.Users.FirstOrDefault(u => u.UserId == storedToken.UserId);
            if (user == null) return Unauthorized();

            var newJwt = TokenHelper.GenerateJwt(user, config);

            var newRefreshToken = TokenHelper.GenerateRefreshToken();
            storedToken.IsRevoked = true;
            context.RefreshTokens.Add(new RefreshToken
            {
                UserId = storedToken.UserId,
                Token = newRefreshToken,
                Expires = PhilippineTime.Now.AddDays(7)
            });
            context.SaveChanges();

            // Set new JWT as HTTP-Only cookie
            Response.Cookies.Append("access_token", newJwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = PhilippineTime.Now.AddMinutes(30),
                Path = "/"
            });

            // Set new Refresh Token as HTTP-Only cookie
            Response.Cookies.Append("refresh_token", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = PhilippineTime.Now.AddDays(7),
                Path = "/api/v1/auth"
            });

            return Ok(new { token = newJwt, refreshToken = newRefreshToken });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshRequest? request = null)
        {
            // Read refresh token from cookie first, fall back to request body (Swagger/Postman)
            var refreshTokenValue = Request.Cookies["refresh_token"] ?? request?.RefreshToken;

            if (!string.IsNullOrEmpty(refreshTokenValue))
            {
                var storedToken = context.RefreshTokens
                    .FirstOrDefault(rt => rt.Token == refreshTokenValue);

                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    context.SaveChanges();
                }
            }

            // Always clear cookies on logout
            Response.Cookies.Delete("access_token", new CookieOptions { Path = "/" });
            Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/api/v1/auth" });

            return Ok(new { message = "Logged out successfully" });
        }
    }
}