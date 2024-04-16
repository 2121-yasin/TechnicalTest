using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyWebApiProject.Data;
using MyWebApiProject.DTOs;
using MyWebApiProject.Models;

namespace MyWebApiProject.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        public readonly AppDbContext _context;

        public TokenController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post(UserModel userModel)
        {
            // Check if UserModel is not null and contains necessary data
            if (userModel != null && userModel.Email != null && userModel.Password != null)
            {
                // Retrieve user from the database based on the provided email
                var user = await GetUser(userModel.Email);

                // Check if user exists
                if (user != null)
                {
                    // Verify if the provided password matches the hashed password stored in the database
                    if (!BCrypt.Net.BCrypt.Verify(userModel.Password, user.Password))
                    {
                        return BadRequest("Wrong password.");
                    }

                    // Define the claims to be included in the JWT token
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("id", user.UserId.ToString()),
                        new Claim("Email", user.Email),
                        // Note: Avoid including sensitive information like passwords in JWT claims
                        // new Claim("Password", user.Password), // Uncomment this line to include password in claims (not recommended)
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    // Generate symmetric security key using the configured JWT key
                    var key = new SymmetricSecurityKey(Encoding.UTF32.GetBytes(_configuration["Jwt:Key"]));
                    // Create signing credentials using the security key and HMACSHA256 algorithm
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    // Generate JWT token with specified issuer, audience, claims, expiration, and signing credentials
                    var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: DateTime.Now.AddYears(5),
                        signingCredentials: signIn);

                    // Return JWT token as response
                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    // User with the provided email does not exist
                    return BadRequest("Invalid credentials");
                }
            }
            else
            {
                // UserModel, email, or password is null
                return BadRequest();
            }
        }

        // Retrieve user information from the database based on email
        private async Task<UserInfo> GetUser(string Email)
        {
            // Query the database for the user with the provided email
            return await _context.UserInfo.FirstOrDefaultAsync(u => u.Email == Email);
        }
    }
}
