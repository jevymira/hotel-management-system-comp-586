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

    [AllowAnonymous]
    [HttpGet("{id}")] // GET api/rooms/0123456789
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ActionName(nameof(GetRoomAsync))] // CreatedAtAction and .NET Async suffixing
    public async Task<IActionResult> GetRoomAsync(string id)
    {
        var room = await _roomService.GetRoomAsync(id);
        if (room == null) { return NotFound($"No room exists with Room ID {id}."); }
        return Ok(room);
    }

    // use case: Admin Rooms page, Rooms
    [HttpGet] // GET api/rooms
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomsAsync()
    {
        return Ok(await _roomService.GetAllAsync());
    }

    // use case: Admin Desk page, Assign Room(s) (of selected type, empty)
    // query string: CASE SENSITIVE (Double != double)
    [HttpGet("empty")] // GET api/rooms/empty?type=Double
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmptyRoomsAsync([BindRequired][FromQuery] string type)
    {
        if (ModelState.IsValid)
            return Ok(await _roomService.GetEmptyRoomsByType(type));
        return BadRequest("Room Type required in request query string.");
    }

    // use case: Admin Rooms page, Add Room
    // request Header: ( Key: Content-Type, Value: multipart/form-data; boundary=<parameter> )
    // request Body:
    //   form-data for content-type: application/json
    //     RoomDTO[roomTypeID], RoomDTO[maxOccupancy], RoomDTO[pricePerNight], RoomDTO[roomNumber]
    //   form-data for content-type: multipart/form-data
    //     images
    [HttpPost] // POST api/rooms
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> PostRoomAsync(
        [FromForm] PostRoomDTO roomDTO,
        [FromForm(Name = "images")] List<IFormFile> images)
    {
        Room? room = await _roomService.CreateAsync(roomDTO, images);
        if (room == null)
            return Conflict($"Room Number {roomDTO.RoomNumber} is already in use.");
        return CreatedAtAction(nameof(GetRoomAsync), new { id = room.RoomID }, value: room);
    }

    // use case: Admin Rooms page, Edit Room
    // request Header: ( Key: Content-Type, Value: multipart/form-data; boundary=<parameter> )
    // request Body:
    //   form-data for content-type: application/json
    //     RoomDTO[roomTypeID], RoomDTO[maxOccupancy], RoomDTO[pricePerNight], RoomDTO[roomNumber], RoomDTO[UpdatedBy]
    //   form-data for content-type: multipart/form-data
    //     images
    [HttpPatch("{id}")] // PATCH api/rooms/0123456789
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // when room number already in use
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchRoomAsync(
        [FromRoute] string id,
        [FromForm] UpdateRoomDTO roomDTO,
        [FromForm(Name = "images")] List<IFormFile> images)
    {
        var result = await _roomService.UpdateAsync(id, roomDTO, images);

        if (!result.IsSuccess)
        {
            if (result.Error.Description.Equals("NotFound"))
                return NotFound($"No room exists with Room ID {id}.");
            else if (result.Error.Description.Equals("Conflict"))
                return Conflict($"Room Number {roomDTO.RoomNumber} is already in use with another room.");
        }

        return NoContent();
    }

}