namespace Domain.Models;

public class RoomOptionDTO
{
    public required string Type { get; set; }
    public required int Quantity { get; set; }
    public decimal Price { get; set; }
    public List<string>? ImageUrls { get; set; }
}
