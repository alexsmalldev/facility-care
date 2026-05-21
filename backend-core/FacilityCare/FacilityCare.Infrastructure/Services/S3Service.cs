using Amazon.S3;
using Amazon.S3.Model;
using FacilityCare.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FacilityCare.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IConfiguration configuration)
    {
        _bucketName = configuration["AWS:BucketName"]!;
        _s3Client = new AmazonS3Client(
            configuration["AWS:AccessKey"],
            configuration["AWS:SecretKey"],
            Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]!)
        );
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var key = $"service_icons/{Guid.NewGuid()}_{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        try
        {
            await _s3Client.PutObjectAsync(request);
        }
        catch (AmazonS3Exception ex)
        {
            throw new Exception($"S3 upload failed: {ex.Message} | ErrorCode: {ex.ErrorCode} | StatusCode: {ex.StatusCode}");
        }

        return $"https://{_bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        var uri = new Uri(fileUrl);
        var key = uri.AbsolutePath.TrimStart('/');

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        });
    }
}