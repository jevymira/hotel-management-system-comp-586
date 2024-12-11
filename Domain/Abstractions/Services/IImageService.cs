using Microsoft.AspNetCore.Http;

namespace Domain.Abstractions.Services;

/// <summary>
/// Service to upload hotel/room images.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Uploads the specified images and creates a resource in the name of the ID.
    /// </summary>
    /// <param name="images">Images to be uploaded.</param>
    /// <param name="id">ID under which a resource is created.</param>
    public Task<List<string>> UploadRoomImagesAsync(List<IFormFile> images, string id);
}
