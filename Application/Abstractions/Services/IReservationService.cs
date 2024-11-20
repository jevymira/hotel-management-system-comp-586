using Application.Models;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IReservationService
{
    public Task<Reservation> AddAsync(PostReservationDTO reservationDTO);
    public Task<Reservation> GetAsync(string id);
    public Task<List<Reservation>> GetByGuestNameAsync(string name);
    public Task<List<Reservation>> GetForDeskAsync();
    public Task UpdateStatusAndRoomsAsync(string id, CheckInOutDTO dto);
}
