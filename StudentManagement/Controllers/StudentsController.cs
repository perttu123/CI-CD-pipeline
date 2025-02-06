using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Data;

namespace StudentManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class StudentsController : ControllerBase
    {
        private readonly StudentContext _context;

        public StudentsController(StudentContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents2()
        {
            return await _context.Students.ToListAsync();

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents(){
            return await _context.Students.ToListAsync();

        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudentByIdRoute(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if(student != null)
            {
                return Ok(student);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return Ok(student);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if(id!=student.Id)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if(id == null){
                return NotFound();
            }
            else{
                var studentToDelete = await _context.Students.FindAsync(id);
                _context.Students.Remove(studentToDelete);
                await _context.SaveChangesAsync();
                return Ok(studentToDelete);
            }  
        }

        [HttpGet]
        public IActionResult GetTestMessage_CD_CI_TEST()
        {
            return Ok(new { message = "Hello from TestController!" });
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
