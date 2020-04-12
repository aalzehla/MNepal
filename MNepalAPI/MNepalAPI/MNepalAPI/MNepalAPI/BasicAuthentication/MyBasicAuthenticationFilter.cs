using System.Web.Http.Controllers;

namespace MNepalAPI.BasicAuthentication
{
    public class MyBasicAuthenticationFilter : BasicAuthenticationFilter
    {
        public MyBasicAuthenticationFilter()
        { }

        public MyBasicAuthenticationFilter(bool active)
            : base(active)
        { }


        protected override bool OnAuthorizeUser(string username, string password, HttpActionContext actionContext)
        {
            var userApi = new ApiSecurity();

            var user = userApi.AuthenticateAndLoad(username, password);
            if (user == null)
                return false;

            return true;
        }
    }
}