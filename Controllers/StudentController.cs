using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentManagementAPI.Models;
using System.Data;

namespace StudentManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        // SQL Server connection string
        private readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=StudentDB;Integrated Security=True;TrustServerCertificate=True;";

        // 1. GET: Get all students
        [HttpGet]
        public IActionResult GetAllStudents()
        {
            List<StudentModel> students = new List<StudentModel>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Sid, Sname, Course, Fees, Mobile FROM Student";

                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        StudentModel student = new StudentModel
                        {
                            Sid = Convert.ToInt32(reader["Sid"]),
                            Sname = reader["Sname"].ToString()!,
                            Course = reader["Course"].ToString()!,
                            Fees = Convert.ToDecimal(reader["Fees"]),
                            Mobile = reader["Mobile"].ToString()!
                        };

                        students.Add(student);
                    }

                    reader.Close();
                }

                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 2. GET: Get student by ID
        [HttpGet("{id}")]
        public IActionResult GetStudentById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Enter a valid Student ID.");
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Sid, Sname, Course, Fees, Mobile FROM Student WHERE Sid = @Sid";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Sid", id);

                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        StudentModel student = new StudentModel
                        {
                            Sid = Convert.ToInt32(reader["Sid"]),
                            Sname = reader["Sname"].ToString()!,
                            Course = reader["Course"].ToString()!,
                            Fees = Convert.ToDecimal(reader["Fees"]),
                            Mobile = reader["Mobile"].ToString()!
                        };

                        reader.Close();

                        return Ok(student);
                    }

                    reader.Close();

                    return NotFound("Student not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 3. POST: Add a new student
        [HttpPost]
        public IActionResult AddStudent(int Sid,string Sname,string Course,decimal Fees,string Mobile)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO Student
                                     (Sid, Sname, Course, Fees, Mobile)
                                     VALUES
                                     (@Sid, @Sname, @Course, @Fees, @Mobile)";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Sid", Sid);
                    cmd.Parameters.AddWithValue("@Sname", Sname);
                    cmd.Parameters.AddWithValue("@Course", Course);
                    cmd.Parameters.AddWithValue("@Fees", Fees);
                    cmd.Parameters.AddWithValue("@Mobile", Mobile);

                    con.Open();

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return Ok("Student added successfully.");
                    }

                    return BadRequest("Student could not be added.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 4. PUT: Update an existing student
        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id, string Sname, string Course, decimal Fees, string Mobile)
        {
            if (id <= 0)
            {
                return BadRequest("Enter a valid Student ID.");
            }
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE Student
                                     SET Sname = @Sname,
                                         Course = @Course,
                                         Fees = @Fees,
                                         Mobile = @Mobile
                                     WHERE Sid = @Sid";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Sid", id);
                    cmd.Parameters.AddWithValue("@Sname", Sname);
                    cmd.Parameters.AddWithValue("@Course", Course);
                    cmd.Parameters.AddWithValue("@Fees", Fees);
                    cmd.Parameters.AddWithValue("@Mobile", Mobile);

                    con.Open();

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return Ok("Student updated successfully.");
                    }

                    return NotFound("Student not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 5. PATCH: Update student details
        // This currently accepts the complete student model, like PUT.
        [HttpPatch("{id}")]
        public IActionResult PatchStudent(int id, string? Sname, string? Course, decimal? Fees, string? Mobile)
        {
            if (id <= 0)
                return BadRequest("Invalid Student ID.");

            List<string> updates = new List<string>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    cmd.Parameters.AddWithValue("@Sid", id);

                    if (Sname != null)
                    {
                        if (Sname.Trim().Length < 3 ||Sname.Trim().Length > 50)
                            return BadRequest("Invalid student name.");
                        updates.Add("Sname = @Sname");
                        cmd.Parameters.AddWithValue("@Sname", Sname.Trim());
                    }

                    if (Course != null)
                    {
                        if (string.IsNullOrWhiteSpace(Course) || Course.Length > 30)
                            return BadRequest("Invalid course.");

                        updates.Add("Course = @Course");
                        cmd.Parameters.AddWithValue("@Course", Course);
                    }

                    if (Fees <= 0 || Fees > 1000000)
                    {
                        return BadRequest("Invalid fees.");
                        updates.Add("Fees = @Fees");
                        cmd.Parameters.AddWithValue("@Fees", Fees);
                    }

                    if (Mobile != null)
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(
                            Mobile, @"^[0-9]{10}$"))
                            return BadRequest("Invalid mobile number.");

                        updates.Add("Mobile = @Mobile");
                        cmd.Parameters.AddWithValue("@Mobile", Mobile);
                    }

                    if (updates.Count == 0)
                        return BadRequest("Provide at least one field to update.");

                    cmd.CommandText = "UPDATE Student SET " +
                                      string.Join(", ", updates) +
                                      " WHERE Sid = @Sid";

                    con.Open();

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                        return Ok("Selected fields updated successfully.");

                    return NotFound("Student not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 6. DELETE: Delete a student
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Enter a valid Student ID.");
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "DELETE FROM Student WHERE Sid = @Sid";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Sid", id);

                    con.Open();

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return Ok("Student deleted successfully.");
                    }

                    return NotFound("Student not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}