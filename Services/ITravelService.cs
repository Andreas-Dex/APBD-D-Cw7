using VetSqlClient.DTOs;
using VetSqlClient.Models;

namespace VetSqlClient.Services;

public interface ITravelService
{
    Task<IEnumerable<TripWithCountries>> GetAllTripsAsync();
    Task<IEnumerable<ClientTrip>> GetClientTripsAsync(int idClient);
    Task AddClientAsync(AddClientDto client);
    Task RegisterClientForTripAsync(int idClient, int idTrip);
    Task UnregisterClientFromTripAsync(int idClient, int idTrip);
}