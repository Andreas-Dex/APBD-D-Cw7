using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using VetSqlClient.DTOs;

namespace VetSqlClient.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ClientsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult AddClient(AddClientDto dto)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var command = new SqlCommand("INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)", connection);
        command.Parameters.AddWithValue("@FirstName", dto.FirstName);
        command.Parameters.AddWithValue("@LastName", dto.LastName);
        command.Parameters.AddWithValue("@Email", dto.Email);
        command.Parameters.AddWithValue("@Telephone", dto.Telephone);
        command.Parameters.AddWithValue("@Pesel", dto.Pesel);
        connection.Open();
        command.ExecuteNonQuery();
        return Ok();
    }
}