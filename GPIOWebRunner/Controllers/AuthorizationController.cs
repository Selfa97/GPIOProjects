using GPIOModels.Authorization;
using GPIOModels.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly List<string> _knownUsers;
        private readonly JWTConfig _jwtConfig;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(
            IOptions<List<string>> knownUsers,
            IOptions<JWTConfig> jwtConfig,
            IConfiguration config,
            ILogger<AuthorizationController> logger)
        {
            _knownUsers = knownUsers.Value;
            _jwtConfig = jwtConfig.Value;
            _config = config;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("authorize")]
        public IActionResult Authorize([FromBody] UserModel user)
        {
            IActionResult response = Unauthorized();

            string authenticatedUsername = AuthenticateUser(user);

            if (authenticatedUsername != null)
            {
                _logger.LogInformation($"Authenticated user: {authenticatedUsername}");

                string tokenBody = GenerateJWT(authenticatedUsername);
                response = Ok(new { token = tokenBody });
            }
            else
            {
                _logger.LogInformation($"Could not authenticate user: {user.Username}");
            }

            return response;
        }

        private string GenerateJWT(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWTSigningSecret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username)
            };

            var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtConfig.ExpiryInMinutes),
            signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string AuthenticateUser(UserModel user)
        {
            string authenticatedUsername = _knownUsers
            .Where(known => user.Username == known && user.Password == _config[$"{known}:Password"])
            .SingleOrDefault();

            return authenticatedUsername;
        }
    }
}