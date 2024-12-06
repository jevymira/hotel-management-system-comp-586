namespace Domain.Models;

/// <summary>
/// Struct (value type) to store room availabilities by room type.
/// </summary>
public struct RoomAvailabilities
{
    public int Single { get; set; }
    public int Double { get; set; }
    public int Triple { get; set; }
    public int Quad { get; set; }
}
