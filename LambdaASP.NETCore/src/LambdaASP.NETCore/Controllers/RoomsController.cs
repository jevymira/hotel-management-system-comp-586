using Abstractions;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LambdaASP.NETCore.Controllers;

[Authorize]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly IImageService _imageService;
    public RoomsController(IRoomService roomService, IImageService imageService)
    {
        _roomService = roomService;
        _imageService = imageService;
    }

    [AllowAnonymous]
    [HttpGet("{id}")] // GET api/rooms/0123456789
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ActionName(nameof(GetRoomAsync))] // CreatedAtAction and .NET Async suffixing
    public async Task<IActionResult> GetRoomAsync(string id)
    {
        try
        {
            return Ok(await _roomService.GetRoomAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet] // GET api/rooms
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoomsAsync()
    {
        return Ok(await _roomService.GetAllAsync());
    }

    // query string: CASE SENSITIVE (Double != double)
    [HttpGet("empty")] // GET api/rooms/empty?type=Double
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmptyRoomsAsync([BindRequired][FromQuery] string type)
    {
        if (ModelState.IsValid)
        {
            return Ok(await _roomService.GetEmptyRoomsByType(type));
        }
        return BadRequest("Room Type required in request query string.");
    }

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
        Room room;

        try
        {
            room = await _roomService.CreateAsync(roomDTO, images);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }

        return CreatedAtAction(nameof(GetRoomAsync), new { id = room.RoomID }, value: room);
    }

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
        try
        {
             await _roomService.UpdateAsync(id, roomDTO, images);
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