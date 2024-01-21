using System;
using Azure.Storage.Queues.Models;
using ImageSharingModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ImageSharingFunctions
{
    /*
     * This function responds to approval of an image, by inserting metadata for the image into the database.
     */

    // https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2?tabs=isolated-process%2Cextensionv4&pivots=programming-language-csharp

    public class ApproveResponder
    {
        private const string CosmosDbConnectionString = "CosmosDbConnectionString";

        private const string DatabaseName = "imagesharing";

        private const string ContainerName = "images";
        
        private readonly ILogger<ApproveResponder> _logger;

        private readonly Container _container;


        public ApproveResponder(ILogger<ApproveResponder> logger,
                                CosmosClient client)
        {
            _logger = logger;
            Database database = client.GetDatabase(DatabaseName);
            _container = database.GetContainer(ContainerName);
        }

        public static CosmosClient GetImageDbClient()
        {
            string connectionString = Environment.GetEnvironmentVariable(CosmosDbConnectionString);
            CosmosClient client = new CosmosClient(connectionString);
            return client;
        }

        // TODO annotate for trigger (use approved-images queue trigger)
        [Function(nameof(ApproveResponder))]
        public async Task Run([QueueTrigger("approved-images", Connection = "QueueStorageConnectionString")]QueueMessage message)
        {
            string imageJson = ImageProperties.messageTextToString(message.MessageText);
            _logger.LogInformation($"Processing approval of image:\n{imageJson}");

            _logger.LogInformation($"Saving image metadata to CosmosDB database.");
            Image image = null;
            // TODO deserialize the image object from the message queue, set Approved, and add it to the database
            image = ImageProperties.messageTextToImage(message.MessageText);
            image.Approved = true;
            await this._container.CreateItemAsync<Image>(image, new PartitionKey(image.UserId)); 
        }
    }
}
