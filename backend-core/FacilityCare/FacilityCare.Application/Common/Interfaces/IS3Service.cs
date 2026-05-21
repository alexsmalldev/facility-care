namespace FacilityCare.Application.Common.Interfaces;

public interface IS3Service
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileUrl);
}