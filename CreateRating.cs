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
    public class Rating
    {
        public string userId { get; set; }

        public string productId { get; set; }

        public string locationName { get; set; }

        public int? rating { get; set; }

        public string userNotes { get; set; }
    }

    public class User
    {

        public string userId { get; set; }

        public string userName { get; set; }

        public string fullName { get; set; }
    }

    public class Product
    {
        public string productId { get; set; }

        public string productName { get; set; }

        public string productDescription { get; set; }
    }

    public class Response
    {
        public string id { get; set; }

        public string userId { get; set; }

        public string productId { get; set; }

        public DateTime timeStamp { get; set; }

        public string locationName { get; set; }

        public int? rating { get; set; }

        public string userNotes { get; set; }
    }

    public static class CreateRating
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

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "OpenHack",
                collectionName: "Rating",
                ConnectionStringSetting = "RatingCosmosConnectionString")]
                IAsyncCollector<Response> ratingOut,
            ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Rating>(requestBody);

            if (
                data.userId == null 
                || data.productId == null 
                || data.locationName == null 
                || data.rating == null)
            {
                return new BadRequestObjectResult("Invalid request, required parameters missing. The required parameters are 'userId', 'productId', 'locationName' and 'rating'.");
            }

            if (data.rating < 0 || data.rating > 5)
            {
                return new BadRequestObjectResult("The 'rating' parameter must be between 0 and 5");
            }

            User user = null;
            var client = new HttpClient();

            try
            {
                user = await GetItemFromApi<User>(
                    client, 
                    $"https://serverlessohapi.azurewebsites.net/api/GetUser?userId={data.userId}"
                );
            }
            catch (HttpRequestException)
            {
                return ItemNotFoundResponse($"User not found for userId = {data.userId}");
            }

            Product product = null;

            try
            {
                product = await GetItemFromApi<Product>(
                    client, 
                    $"https://serverlessohapi.azurewebsites.net/api/GetProduct?productId={data.productId}"
                );
            }
            catch (HttpRequestException)
            {
                return ItemNotFoundResponse($"Product not found for productId = {data.productId}");
            }

            var finalResponse = new Response()
            {
                id = Guid.NewGuid().ToString(),
                userId = user.userId,
                productId = product.productId,
                timeStamp = DateTime.UtcNow,
                locationName = data.locationName,
                rating = data.rating,
                userNotes = data.userNotes
            };

            await ratingOut.AddAsync(finalResponse);

            return new JsonResult(finalResponse);
        }
    }
}
