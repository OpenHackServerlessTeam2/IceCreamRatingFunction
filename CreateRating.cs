using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
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

        public int rating { get; set; }

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

        public int rating { get; set; }

        public string userNotes { get; set; }
    }

    public class BadResponse
    {
        public string message { get; set; }
    }

    public static class CreateRating
    {
        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Rating data = JsonConvert.DeserializeObject<Rating>(requestBody);

            if (data.userId == null || data.productId == null)
            {
                return new BadRequestResult();
            }

            HttpClient client = new HttpClient();

            User user = null;

            try
            {
                var response = await client.GetAsync($"https://serverlessohapi.azurewebsites.net/api/GetUser?userId={data.userId}");
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                user = JsonConvert.DeserializeObject<User>(responseString);
            }
            catch (HttpRequestException)
            {
                return new NotFoundResult();
            }

            Product product = null;
            try
            {
                var response = await client.GetAsync($"https://serverlessohapi.azurewebsites.net/api/GetProduct?productId={data.productId}");
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                product = JsonConvert.DeserializeObject<Product>(responseString);
            }
            catch (HttpRequestException)
            {
                return new NotFoundResult();
            }

            // if (data.rating < 0 || data.rating > 5)
            // {
            //     return new JsonResult(400, JsonConvert.DeserializeObject<BadResponse>("message = Rating must be between 0 and 5"));
            // }

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

            return new JsonResult(finalResponse);
        }
    }
}
