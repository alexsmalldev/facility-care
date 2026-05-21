using FacilityCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FacilityCare.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IS3Service _s3Service;

    public FileService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        ValidateFile(file);

        using var stream = file.OpenReadStream();
        return await _s3Service.UploadFileAsync(stream, file.FileName, file.ContentType);
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        await _s3Service.DeleteFileAsync(fileUrl);
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length > 5 * 1024 * 1024)
            throw new Exception("File size must not exceed 5MB.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType))
            throw new Exception("Only JPEG, PNG and GIF files are allowed.");
    }
}