using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApiProject.Data;
using MyWebApiProject.Models;

namespace MyWebApiProject.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Constructor that initializes the controller with an instance of the application's DbContext
        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        // Handles HTTP POST requests to create a new location
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation(Location location)
        {
            // Checks if the ModelState is valid
            if (!ModelState.IsValid)
            {
                // Returns a response with HTTP status code 400 (Bad Request) and the ModelState if it's not valid
                return BadRequest(ModelState);
            }

            // Adds the received location entity to the database context
            _context.Locations.Add(location);
            // Persists the changes to the database
            await _context.SaveChangesAsync();

            // Returns a response with HTTP status code 201 (Created) and the newly created location entity
            return CreatedAtAction(nameof(GetLocationById), new { id = location.Id }, location);
        }

        // Handles HTTP PUT requests to update an existing location
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, Location location)
        {
            // Checks if the provided ID matches the ID of the location being updated
            if (id != location.Id)
            {
                // Returns a response with HTTP status code 400 (Bad Request) if the IDs do not match
                return BadRequest();
            }

            // Checks if the ModelState is valid
            if (!ModelState.IsValid)
            {
                // Returns a response with HTTP status code 400 (Bad Request) and the ModelState if it's not valid
                return BadRequest(ModelState);
            }

            // Marks the provided location entity as modified in the database context
            _context.Entry(location).State = EntityState.Modified;

            try
            {
                // Persists the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Checks if the location being updated exists in the database
                if (!LocationExists(id))
                {
                    // Returns a response with HTTP status code 404 (Not Found) if the location does not exist
                    return NotFound();
                }
                else
                {
                    // Throws the exception if there's a concurrency issue
                    throw;
                }
            }

            // Returns a response with HTTP status code 204 (No Content) if the update is successful
            return NoContent();
        }

        // Handles HTTP GET requests to retrieve a specific location by its ID
        [HttpGet("{id}", Name = "GetLocationById")]
        public async Task<ActionResult<Location>> GetLocationById(int id)
        {
            // Retrieves the location entity with the specified ID from the database
            var location = await _context.Locations.FindAsync(id);

            // Checks if the location entity exists
            if (location == null)
            {
                // Returns a response with HTTP status code 404 (Not Found) if the location does not exist
                return NotFound();
            }

            // Returns the retrieved location entity
            return location;
        }

        // Helper method to check if a location with the specified ID exists in the database
        private bool LocationExists(int id)
        {
            // Returns true if a location with the specified ID exists in the database, otherwise returns false
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
