using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GPIOModels.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GPIOWebRunner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController
    {
        private readonly List<UserModel> _knownUsers;
        private readonly JWTConfig _jwtConfig;
        
        public AuthorizationController(
            IOptions<List<UserModel>> knownUsers,
            IOptions<JWTConfig> jwtConfig
        )
        {
            _knownUsers = knownUsers.Value;
            _jwtConfig = jwtConfig.Value;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authorize([FromBody] UserModel user)
        {
            IActionResult response = new UnauthorizedResult();

            UserModel authenticatedUser = AuthenticateUser(user);

            if (user != null)
            {
                string token = GenerateJWT()
            }

            return response;
        }

        private string GenerateJWT()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Key));
        }

        private UserModel AuthenticateUser(UserModel user)
        {
            UserModel authenticatedUser = _knownUsers
                .Where(u => u.Username == user.Username && u.Password == user.Password)?
                .Single();

            return authenticatedUser;
        }
    }
}