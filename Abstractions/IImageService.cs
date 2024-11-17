using Microsoft.AspNetCore.Http;

namespace Abstractions;

public interface IImageService
{
    public Task<List<string>> UploadRoomImagesAsync(List<IFormFile> images, string id);
}
