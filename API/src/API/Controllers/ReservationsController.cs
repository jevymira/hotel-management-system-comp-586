using Application.Abstractions.Services;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// <summary>
    /// Get reservation by booking/confirmation number.
    /// </summary>
    /// <param name="id">The booking/confirmation number.</param>
    /// <response code="404">No reservation exists for the provided booking/confirmation number.</response>
    /// <response code="200">The reservation is retrieved successfully.</response>
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
    /// <summary>
    /// Retrieve all reservations with the associated full name of a guest.
    /// </summary>
    /// <param name="name">Full Name.</param>
    /// <response code="200">The reservations, if any, are retrieved successfully.</response>
    [HttpGet("by-name/{name}")] // GET api/reservations/by-name/John%20Doe
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByNameAsync(string name)
    {
        return Ok(await _reservationService.GetByGuestNameAsync(name));
    }

    // use case: Admin Reservations page, Reservations
    /// <summary>
    /// Retrieve:
    /// all Due In reservations,
    /// all Checked In reservations,
    /// only Checked Out reservations of the current date, and
    /// only Confirmed reservations with a check in date from the current date onward.
    /// </summary>
    /// <response code="200">The reservations, if any, are retrieved successfully.</response>
    [HttpGet] // GET api/reservations
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForDeskAsync()
    {
        return Ok(await _reservationService.GetForDeskAsync());
    }

    // use case: Booking Page, Booking Confirmation Page
    /// <summary>
    /// Create a new reservation.
    /// </summary>
    /// <param name="reservationDTO"></param>
    /// <response code="422">Reservation violated business rule (including overbooking).</response>
    /// <response code="201">Reservation created successfully; returned with confirmation number.</response>
    /* sample request body
    {
        "RoomType": "Single",
        "OrderQuantity": 1,
        "TotalPrice" : 75.00,
        "CheckInDate": "2024-12-12",
        "CheckOutDate": "2024-12-13",
        "NumberOfGuests": 1,
        "GuestFullName": "John Doe",
        "GuestEmail": "jdoe@email.com",
        "GuestDateOfBirth": "1980-01-01"
        // "GuestPhoneNumber": "(555) 555-5555" // (optional)
    }  
    */
    [AllowAnonymous]
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
    /// <summary>
    /// Edit a reservation's details (including its status) and room assignment(s).
    /// </summary>
    /// <param name="id">Reservation ID.</param>
    /// <response code="400">Missing parameter in request.</response>
    /// <response code="404">No reservation exists with provided ID.</response>
    /// <response code="422">Violation of one or more business rules.</response>
    /// <response code="500">Server unable to carry out update.</response>
    /// <response code="204">Reservation update is successful.</response>
    /* sample request body
    {
        "guestFullName": "John Doe",
        "guestDateOfBirth": "1980-01-01",
        "guestEmail": "jdoe@email.com",
        // "GuestPhoneNumber": "(555) 555-5555", // optional
        "reservationStatus": "Checked In",
        "roomNumbers": [ // (not room ids)
            "4",
            "5"
        ],
        "checkInDate": "2024-12-12",
        "checkOutDate": "2024-12-13",
        "updatedBy": "CI#UPDATE#TEST"
    }
    // alternative sample body
    {
        "guestFullName": "John Doe",
        "guestDateOfBirth": "1980-01-01",
        "guestEmail": "jdoe@email.com",
        // "GuestPhoneNumber": "(555) 555-5555", // optional
        "reservationStatus": "Checked Out", // Due In/Confirmed/Cancelled
        // "roomNumbers": // omit
        "checkInDate": "2024-12-12",
        "checkOutDate": "2024-12-13",
        "updatedBy": "CO#UPDATE#TEST"
    }
    */
    [HttpPatch("by-id/{id}")] // PATCH api/reservations/by-id/0123456789
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchCheckInOut(
        string id, [FromBody] UpdateReservationDTO model)
    {
        if (!(ModelState.IsValid))
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _reservationService.UpdateStatusAndRoomsAsync(id, model);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
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

    // invoked daily at hotel-wide check in time (02:00 PM PST) by EventBridge Rule trigger
    /// <summary>
    /// Change the status of Confirmed reservatons to Due In, for 
    /// those with check in dates that match the current date.
    /// </summary>
    /// <response code="204">Reservation(s) updated to Due In.</response>
    [AllowAnonymous]
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchScheduledDueIn()
    {
        await _reservationService.UpdateConfirmedToDueInAsync();

        return NoContent();
    }

}
