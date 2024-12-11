using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Application.Abstractions.Services;

namespace Application.Services.Repository;

public class ImageService : IImageService
{
    public async Task<List<string>> UploadRoomImagesAsync(List<IFormFile> images, string id)
    {
        var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);

        // upload each image file
        for (int i = 0; i < images.Count; i++)
        {
            var upload = new TransferUtilityUploadRequest
            {
                InputStream = images[i].OpenReadStream(),
                Key = $"rooms/{id}/image-{i + 1}", // side-steps file name special characters
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
