using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Domain.Models;
using Application.Abstractions.Services;
using Application.Models;
using System.Transactions;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    // use case: Admin Desk page, search by booking number
    [HttpGet("by-id/{id}")] // GET api/reservations/by-id/0123456789
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ActionName(nameof(GetReservationAsync))] // CreatedAtAction and .NET Async suffixing
    public async Task<IActionResult> GetReservationAsync(string id)
    {
        var reservation = await _reservationService.GetAsync(id);
        if (reservation == null) { return NotFound($"No reservation exists with Reservation ID {id}."); }
        return Ok(reservation);
    }

    // use case: Admin Desk page, search by Guest
    [HttpGet("by-name/{name}")] // GET api/reservations/by-name/John%20Doe
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByNameAsync(string name)
    {
        return Ok(await _reservationService.GetByGuestNameAsync(name));
    }

    // use case: Admin Reservations page, Reservations
    // returns:
    // all due in reservations
    // all checked in reservations
    // checked out reservations of the current date
    // confirmed reservations with a check in date from the current date onward
    [HttpGet] // GET api/reservations
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForDeskAsync()
    {
        return Ok(await _reservationService.GetForDeskAsync());
    }

    // use case: Booking Page, Booking Confirmation Page
    /* sample request body
    {
        "RoomType": "Double",
        "OrderQuantity": 1,
        "TotalPrice" : 100.00,
        "CheckInDate": "2024-12-01",
        "CheckOutDate": "2024-12-03",
        "NumberOfGuests": 2,
        "GuestFullName": "John Doe",
        "GuestEmail": "jdoe@email.com",
        "GuestDateOfBirth": "1980-01-01"
        // "GuestPhoneNumber": "(555) 555-5555" // (optional)
    }  
    */
    [HttpPost] // POST api/reservations
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> PostAsync([FromBody] PostReservationDTO reservationDTO)
    {
        try
        {
            var reservation = await _reservationService.AddAsync(reservationDTO);
            return CreatedAtAction(nameof(GetReservationAsync), new { id = reservation.ReservationID }, value: reservation);
        }
        catch (ArgumentException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
    }

    // use case: Admin Desk page, Save reservation changes at check in/out
    /* sample request body
    {
        "reservationStatus": "Checked In",
        "roomNumbers": [ // (not room ids)
            "MO",
            "CK"
        ],
        "updatedBy": "TEST"
    }
    // alternative sample body
    {
        "reservationStatus": "Checked Out",
        // omit room numbers
        "updatedBy": "TEST"
    }
    */
    [HttpPatch("by-id/{id}")] // PATCH api/reservations/by-id/0123456789
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchCheckInOut(
        string id, [FromBody] CheckInOutDTO model)
    {
        try
        {
            await _reservationService.UpdateStatusAndRoomsAsync(id, model);
        }
        catch (ArgumentException ex)
        {
            return UnprocessableEntity(ex.Message);
        }
        catch (TransactionException ex)
        {
            return StatusCode(500, ex.Message);
        }

        return NoContent();
    }

}
