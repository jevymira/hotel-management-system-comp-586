using Domain.Entities;
namespace Domain.Abstractions.Repositories;

/// <summary>
/// Encapsulates the logic for the retrieval/persistence of guest reservations.
/// </summary>
public interface IReservationRepository
{
    public Task SaveAsync(Reservation reservation);
    public Task<Reservation> LoadReservationAsync(string id);

    /// <summary>
    /// Retrieves all reservations with a full name matching the guest's.
    /// </summary>
    /// <param name="name">Guest full name.</param>
    public Task<List<Reservation>> QueryByNameAsync(string name);
    public Task<List<Reservation>> QueryDueInAsync();
    public Task<List<Reservation>> QueryCheckedInAsync();
    public Task<List<Reservation>> QueryCheckedOutAsync(string date);
    public Task<List<Reservation>> QueryConfirmedAsync(string date);
    public Task<List<Reservation>> QueryConfirmedForDateAsync(string date);

    /// <summary>
    /// Query the number of rooms of a given type that will be occupied between two dates,
    /// drawing on the quantity specified by each reservation of a type.
    /// </summary>
    /// <param name="roomType">Room type.</param>
    /// <param name="checkInDate">Date of guest check in.</param>
    /// <param name="checkOutDate">Date of guest check out.</param>
    /// <returns>Number of that will be occupied.</returns>
    public Task<int> QueryOverlapCountAsync(string roomType, string checkInDate, string checkOutDate);

    /// <summary>
    /// Overwrite the reservation and its corresponding rooms.
    /// </summary>
    /// <param name="reservation">Reservations to be overwritten.</param>
    /// <param name="rooms">Rooms to be overwritten.</param>
    public Task TransactWriteRoomReservationAsync(Reservation reservation, List<Room> rooms);

    /// <summary>
    /// Change the status of Confirmed reservatons to Due In, for 
    /// those with check in dates that match the current date.
    /// </summary>
    public Task TransactWriteDueInReservations(List<Reservation> reservations);
}
