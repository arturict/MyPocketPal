using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication_REST.Models;

namespace WebApplication_REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly string _connectionString; // Verbindungszeichenfolge zur Datenbank

        public TransactionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDbConnection");
        }

        [HttpGet]
        public IEnumerable<Transaction> Get()
        {
            List<Transaction> transactions = new List<Transaction>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM Transactions";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Transaction transaction = new Transaction
                            {
                                Id = Guid.Parse(reader["Id"].ToString()),
                                Date = DateTime.Parse(reader["Date"].ToString()),
                                Amount = decimal.Parse(reader["Amount"].ToString()),
                                Description = reader["Description"].ToString()
                            };

                            transactions.Add(transaction);
                        }
                    }
                }

                connection.Close();
            }

            return transactions;
        }

        [HttpPost]
        public ActionResult<Transaction> Post(Transaction transaction)
        {
            try
            {
                transaction.Id = Guid.NewGuid(); // Eindeutige ID für die Transaktion.

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "INSERT INTO Transactions (Id, Date, Amount, Description) VALUES (@Id, @Date, @Amount, @Description)";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", transaction.Id);
                        command.Parameters.AddWithValue("@Date", transaction.Date);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@Description", transaction.Description);

                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return Ok(transaction); // Rückgabe der erstellten Transaktion mit Statuscode 200 (OK).
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Erstellen der Transaktion: " + ex.Message); // Rückgabe eines Fehlerstatuscodes mit einer Fehlermeldung.
            }
        }

        [HttpPut]
        public ActionResult<Transaction> Put(Transaction transaction)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "UPDATE Transactions SET Date = @Date, Amount = @Amount, Description = @Description WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", transaction.Id);
                        command.Parameters.AddWithValue("@Date", transaction.Date);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@Description", transaction.Description);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Transaktion nicht gefunden");
                        }
                    }

                    connection.Close();
                }

                return Ok(transaction); // Rückgabe der aktualisierten Transaktion mit Statuscode 200 (OK).
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Aktualisieren der Transaktion: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sqlQuery = "DELETE FROM Transactions WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound("Transaktion nicht gefunden");
                        }
                    }

                    connection.Close();
                }

                return NoContent(); // Erfolgreiche Löschung mit Statuscode 204 (NoContent).
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Löschen der Transaktion: " + ex.Message);
            }
        }
    }
}
