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
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Constructor that initializes the controller with an instance of the application's DbContext
        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        // Handles HTTP POST requests to create a new job
        [HttpPost]
        public async Task<ActionResult<Job>> PostJob(Job job)
        {
            // Adds the received job entity to the database context
            _context.Jobs.Add(job);
            // Persists the changes to the database
            await _context.SaveChangesAsync();

            // Returns a response with HTTP status code 201 (Created) and the newly created job entity
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }

        // Handles HTTP PUT requests to update an existing job
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJob(int id, Job job)
        {
            // Checks if the provided ID matches the ID of the job being updated
            if (id != job.Id)
            {
                // Returns a response with HTTP status code 400 (Bad Request) if the IDs do not match
                return BadRequest();
            }

            // Marks the provided job entity as modified in the database context
            _context.Entry(job).State = EntityState.Modified;

            try
            {
                // Persists the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Checks if the job being updated exists in the database
                if (!JobExists(id))
                {
                    // Returns a response with HTTP status code 404 (Not Found) if the job does not exist
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

        // Handles HTTP GET requests to retrieve all jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            // Retrieves all job entities from the database and returns them as a list
            return await _context.Jobs.ToListAsync();
        }

        // Handles HTTP GET requests to retrieve a specific job by its ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            // Retrieves the job entity with the specified ID from the database
            var job = await _context.Jobs.FindAsync(id);

            // Checks if the job entity exists
            if (job == null)
            {
                // Returns a response with HTTP status code 404 (Not Found) if the job does not exist
                return NotFound();
            }

            // Returns the retrieved job entity
            return job;
        }

        // Helper method to check if a job with the specified ID exists in the database
        private bool JobExists(int id)
        {
            // Returns true if a job with the specified ID exists in the database, otherwise returns false
            return _context.Jobs.Any(e => e.Id == id);
        }
    }
}
