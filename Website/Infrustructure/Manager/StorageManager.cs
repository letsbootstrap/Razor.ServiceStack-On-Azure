using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using Website.Infrustructure.DataSource;

namespace Website.Infrustructure.Manager
{
    public interface IStorageManager
    {
        List<IListBlobItem> GetContainerContents(string container);
        CloudBlob SaveImage(int authUserId, string container, string id, string name, string fileName, string contentType, byte[] data);
        void DeleteImage(string container, string blobUri);
    }

    public class StorageManager : IStorageManager
    {
        public IRepository Repository { get; set; }              // Injected by IOC

        public StorageManager(IRepository repository)
        {
            Repository = repository;
        }

        public List<IListBlobItem> GetContainerContents(string container)
        {
            List<IListBlobItem> items = new List<IListBlobItem>();

            try
            {
                this.EnsureContainerExists(container);
                items =
                  this.GetContainer(container).ListBlobs(new BlobRequestOptions()
                  {
                      UseFlatBlobListing = true,
                      BlobListingDetails = BlobListingDetails.All
                  }).ToList();
            }
            catch (System.Net.WebException we)
            {
                Console.WriteLine("Network error: " + we.Message);
                if (we.Status == System.Net.WebExceptionStatus.ConnectFailure)
                {
                    Console.WriteLine("Please check if the blob service is running at " + ConfigurationManager.AppSettings["StorageConnectionString"]);
                }
            }
            catch (StorageException se)
            {
                Console.WriteLine("Storage service error: " + se.Message);
            }

            return items;
        }

        public CloudBlob SaveImage(int authUserId, string container, string id, string name, string fileName, string contentType, byte[] data)
        {
            // Create a blob in container and upload image bytes to it
            CloudBlob blob = this.GetContainer(container).GetBlobReference(name);
            blob.Properties.ContentType = contentType;
            // Create some metadata for this image
            var metadata = new NameValueCollection();
            metadata["Id"] = id;
            metadata["FileName"] = fileName;
            metadata["ContentType"] = contentType;
            metadata["FileSize"] = data.Length.ToString();
            // Add and commit metadata to blob
            blob.Metadata.Add(metadata);
            blob.UploadByteArray(data);

            //User user = Repository.Find<User>(u => u.UserAuthId == authUserId)[0];
            //user.UploadedSize = user.UploadedSize + data.Length;
            //Repository.Update<User>(user);

            return blob;
        }

        public void DeleteImage(string container, string blobUri)
        {
            var blob = this.GetContainer(container).GetBlobReference(blobUri);
            blob.DeleteIfExists();
        }

        private void EnsureContainerExists(string container)
        {
            var cloudContainer = GetContainer(container);
            cloudContainer.CreateIfNotExist();
            var permissions = cloudContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            cloudContainer.SetPermissions(permissions);
        }

        private CloudBlobContainer GetContainer(string container)
        {
            CloudBlobContainer cloudContainer = null;

            // Retrieve storage account from connection-string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"].ToString());

            // Create the blob client 
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container 
            cloudContainer = blobClient.GetContainerReference(container);

            try
            {
                // Create the container if it doesn't already exist
                cloudContainer.CreateIfNotExist();

                cloudContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            }
            catch (Exception ex)
            {

            }

            return cloudContainer;
        }
    }
}