using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Text;
using WebApplication_REST.Models;

namespace WebApplication_REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly string _connectionString;  

        public TransactionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
        }
        decimal totalBalance = 0m;

        [HttpGet]
        public ActionResult<IEnumerable<TransactionGet>> Get()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }

            try
            {
                List<TransactionGet> transactions = new List<TransactionGet>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT t.*, c.Name as CategoryName FROM Transactions t LEFT JOIN Categories c ON t.CategoryId = c.Id WHERE t.UserId = @UserId";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TransactionGet transaction = new TransactionGet
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                                    IsIncome = reader.GetBoolean(reader.GetOrdinal("IsIncome")),
                                    UserId = userId
                                };

                                transactions.Add(transaction);

                                totalBalance += transaction.IsIncome ? transaction.Amount : -transaction.Amount;
                            }
                        }
                    }
                }

                return Ok(new { Transactions = transactions, TotalBalance = totalBalance });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler aufgetreten: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
        }
        [HttpGet("{transactionId}")]
        public ActionResult<TransactionGet> GetTransactionById(int transactionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT t.*, c.Name as CategoryName FROM Transactions t LEFT JOIN Categories c ON t.CategoryId = c.Id WHERE t.UserId = @UserId AND t.Id = @TransactionId";
                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@TransactionId", transactionId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                TransactionGet transaction = new TransactionGet
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                                    IsIncome = reader.GetBoolean(reader.GetOrdinal("IsIncome")),
                                    UserId = userId
                                };

                                return Ok(transaction);
                            }
                            else
                            {
                                return NotFound("Transaktion nicht gefunden.");
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler aufgetreten: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
        }



        [HttpGet("transactions")]
        public ActionResult<IEnumerable<TransactionGet>> GetTransactions(int? categoryId = null, bool? isIncome = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }
            decimal totalBalance = 0m;

            try
            {
                List<TransactionGet> transactions = new List<TransactionGet>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT t.*, c.Name as CategoryName, c.IsIncome FROM Transactions t LEFT JOIN Categories c ON t.CategoryId = c.Id WHERE t.UserId = @UserId";

                    if (categoryId.HasValue)
                    {
                        sqlQuery += " AND t.CategoryId = @CategoryId";
                    }
                    if (isIncome.HasValue)
                    {
                        sqlQuery += " AND c.IsIncome = @IsIncome";
                    }

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        if (categoryId.HasValue)
                        {
                            command.Parameters.AddWithValue("@CategoryId", categoryId.Value);
                        }
                        if (isIncome.HasValue)
                        {
                            command.Parameters.AddWithValue("@IsIncome", isIncome.Value);
                        }

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TransactionGet transaction = new TransactionGet
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                                    IsIncome = reader.GetBoolean(reader.GetOrdinal("IsIncome")),
                                    UserId = userId
                                };

                                transactions.Add(transaction);

                                totalBalance += transaction.IsIncome ? transaction.Amount : -transaction.Amount;
                            }
                        }
                    }
                }

                return Ok(new { Transactions = transactions, TotalBalance = totalBalance });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler: " + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Interner Serverfehler: " + ex.Message);
            }
        }
        [HttpGet("balance")]
        public ActionResult<decimal> GetUserBalance()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }

            decimal balance = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT Amount, IsIncome FROM Transactions WHERE UserId = @UserId";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal amount = reader.GetDecimal(reader.GetOrdinal("Amount"));
                            bool isIncome = reader.GetBoolean(reader.GetOrdinal("IsIncome"));

                            if (isIncome)
                            {
                                balance += amount;
                            }
                            else
                            {
                                balance -= amount;
                            }
                        }
                    }
                }
            }

            return Ok(balance);
        }
        





        [HttpPost]
        public ActionResult<Transaction> Post(Transaction transaction)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
                {
                    return BadRequest("Benutzer nicht authentifiziert oder ungültige UserId im Token.");
                }
                if (transaction.Date == null || transaction.Date == DateTime.MinValue)
                {
                    transaction.Date = DateTime.Now;
                }
                transaction.UserId = currentUserId;

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Transactions (Date, Amount, Description, CategoryId, UserId, IsIncome) VALUES (@Date, @Amount, @Description, @CategoryId, @UserId, @IsIncome)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Date", transaction.Date);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@Description", transaction.Description);
                        command.Parameters.AddWithValue("@CategoryId", transaction.CategoryId);
                        command.Parameters.AddWithValue("@UserId", transaction.UserId);
                        command.Parameters.AddWithValue("@IsIncome", transaction.IsIncome);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Erstellen der Transaktion: " + ex.Message);
            }
        }


        [HttpPut]
        public ActionResult<TransactionGet> Put(TransactionGet updatedTransaction)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "UPDATE Transactions SET Date = @Date, Amount = @Amount, Description = @Description, CategoryId = @CategoryId, IsIncome = @IsIncome WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", updatedTransaction.Id);
                        command.Parameters.AddWithValue("@Date", updatedTransaction.Date);
                        command.Parameters.AddWithValue("@Amount", updatedTransaction.Amount);
                        command.Parameters.AddWithValue("@Description", updatedTransaction.Description);
                        command.Parameters.AddWithValue("@CategoryId", updatedTransaction.CategoryId);
                        command.Parameters.AddWithValue("@IsIncome", updatedTransaction.IsIncome);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Transaktion nicht gefunden");
                        }
                    }

                    connection.Close();
                }

                return Ok(updatedTransaction);
            }
            catch (Exception ex)
            {
                return BadRequest("Fehler beim Aktualisieren der Transaktion: " + ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return BadRequest("Benutzeridentifikation fehlgeschlagen. Stellen Sie sicher, dass Sie angemeldet sind.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "DELETE FROM Transactions WHERE Id = @Id AND UserId = @UserId";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@UserId", currentUserId);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Transaktion mit der angegebenen ID für den aktuellen Benutzer nicht gefunden.");
                        }
                    }
                }

                return NoContent();
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Datenbankfehler aufgetreten: " + ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Unbekannter Fehler beim Löschen der Transaktion: " + ex.Message);
            }
        }

    }
}
