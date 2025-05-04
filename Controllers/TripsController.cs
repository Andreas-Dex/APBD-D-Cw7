using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using VetSqlClient.DTOS;

namespace VetSqlClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public TripsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetTrips()
        {
            var trips = new List<TripDto>();

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            using var command = new SqlCommand(@"
                SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, c.Name AS CountryName
                FROM Trip t
                LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                LEFT JOIN Country c ON ct.IdCountry = c.IdCountry
                ORDER BY t.DateFrom DESC;
            ", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                trips.Add(new TripDto
                {
                    IdTrip = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    CountryName = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return Ok(trips);
        }
    }
}