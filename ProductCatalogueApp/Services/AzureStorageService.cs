using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ProductCatalogueApp.Models;
using Microsoft.Extensions.Options;

namespace ProductCatalogueApp.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly AzureStorageConfig storageConfig;
        public AzureStorageService(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        } 
        public async Task<bool> DeleteImageFromStorage(IFormFile file)
        {
            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);

            // Upload the file
            await blockBlob.DeleteIfExistsAsync();

            return await Task.FromResult(true);
        }

        public string GetImagePath(string imageName)
        {
            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageName);

            return blockBlob.Uri.ToString();
        }

        public async Task<bool> UploadImageToStorage(IFormFile file)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(storageConfig.AccountName, storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(file.FileName);

            // Upload the file
            await blockBlob.UploadFromStreamAsync(file.OpenReadStream());

            return await Task.FromResult(true);
        }

        
    }
}
