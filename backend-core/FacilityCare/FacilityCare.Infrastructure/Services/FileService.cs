using FacilityCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FacilityCare.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadPath;

    public FileService()
    {
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        ValidateFile(file);

        var folderPath = Path.Combine(_uploadPath, folder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
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