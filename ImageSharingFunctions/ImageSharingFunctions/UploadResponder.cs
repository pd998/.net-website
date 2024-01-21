using Azure.Storage.Blobs.Models;
using ImageSharingModels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ImageSharingFunctions
{
    /*
     * This function responds to completion of upload of an image, by requesting approval of the image.
     * In a production setting, it might first do some form of validation of the image such as parsing
     * it as an image, with the approval request sent after that validation succeeded.
     */

    // https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide

    // https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-vs?tabs=in-process

    // https://learn.microsoft.com/en-us/azure/storage/blobs/blob-upload-function-trigger?tabs=in-process

    // https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-output?tabs=isolated-process%2Cextensionv5&pivots=programming-language-csharp

    public class UploadResponder
    {
        private readonly ILogger<UploadResponder> _logger;

        public UploadResponder(ILogger<UploadResponder> logger)
        {
            _logger = logger;
        }

        // TODO annotate for trigger and for queue output
        [Function(nameof(UploadResponder))]
        [QueueOutput("approval-requests", Connection = "QueueStorageConnectionString")]
        public string Run( [BlobTrigger("images" + "/{blobname}",Connection = "BlobStorageConnectionString")]string myBlob, 
                          string blobname,
                          IDictionary<string, string> metadata)
        {
            Image image = new Image
            {
                UserId = metadata[ImageProperties.USER_KEY],
                Id = metadata[ImageProperties.ID_KEY],
                UserName = metadata[ImageProperties.USERNAME_KEY],
                Caption = metadata[ImageProperties.CAPTION_KEY],
                Description = metadata[ImageProperties.DESCRIPTION_KEY],
                DateTaken = JsonConvert.DeserializeObject<DateTime>(metadata[ImageProperties.DATE_TAKEN_KEY]),
                Uri = metadata[ImageProperties.URL_KEY],
                Valid = true, // Should really be validated by a microservice
                Approved = false // Requires oversight for approval
            };

            // Azure Queue requires that message payload is Base64-encoded.
            string messageText = ImageProperties.imageToMessageText(image);

            _logger.LogInformation($"Image uploaded ({blobname}), requesting approval.\nImage: {image.Uri}");

            return messageText;
        }
    }
}
