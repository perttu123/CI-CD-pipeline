

using StudentManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Data
{
    public class StudentContext : DbContext
    {
        public StudentContext(DbContextOptions<StudentContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; } = null!;
    }
}
