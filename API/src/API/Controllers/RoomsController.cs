﻿using Application.Abstractions.Services;
using Application.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    /// <summary>
    /// Retrieve a room by its ID.
    /// </summary>
    /// <param name="id">The room ID.</param>
    /// <response code="404">No room exists with the supplied ID.</response>
    /// <response code="200">The room is retrieved successfully.</response>
    [AllowAnonymous]
    [HttpGet("by-id/{id}")] // GET api/rooms/abc123
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ActionName(nameof(GetRoomAsync))] // CreatedAtAction and .NET Async suffixing
    public async Task<IActionResult> GetRoomAsync(string id)
    {
        var room = await _roomService.GetRoomAsync(id);
        if (room == null) { return NotFound($"No room exists with Room ID {id}."); }
        return Ok(room);
    }

    // use case: Home page
    /// <summary>
    /// Retrieve the room type(s) and their quantity that can service the stay date, room count, and guest count.
    /// </summary>
    /// <param name="start">CheckInDate</param>
    /// <param name="end">CheckOutDate</param>
    /// <param name="guests">Room Count</param>
    /// <param name="rooms">Guest Count</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("search")] // GET api/rooms/search?start=2024-12-10&end=2024-12-12&rooms=2&guests=5
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomOptionsAsync(
        [FromQuery] string start,
        [FromQuery] string end,
        [FromQuery] int rooms,
        [FromQuery] int guests)
    {
        var options = await _roomService.GetMatchingRoomsAsync(start, end, rooms, guests);
        return Ok(options);
    }

    // use case: Admin Rooms page, Rooms
    /// <summary>
    /// Retrieve all rooms.
    /// </summary>
    /// <response code="200">Rooms are retrieved successfully.</response>
    [HttpGet] // GET api/rooms
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomsAsync()
    {
        return Ok(await _roomService.GetAllAsync());
    }

    // use case: Admin Desk page, Assign Room(s) (of selected type, empty)
    /// <summary>
    /// Retrieve all empty rooms of a specified type.
    /// </summary>
    /// <remarks>
    /// query string: CASE SENSITIVE (Double != double)
    /// </remarks>
    /// <param name="type">Room type.</param>
    /// <response code="400">Room Type absent from the query string.</response>
    /// <response code="200">Rooms are retrieved successfully.</response>
    [HttpGet("empty")] // GET api/rooms/empty?type=Double
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmptyRoomsAsync([BindRequired][FromQuery] string type)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Room Type required in request query string.");
        }

        return Ok(await _roomService.GetEmptyRoomsByType(type));
    }

    // use case: Admin Rooms page, Add Room
    /// <summary>
    /// Add a new room and upload its images.
    /// </summary>
    /// <param name="dto">Image files.</param>
    /// <response code="409">Room Number already in use.</response>
    /// <response code="201">Room added successfully.</response>
    [HttpPost] // POST api/rooms
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> PostRoomAsync(
        // [FromForm] PostRoomDTO roomDTO,
        // [FromForm(Name = "images")] string images
        [FromBody] PostRoomDTO dto)
    {
        try
        {
            Room room = await _roomService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetRoomAsync), new { id = room.RoomID }, value: room);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // use case: Admin Rooms page, Edit Room
    /// <summary>
    /// Edit the details and images of the room with the specified ID.
    /// </summary>
    /// <param name="id">The room ID.</param>
    /// <response code="404">No room exists with the specified Room ID.</response>
    /// <response code="409">Room number already in use with another room.</response>
    /// <response code="204">Room edited successfully.</response>
    [HttpPatch("{id}")] // PATCH api/rooms/abc123
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchRoomAsync(
        [FromRoute] string id,
        // [FromForm] UpdateRoomDTO roomDTO,
        // [FromForm(Name = "images")] List<IFormFile> images
        [FromBody] UpdateRoomDTO dto)
    {
        try
        {
            await _roomService.UpdateAsync(id, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }
}