using MNepalAPI.BasicAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace MNepalAPI.Controllers
{
    [MyBasicAuthenticationFilter]
    public class HelpController : ApiController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> ContactUs()
        {
            using (var httpClient = new HttpClient())
            {
                // Do the actual request and await the response
                var URL = System.Configuration.ConfigurationManager.AppSettings["HelpUrl"];
                var httpResponse = await httpClient.GetAsync(URL);

                // If the response contains content we want to read it!
                if (httpResponse.Content != null)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, responseContent);
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
