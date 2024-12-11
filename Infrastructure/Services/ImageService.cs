using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Domain.Abstractions.Services;

namespace Infrastructure.Services;

/// <summary>
/// Service to upload images to AWS S3, creating a resource in turn.
/// </summary>
public class ImageService : IImageService
{
    /// <summary>
    /// Uploads the specified images and creates a resource in the name of the ID.
    /// </summary>
    /// <param name="images">Images to be uploaded.</param>
    /// <param name="id">ID under which a resource is created.</param>
    public async Task<List<string>> UploadRoomImagesAsync(List<IFormFile> images, string id)
    {
        var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);

        // upload each image file
        for (int i = 0; i < images.Count; i++)
        {
            var upload = new TransferUtilityUploadRequest
            {
                InputStream = images[i].OpenReadStream(),
                Key = $"rooms/{id}/image-{i+1}", // side-steps file name special characters
                BucketName = "travelers-inn-images"
            };
            var utility = new TransferUtility(client);
            await utility.UploadAsync(upload);
        }

        // from the folder, get all image urls
        List<string> urls = new List<string>();
        ListObjectsRequest req = new ListObjectsRequest
        {
            BucketName = "travelers-inn-images",
            Prefix = $"rooms/{id}/"
        };
        var response = await client.ListObjectsAsync(req);
        foreach (S3Object items in response.S3Objects)
        {
            urls.Add("https://travelers-inn-images.s3.us-east-1.amazonaws.com/" + items.Key);
        }

        return urls;
    }
}
