namespace Application.Models;

public class RoomOptionsDTO
{
    public RoomOptionDTO? primaryOption { get; set; }
    public List<RoomOptionDTO> alternativeOptions { get; set; } = new List<RoomOptionDTO>();
}
