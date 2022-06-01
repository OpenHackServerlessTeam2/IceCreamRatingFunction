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
using System.Linq;

namespace OpenHack.Challenge
{
   
    

    public static class GetRatings
    {
        private static ObjectResult ItemNotFoundResponse(string message)
        {
            var result = new ObjectResult(message);
            result.StatusCode = 404;
            return result;
        }


        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,


            [CosmosDB(
                databaseName:@"OpenHack", 
                collectionName:@"Rating", 
                ConnectionStringSetting = @"RatingCosmosConnectionString")] 
                IEnumerable<Response> allRatings,
                ILogger log
            )

        {
            string userId = null;

            if (req.GetQueryParameterDictionary()?.TryGetValue(@"userId", out userId) == true
                && !string.IsNullOrWhiteSpace(userId))
            {
                var userRatings = allRatings.Where(r => r.userId == userId);
                return !userRatings.Any() ? new NotFoundObjectResult($@"No ratings found for user '{userId}'") : (IActionResult)new OkObjectResult(userRatings);

            }
            else
            {
                return new BadRequestObjectResult(@"userId is required as a query parameter");
            }
        }
    }
}
