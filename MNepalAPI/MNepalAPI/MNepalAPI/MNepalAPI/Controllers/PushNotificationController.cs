using MNepalAPI.BasicAuthentication;
using MNepalAPI.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using static MNepalAPI.Models.Notifications;

namespace MNepalAPI.Controllers
{
    [MyBasicAuthenticationFilter]
    public class PushNotificationController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> PushNotification([FromBody] Notificationsobject notifications)
        {
            var notificationsobject = new Notificationsobject
            {
                to = notifications.to,
                data = new Data
                {
                    extra_information = notifications.data.extra_information
                },
                notification = new Notification
                {
                    title = notifications.notification.title,
                    text = notifications.notification.text
                }
            };

            // Serialize our concrete class into a JSON String
            var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(notificationsobject));

            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                var AuthorizationKey = System.Configuration.ConfigurationManager.AppSettings["AuthorizationKey"];
                var AuthorizationKeyValue = System.Configuration.ConfigurationManager.AppSettings["AuthorizationKeyValue"];
                var NotificationPostUrl = System.Configuration.ConfigurationManager.AppSettings["NotificationPostUrl"];
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthorizationKey, AuthorizationKeyValue);

                // Do the actual request and await the response
                var httpResponse = await httpClient.PostAsync(NotificationPostUrl, httpContent);

                // If the response contains content we want to read it!
                if (httpResponse.Content != null)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();

                    //Deserialize Object to get object value
                    var result = JsonConvert.DeserializeObject<Response>(responseContent);
                    var message_Id = result.message_id;

                    NotificationModel notification = new NotificationModel();
                    notification.extraInformation = notifications.data.extra_information;
                    notification.text = notifications.notification.text;
                    notification.title = notifications.notification.title;
                    notification.messageId = message_Id;

                    //Database
                    int resultsPayments = NotificationUtilities.Notification(notification);
                    if (resultsPayments == -1)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { notification });
                    }
                    //return Request.CreateResponse(HttpStatusCode.OK,responseContent);
                    // From here on you could deserialize the ResponseContent back again to a concrete C# type using Json.Net
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);

        }
    }
}
