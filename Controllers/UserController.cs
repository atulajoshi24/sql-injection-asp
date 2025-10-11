
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace SqliteSqlInjectionDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private const string ConnectionString = "Data Source=demo.db;";

        [HttpGet("search")]
        public IActionResult LoginVulnerable(string email)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            // WARNING: vulnerable to SQL injection
            string sql = "SELECT Id, Name, Email FROM Users WHERE Email = '" + email; 

            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2)
                    }
                });
            }

            return Unauthorized(new { success = false, message = "Invalid credentials" });
        }

.
        [HttpGet("safesearch")]
        public IActionResult LoginSafe(string email)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT Id, Name, Email FROM Users WHERE Email = @email";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
 
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2)
                    }
                });
            }

            return Unauthorized(new { success = false, message = "Invalid credentials" });
        }

    }
}
