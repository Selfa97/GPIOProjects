using GPIOModels.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace GPIOWebRunner.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        private readonly List<UserModel> _knownUsers;
        private readonly JWTConfig _jwtConfig;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(
            IOptions<List<UserModel>> knownUsers,
            IOptions<JWTConfig> jwtConfig,
            ILogger<AuthorizationController> logger)
        {
            _knownUsers = knownUsers.Value;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("authorize")]
        public IActionResult Authorize([FromBody] UserModel user)
        {
            IActionResult response = new UnauthorizedResult();

            UserModel authenticatedUser = AuthenticateUser(user);

            if (authenticatedUser != null)
            {
                _logger.LogInformation($"Authenticated user: {authenticatedUser.Username}");

                string tokenBody = GenerateJWT(authenticatedUser);
                response = new OkObjectResult(new { token = tokenBody });
            }
            else
            {
                _logger.LogInformation($"Could not authenticate user: {user.Username}");
            }

            return response;
        }

        private string GenerateJWT(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new Claim[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username)
            };

            var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtConfig.ExpiryInMinutes),
            signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel AuthenticateUser(UserModel user)
        {
            UserModel authenticatedUser = _knownUsers
            .Where(u => u.Username == user.Username && u.Password == user.Password)
            .SingleOrDefault();

            return authenticatedUser;
        }
    }
}