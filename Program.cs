using Microsoft.Data.Sqlite;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.UseHttpsRedirection();
DbSeeder.EnsureSeed("demo.db");
app.Run();


static class DbSeeder
{
    public static void EnsureSeed(string dbPath)
    {
        Console.WriteLine("Seedf Ensured");
        var cs = $"Data Source={dbPath};";

        using var conn = new SqliteConnection(cs);
        conn.Open();

        using var transaction = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;

         // create table if not exists
        cmd.CommandText = @"DROP TABLE IF EXISTS Users";
        cmd.ExecuteNonQuery();

        // create table if not exists
        cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE
                );
        ";

        cmd.ExecuteNonQuery();
        transaction.Commit();

            // Users to insert: (Name, Email, PlainPassword)
        var users = new List<(string Name, string Email)>
        {
            ("Alice", "alice@example.com"),
            ("Bob",   "bob@example.com"),
            ("Carol", "carol@example.com"),
            ("Eve",   "eve@example.com"),
            ("Admin", "admin@example.com")
        };

        using var tx = conn.BeginTransaction();
        using var cmdQuery = conn.CreateCommand();
        cmdQuery.CommandText = @"
INSERT INTO Users (Name, Email)
VALUES (@name, @email);
";
        // prepare parameter placeholders once
        var pName = cmdQuery.CreateParameter();
        pName.ParameterName = "@name";
        cmdQuery.Parameters.Add(pName);

        var pEmail = cmd.CreateParameter();
        pEmail.ParameterName = "@email";
        cmdQuery.Parameters.Add(pEmail);


        foreach (var u in users)
        {
            pName.Value = u.Name;
            pEmail.Value = u.Email;
            try
            {
                cmdQuery.ExecuteNonQuery();
                Console.WriteLine($"Inserted user: {u.Email}");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // constraint violation (unique)
            {
                Console.WriteLine($"Skipped (already exists): {u.Email}");
            }
        }

        tx.Commit();
        Console.WriteLine("Seeding complete.");
    
    }
}

