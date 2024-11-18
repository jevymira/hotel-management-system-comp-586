using Abstractions;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Authorization;
using Domain.Entities;
using Domain.Models;
using Domain.Abstractions.Services;
using LambdaASP.NETCore.Services;
using System;
using System.Collections.Concurrent;

namespace LambdaASP.NETCore.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    AmazonDynamoDBClient _client;
    Random _random;
    IReservationService _reservationService;

    public ReservationsController(
        IDBClientFactory<AmazonDynamoDBClient> factory,
        IReservationService reservationService)
    {
        _client = factory.GetClient();
        _random = new Random();
        _reservationService = reservationService;
    }

    [HttpGet("by-id/{id}")] // GET api/reservations/by-id/0123456789
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ActionName(nameof(GetReservationAsync))] // CreatedAtAction and .NET Async suffixing
    public async Task<IActionResult> GetReservationAsync(string id)
    {
        try
        {
            return Ok(await _reservationService.ReadReservationAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("by-name/{name}")] // GET api/reservations/by-name/John%20Doe
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByName(string name)
    {
        return Ok(await _reservationService.ReadReservationsByNameAsync(name));
    }

    // returns in order of:
    // all due in reservations
    // all checked in reservations
    // checked out reservations of the current date
    // confirmed reservations with a check in date from the current date onward
    [HttpGet] // GET api/reservations
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> QueryByDate()
    {
        return Ok(await _reservationService.ReadReservationsForCurrentDay());
    }

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
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReservation([FromBody] PostReservationDTO reservationDTO)
    {
        var reservation = await _reservationService.CreateAsync(reservationDTO);
        return CreatedAtAction(nameof(GetReservationAsync), new { id = reservation.ReservationID }, value: reservation);
    }

    /* sample request body
    {
        "reservationStatus": "Checked In",
        "roomNumbers": [ // not room ids
            "MO",
            "CK"
        ],
        "roomStatus": "Occupied"
    }
    // alternative sample body
    {
        "reservationStatus": "Checked Out",
        // omit room numbers
        "roomStatus": "Empty"
    }
    */
    [HttpPatch("by-id/{id}")] // PATCH api/reservations/by-id/0123456789
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchCheckReservation(
        string id, [FromBody] CheckInOutDTO model)
    {
        await _reservationService.CheckReservationInOutAsync(id, model);
        return NoContent();
    }

}
