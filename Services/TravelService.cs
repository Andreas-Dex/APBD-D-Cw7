using System.Data.SqlClient;
using VetSqlClient.DTOs;
using VetSqlClient.Models;

namespace VetSqlClient.Services;

public class TravelService : ITravelService
{
    private readonly IConfiguration _configuration;

    public TravelService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<TripWithCountries>> GetAllTripsAsync()
    {
        var trips = new List<TripWithCountries>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using var command = new SqlCommand(@"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, c.Name as CountryName
            FROM Trip t
            JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
            JOIN Country c ON ct.IdCountry = c.IdCountry
            ORDER BY t.DateFrom DESC
        ", connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int id = reader.GetInt32(0);
            var existingTrip = trips.FirstOrDefault(t => t.IdTrip == id);
            if (existingTrip == null)
            {
                existingTrip = new TripWithCountries
                {
                    IdTrip = id,
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    DateFrom = reader.GetDateTime(3),
                    DateTo = reader.GetDateTime(4),
                    Countries = new List<string>()
                };
                trips.Add(existingTrip);
            }
            existingTrip.Countries.Add(reader.GetString(5));
        }

        return trips;
    }

    public async Task<IEnumerable<ClientTrip>> GetClientTripsAsync(int idClient)
    {
        var trips = new List<ClientTrip>();
        using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using var command = new SqlCommand(@"
            SELECT t.Name, t.Description, t.DateFrom, t.DateTo
            FROM Trip t
            JOIN Client_Trip ct ON ct.IdTrip = t.IdTrip
            WHERE ct.IdClient = @IdClient
        ", connection);

        command.Parameters.AddWithValue("@IdClient", idClient);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            trips.Add(new ClientTrip
            {
                Name = reader.GetString(0),
                Description = reader.GetString(1),
                DateFrom = reader.GetDateTime(2),
                DateTo = reader.GetDateTime(3)
            });
        }

        return trips;
    }

    public async Task AddClientAsync(AddClientDto client)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using var command = new SqlCommand(@"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)
        ", connection);

        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task RegisterClientForTripAsync(int idClient, int idTrip)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();

        using var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", connection);
        checkCommand.Parameters.AddWithValue("@IdClient", idClient);
        checkCommand.Parameters.AddWithValue("@IdTrip", idTrip);

        var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;
        if (exists) return;

        using var insertCommand = new SqlCommand(@"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt, PaymentDate)
            VALUES (@IdClient, @IdTrip, @RegisteredAt, @PaymentDate)
        ", connection);

        insertCommand.Parameters.AddWithValue("@IdClient", idClient);
        insertCommand.Parameters.AddWithValue("@IdTrip", idTrip);
        insertCommand.Parameters.AddWithValue("@RegisteredAt", DateTime.UtcNow);
        insertCommand.Parameters.AddWithValue("@PaymentDate", DBNull.Value);

        await insertCommand.ExecuteNonQueryAsync();
    }

    public async Task UnregisterClientFromTripAsync(int idClient, int idTrip)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();

        using var command = new SqlCommand(@"
            DELETE FROM Client_Trip
            WHERE IdClient = @IdClient AND IdTrip = @IdTrip
        ", connection);

        command.Parameters.AddWithValue("@IdClient", idClient);
        command.Parameters.AddWithValue("@IdTrip", idTrip);

        await command.ExecuteNonQueryAsync();
    }
}
