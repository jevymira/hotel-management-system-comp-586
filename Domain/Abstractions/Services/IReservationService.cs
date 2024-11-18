using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Services;

public interface IReservationService
{
    public Task<Reservation> AddAsync(PostReservationDTO reservationDTO);
    public Task<Reservation> GetAsync(string id);
    public Task<List<Reservation>> GetByGuestNameAsync(string name);
    public Task<List<Reservation>> GetForDeskAsync();
    public Task UpdateCheckInOutAsync(string id, CheckInOutDTO dto);
}
