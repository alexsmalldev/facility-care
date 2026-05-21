using Microsoft.AspNetCore.Http;

namespace FacilityCare.Application.Common.Interfaces;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task DeleteFileAsync(string fileUrl);
}