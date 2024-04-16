using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApiProject.Data;
using MyWebApiProject.DTOs;
using MyWebApiProject.Models;

namespace MyWebApiProject.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Specifies that only users with the "Admin" role can access this controller
    public class UserInfoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserInfoController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserInfo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserInfo>>> GetUserInfo()
        {
            // Retrieves all user information from the database
            return await _context.UserInfo.ToListAsync();
        }

        // GET: api/UserInfo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInfo>> GetUserInfo(int id)
        {
            // Retrieves user information with the specified ID from the database
            var userInfo = await _context.UserInfo.FindAsync(id);

            // If user information is not found, returns NotFound response
            if (userInfo == null)
            {
                return NotFound();
            }

            return userInfo;
        }

        // PUT: api/UserInfo/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserInfo(int id, UserInfo userInfo)
        {
            // Checks if the provided ID matches the ID of the user information object
            if (id != userInfo.UserId)
            {
                return BadRequest();
            }

            // Sets the state of the user information object to Modified
            _context.Entry(userInfo).State = EntityState.Modified;

            try
            {
                // Saves changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // If user information does not exist, returns NotFound response
                if (!UserInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Returns NoContent response after successful update
            return NoContent();
        }

        // POST: api/UserInfo
        [HttpPost]
        [AllowAnonymous] // Specifies that this action can be accessed without authentication
        public async Task<ActionResult<UserModel>> PostUserInfo(UserModel userModel)
        {
            // Checks if a user with the same email already exists in the database
            var existingUser = await _context.UserInfo.FirstOrDefaultAsync(u => u.Email == userModel.Email);

            if (existingUser != null)
            {
                // If a user with the same email exists, returns BadRequest response
                return BadRequest("User already exists");
            }

            // Hashes the user's password using BCrypt.Net
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userModel.Password);

            // Creates a new UserInfo object with the provided email and hashed password
            UserInfo userInfo = new UserInfo();
            userInfo.Email = userModel.Email;
            userInfo.Password = passwordHash;

            // Adds the new user information to the database
            _context.UserInfo.Add(userInfo);
            await _context.SaveChangesAsync();

            // Returns a CreatedAtAction response with the newly created user information
            return CreatedAtAction("GetUserInfo", new { id = userInfo.UserId }, userInfo);
        }

        // DELETE: api/UserInfo/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserInfo>> DeleteUserInfo(int id)
        {
            // Finds user information with the specified ID in the database
            var userInfo = await _context.UserInfo.FindAsync(id);
            if (userInfo == null)
            {
                // If user information is not found, returns NotFound response
                return NotFound();
            }

            // Removes the user information from the database
            _context.UserInfo.Remove(userInfo);
            await _context.SaveChangesAsync();

            // Returns the deleted user information
            return userInfo;
        }

        // Checks if user information with the specified ID exists in the database
        private bool UserInfoExists(int id)
        {
            return _context.UserInfo.Any(e => e.UserId == id);
        }
    }
}
