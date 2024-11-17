using Microsoft.AspNetCore.Http;

namespace Abstractions;

public interface IImageService
{
    public Task<List<string>> UploadImagesAsync(List<IFormFile> images, string id);
}
