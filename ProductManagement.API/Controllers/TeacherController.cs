using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ProductManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        // In-memory data storage for teachers
        private static List<Teacher> _teachers = new List<Teacher>
        {
            new Teacher { Id = 1, Name = "Teacher 1", Subject = "Maths" },
            new Teacher { Id = 2, Name = "Teacher 2", Subject = "Science" },
            new Teacher { Id = 3, Name = "Teacher 3", Subject = "History" }
        };

        // GET: api/teacher
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_teachers);
        }

        // GET: api/teacher/{id}
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var teacher = _teachers.FirstOrDefault(t => t.Id == id);
            if (teacher == null)
                return NotFound();
            return Ok(teacher);
        }

        // POST: api/teacher
        [HttpPost]
        public IActionResult Post([FromBody] Teacher teacher)
        {
            if (teacher == null)
                return BadRequest();

            teacher.Id = _teachers.Any() ? _teachers.Max(t => t.Id) + 1 : 1;
            _teachers.Add(teacher);
            return CreatedAtAction(nameof(Get), new { id = teacher.Id }, teacher);
        }

        // PUT: api/teacher/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Teacher updatedTeacher)
        {
            var teacher = _teachers.FirstOrDefault(t => t.Id == id);
            if (teacher == null)
                return NotFound();

            teacher.Name = updatedTeacher.Name;
            teacher.Subject = updatedTeacher.Subject;
            return NoContent();
        }

        // DELETE: api/teacher/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var teacher = _teachers.FirstOrDefault(t => t.Id == id);
            if (teacher == null)
                return NotFound();

            _teachers.Remove(teacher);
            return NoContent();
        }

        // Simple Teacher model for in-memory storage
        public class Teacher
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Subject { get; set; }
        }
    }
}
