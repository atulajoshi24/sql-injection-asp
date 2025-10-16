
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Data;

//http://localhost:5013/api/user/safesearch?email=alice@example.com%27%20OR%201=1%20--
//http://localhost:5013/api/user/safesearch?email=alice@example.com

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
            Console.WriteLine($"Login attempt for email: {email}");
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            // WARNING: vulnerable to SQL injection
            string sql = "SELECT Id, Name, Email FROM Users WHERE Email = '" + email + "'"; 

            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine($"cmd: {cmd.CommandText}");

            var users = new List<object>();

            while (reader.Read())
            {
                Console.WriteLine($"Reading The Email from DB: {email}");
                users.Add(new
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2)
                });
                          
            }
            return Ok(new { success = true, users });
        }


        [HttpGet("safesearch")]
        public IActionResult LoginSafe(string email)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT Id, Name, Email FROM Users WHERE Email = @email";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
 
            using var reader = cmd.ExecuteReader();
            var users = new List<object>();
            while (reader.Read())
            {
                Console.WriteLine($"Reading The Email from DB: {email}");
                users.Add(new
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2)
                });
                          
            }
            return Ok(new { success = true, users });

        }

    }
}
