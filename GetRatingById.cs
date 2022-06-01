using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OpenHack.Challenge
{
   
    

    public static class GetRatingById
    {
        private static ObjectResult ItemNotFoundResponse(string message)
        {
            var result = new ObjectResult(message);
            result.StatusCode = 404;
            return result;
        }

        private static async Task<T> GetItemFromApi<T>(HttpClient client, string endpoint)
        {
            var response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseString);
        }

        [FunctionName("GetRatingByRatingId")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = "rating/{ratingId}")] 
             HttpRequest req,
             string ratingId,
            [CosmosDB(
                databaseName: "OpenHack",
                collectionName: "Rating",
                ConnectionStringSetting = "RatingsDatabase",
                Id = "{ratingId}",
                PartitionKey = "{ratingId}" 
                )] Response response,
            ILogger log)
        {
            if (response == null)
        
            {
                return ItemNotFoundResponse($"rating ID = {req.Query["ratingId"]} not found");
            }

           

            return new JsonResult(response);
        }
    }
}
