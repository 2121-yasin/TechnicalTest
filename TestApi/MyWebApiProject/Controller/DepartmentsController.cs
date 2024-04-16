using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApiProject.Data;
using MyWebApiProject.Models;

namespace MyWebApiProject.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Constructor that initializes the controller with an instance of the application's DbContext
        public DepartmentsController(AppDbContext context)
        {
            _context = context;
        }

        // Handles HTTP POST requests to create a new department
        [HttpPost]
        public async Task<ActionResult<Department>> PostDepartment(Department department)
        {
            // Checks if the ModelState is valid
            if (!ModelState.IsValid)
            {
                // Returns a response with HTTP status code 400 (Bad Request) and the ModelState if it's not valid
                return BadRequest(ModelState);
            }

            // Adds the received department entity to the database context
            _context.Departments.Add(department);
            // Persists the changes to the database
            await _context.SaveChangesAsync();

            // Returns a response with HTTP status code 201 (Created) and the newly created department entity
            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, department);
        }

        // Handles HTTP PUT requests to update an existing department
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartment(int id, Department department)
        {
            // Checks if the provided ID matches the ID of the department being updated
            if (id != department.Id)
            {
                // Returns a response with HTTP status code 400 (Bad Request) if the IDs do not match
                return BadRequest();
            }

            // Marks the provided department entity as modified in the database context
            _context.Entry(department).State = EntityState.Modified;

            try
            {
                // Persists the changes to the database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Checks if the department being updated exists in the database
                if (!DepartmentExists(id))
                {
                    // Returns a response with HTTP status code 404 (Not Found) if the department does not exist
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

        // Handles HTTP GET requests to retrieve all departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
        {
            // Retrieves all department entities from the database and returns them as a list
            return await _context.Departments.ToListAsync();
        }

        // Handles HTTP GET requests to retrieve a specific department by its ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            // Retrieves the department entity with the specified ID from the database
            var department = await _context.Departments.FindAsync(id);

            // Checks if the department entity exists
            if (department == null)
            {
                // Returns a response with HTTP status code 404 (Not Found) if the department does not exist
                return NotFound();
            }

            // Returns the retrieved department entity
            return department;
        }

        // Helper method to check if a department with the specified ID exists in the database
        private bool DepartmentExists(int id)
        {
            // Returns true if a department with the specified ID exists in the database, otherwise returns false
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}
