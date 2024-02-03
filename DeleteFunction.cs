using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System;

namespace CosmosDbCrudFunctions
{
    public static class DeleteFunction
    {
        private static readonly string EndpointUri = Environment.GetEnvironmentVariable("CosmosDbEndpointUri");
        private static readonly string PrimaryKey = Environment.GetEnvironmentVariable("CosmosDbPrimaryKey");
        private static readonly string DatabaseName = "ToDoList";
        private static readonly string ContainerName = "Items";
        private static CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        [Function("DeleteItem")]
        public static async Task<HttpResponseData> DeleteItem(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "items/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("DeleteItem");
            logger.LogInformation($"Deleting item with ID: {id}");

            Container container = cosmosClient.GetContainer(DatabaseName, ContainerName);

            try
            {
                // Attempt to delete the item. If it does not exist, a CosmosException with status code 404 is thrown.
                await container.DeleteItemAsync<object>(id, new PartitionKey(id));
                var okResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                okResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await okResponse.WriteStringAsync($"Successfully deleted item with ID: {id}");
                return okResponse;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                notFoundResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await notFoundResponse.WriteStringAsync($"Item with ID: {id} not found.");
                return notFoundResponse;
            }
        }
    }
}
