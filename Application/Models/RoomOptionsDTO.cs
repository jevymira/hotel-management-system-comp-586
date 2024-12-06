using Domain.Models;

namespace Application.Models;

public class RoomOptionsDTO
{
    public List<RoomOptionDTO> PrimaryOption { get; set; } = new List<RoomOptionDTO>();
    public List<List<RoomOptionDTO>> AlternativeOptions { get; set; } = new List<List<RoomOptionDTO>>();
}
