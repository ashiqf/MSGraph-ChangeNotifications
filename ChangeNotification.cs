using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSGraph_ChangeNotifications.Common;
using System.Linq;

namespace MSGraph_ChangeNotifications
{
    public static class ChangeNotification
    {
        [FunctionName("ChangeNotification")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // parse query parameter
            var validationToken = req.Query["validationToken"];
            if (!string.IsNullOrEmpty(validationToken))
            {
                log.LogInformation("validationToken: " + validationToken);
                return new ContentResult { Content = validationToken, ContentType = "text/plain" };
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Notifications>(requestBody);
            if (!data.Value.FirstOrDefault().ClientState.Equals("SecretClientState", StringComparison.OrdinalIgnoreCase))
            {
                //client state is not valid (doesn't match the one submitted with the subscription)
                return new BadRequestResult();
            }

            // Get users teams presence 
            foreach (var notification in data.Value)
            {
                var userTeamsAvailabilityStatus = notification.ResourceData?.Availability;
                log.LogInformation(userTeamsAvailabilityStatus);
            }

            return new OkResult();
        }
    }
}
