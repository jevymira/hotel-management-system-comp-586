using Application.Models;
using Domain.Entities;

namespace Application.Abstractions.Services;

/// <summary>
/// High-level interface for operations relating to reservations.
/// </summary>
public interface IReservationService
{
    public Task<Reservation> AddAsync(PostReservationDTO reservationDTO);
    public Task<ReservationDTO> GetAsync(string id);

    /// <summary>
    /// Retrieves all reservations with a full name matching the guest's.
    /// </summary>
    /// <param name="name">Guest full name.</param>
    public Task<List<ReservationDTO>> GetByGuestNameAsync(string name);

    /// <summary>
    /// Returns reservations to be displayed by default at desk.
    /// </summary>
    /// <returns>
    /// Returns reservations in order of:
    /// all due in reservations,
    /// all checked in reservations,
    /// checked out reservations of the current date,
    /// and confirmed reservations with a check in date from the current date onward.
    /// </returns>
    public Task<List<ReservationDTO>> GetForDeskAsync();
    public Task UpdateStatusAndRoomsAsync(string id, UpdateReservationDTO dto);

    /// <summary>
    /// Change the status of Confirmed reservatons to Due In, for 
    /// those with check in dates that match the current date.
    /// </summary>
    public Task UpdateConfirmedToDueInAsync();
}
