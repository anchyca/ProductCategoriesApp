using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueApp.Services
{
    public interface IStorageService
    {
        Task<bool> UploadImageToStorage(IFormFile file);
        Task<bool> DeleteImageFromStorage(IFormFile file);
        string GetImagePath(string imageName);
    }
}
