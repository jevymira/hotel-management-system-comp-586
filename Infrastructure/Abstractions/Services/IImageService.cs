using Microsoft.AspNetCore.Http;

namespace Infrastructure.Abstractions.Services;

public interface IImageService
{
    public Task<List<string>> UploadRoomImagesAsync(List<IFormFile> images, string id);
}
