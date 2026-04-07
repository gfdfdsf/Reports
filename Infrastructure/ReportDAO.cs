using System.Data;
using Microsoft.Data.SqlClient;
using Proyecto_Integrador_DevOps.Models;

namespace Proyecto_Integrador_DevOps.Infrastructure
{
    public class ReportDAO
    {
        private readonly string _connectionString;

        public ReportDAO(IConfiguration configuration) => _connectionString = configuration.GetConnectionString("Reports")!;

        public User GetUserByEmailAndPassword(string email, string password)
        {
            User user = new User();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"SELECT * FROM Users
                         WHERE UserEmail = @Email
                         AND Password = @Password";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                Email = reader["UserEmail"].ToString()!,
                                PasswordHash = reader["Password"].ToString()!,
                                CreatedDate = Convert.ToDateTime(reader["CreatedAt"]),
                                FullName = reader["FullName"].ToString()!,
                                Area = reader["Area"].ToString()!,
                                IsAdmin = Convert.ToBoolean(reader["IsAdmin"])
                            };
                        }
                    }
                }
            }

            return user;
        }

        public void InsertUser(User user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"INSERT INTO Users
                         (UserEmail, Password, CreatedAt, FullName, Area, IsAdmin)
                         VALUES
                         (@Email, @Password, GETDATE(), @FullName, @Area, @IsAdmin)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Area", user.Area);
                    cmd.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<QueryReport> GetAllReports()
        {
            var reports = new List<QueryReport>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM Reports";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reports.Add(MapReport(reader));
                    }
                }
            }

            return reports;
        }

        public List<QueryReport> GetReportsByArea(string area)
        {
            var reports = new List<QueryReport>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM Reports WHERE Area = @Area";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Area", area);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reports.Add(MapReport(reader));
                        }
                    }
                }
            }

            return reports;
        }

        public QueryReport? GetReportById(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = "SELECT * FROM Reports WHERE ReportId = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReport(reader);
                        }
                    }
                }
            }

            return null;
        }

        public async Task CreateAsync(QueryReport report)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = conn;

                cmd.CommandText = @"
                    INSERT INTO Reports
                    (
                        Name,
                        Area,
                        ConnectionString,
                        Query,
                        UserId,
                        CreatedAt
                    )
                    VALUES
                    (
                        @Name,
                        @Area,
                        @DatabaseName,
                        @Query,
                        @UserId,
                        GETDATE()
                    );";

                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = report.Name;
                cmd.Parameters.Add("@Area", SqlDbType.NVarChar, 100).Value = report.Area;
                cmd.Parameters.Add("@DatabaseName", SqlDbType.NVarChar, 100).Value = report.ConnectionString;
                cmd.Parameters.Add("@Query", SqlDbType.NVarChar).Value = report.Query;
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = report.UserId;

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public void UpdateReport(QueryReport report)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"UPDATE Reports
                         SET Name = @Name,
                             Query = @Query,
                             ConnectionString = @ConnectionString,
                             Area = @Area
                         WHERE ReportId = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", report.Name);
                    cmd.Parameters.AddWithValue("@Query", report.Query);
                    cmd.Parameters.AddWithValue("@ConnectionString", report.ConnectionString);
                    cmd.Parameters.AddWithValue("@Area", report.Area);
                    cmd.Parameters.AddWithValue("@Id", report.ReportId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteReport(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"DELETE FROM Reports WHERE ReportId = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private QueryReport MapReport(SqlDataReader reader)
        {
            return new QueryReport
            {
                ReportId = Convert.ToInt32(reader["ReportId"]),
                Name = reader["Name"].ToString() ?? string.Empty,
                UserId = Convert.ToInt32(reader["UserId"]),
                Query = reader["Query"].ToString()!,
                ConnectionString = reader["ConnectionString"].ToString()!,
                Area = reader["Area"].ToString()!,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        public (DataTable? Result, string? ErrorMessage) ExecuteReadOnlyQuery(string connectionString, string query)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                using var cmd = new SqlCommand(query, conn)
                {
                    CommandTimeout = 300
                };

                using var adapter = new SqlDataAdapter(cmd);
                var table = new DataTable();

                conn.Open();
                adapter.Fill(table);

                return (table, null);
            }
            catch (SqlException ex)
            {
                return (null, $"Error de SQL: {ex.Message}");
            }
            catch (Exception)
            {
                return (null, "Ocurrió un error inesperado al ejecutar la consulta.");
            }
        }
    }
}

