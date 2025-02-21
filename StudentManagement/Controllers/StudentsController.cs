using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Data;
using Microsoft.AspNetCore.Authorization;

//KOMMENTTI
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

        [Authorize]
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

        [Authorize(Policy = "RequireAdminRole")]
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
        
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserCredentials credentials)
        {
            // Tarkista tässä credentials-olion arvot, esimerkiksi tietokantahakujen kautta
            if (credentials.Username == "testuser" && credentials.Password == "testpassword")
            {
                // Jos tunnistetiedot ovat oikein, generoi JWT-token ja palauta se
                var tokenService = new TokenService(); // Oletetaan, että TokenService on luokka, joka generoi tokenin
                var token = tokenService.GenerateToken(credentials.Username, false);
                return Ok(new { Token = token });
            }
            else if(credentials.Username == "admin" && credentials.Password == "adminpassword")
            {
                var tokenService = new TokenService();
                var token = tokenService.GenerateToken(credentials.Username, true);
                return Ok(new {Token = token});
            }
            else
            {
                // Jos tunnistetiedot ovat väärin, palauta virheilmoitus
                return Unauthorized("Käyttäjätunnus tai salasana on väärin.");
            }
        }

        [HttpGet("get")]
        public IActionResult GetMockData()
        {
            var mockData = new 
            {
                id = 1,
                name = "Sample Item",
                description = "This is a mock item for API testing",
                price = 19.99
            };

            return Ok(mockData);
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
