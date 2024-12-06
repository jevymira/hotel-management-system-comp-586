using Domain.Entities;

namespace Domain.Models;

/// <summary>
/// Class storing references to lists of rooms based on Room Type.
/// </summary>
public class RoomsByType
{
    public required List<Room> Single { get; set; }
    public required List<Room> Double { get; set; }
    public required List<Room> Triple { get; set; }
    public required List<Room> Quad { get; set; }
}
