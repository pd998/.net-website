using System;
using Azure.Storage.Blobs;
using Azure.Storage.Queues.Models;
using ImageSharingModels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ImageSharingFunctions
{
    /*
     * This function responds to rejection of an image, by deleting it from blob storage.
     */


    public class RejectResponder
    {
        private readonly ILogger<RejectResponder> _logger;

        private const string BlobStorageConnectionString = "BlobStorageConnectionString";

        private const string BlobContainer = "images";

        public RejectResponder(ILogger<RejectResponder> logger)
        {
            _logger = logger;
        }

        // TODO annotate for trigger (use rejected-images queue, no output but blob client used to delete image)
        [Function(nameof(RejectResponder))]
        public async Task Run([QueueTrigger("rejected-images", Connection = "QueueStorageConnectionString")] QueueMessage message)
        {
            _logger.LogInformation($"Rejected approval of image: {message.MessageText}");

            Image image = null;
            // TODO deserialize the image object from the message queue.
            image = ImageProperties.messageTextToImage(message.MessageText);
            // TODO get connection string from environment variable and delete the blob
            string connectionString = Environment.GetEnvironmentVariable(BlobStorageConnectionString);
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainer);
            BlobClient blobClient = blobContainerClient.GetBlobClient(image.Id);
            _logger.LogInformation("Deleting image blob at URI {0}", blobClient.Uri);
            await blobClient.DeleteAsync();
        }
    }
}
