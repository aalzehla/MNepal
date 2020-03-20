using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MNepalProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();

            if (ex is HttpAntiForgeryException)
            {
                Response.Clear();
                Server.ClearError(); //make sure you log the exception first
                Response.Redirect("/MNepal/Error/Invalid_Request", true);
            }
            else
            {
                Response.Clear();
                Server.ClearError(); //make sure you log the exception first
                Response.Redirect("/MNepal/Error/ApplicationError", true);
            }
        }
    }
}
