using Domain.Entities;
namespace Domain.Abstractions.Repositories;

public interface IReservationRepository
{
    public Task SaveAsync(Reservation reservation);
    public Task<Reservation> LoadReservationAsync(string id);
    public Task<List<Reservation>> QueryByNameAsync(string name);
    public Task<List<Reservation>> QueryDueInAsync();
    public Task<List<Reservation>> QueryCheckedInAsync();
    public Task<List<Reservation>> QueryCheckedOutAsync(string date);
    public Task<List<Reservation>> QueryConfirmedAsync(string date);
    public Task<List<Reservation>> QueryConfirmedTodayAsync(string date);
    public Task<int> QueryOverlapCountAsync(Reservation reservation, string bookingStatus);
    public Task TransactWriteRoomReservationAsync(Reservation reservation, List<Room> rooms);
    public Task TransactWriteDueInReservations(List<Reservation> reservations);
}
