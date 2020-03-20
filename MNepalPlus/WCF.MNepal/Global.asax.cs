using System;
using System.Web;
using System.Web.Routing;
using System.ServiceModel.Activation;


namespace WCF.MNepal
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new ServiceRoute("ft", new WebServiceHostFactory(), typeof(WCF.MNepal.ft)));
            RouteTable.Routes.Add(new ServiceRoute("pt", new WebServiceHostFactory(), typeof(WCF.MNepal.pt)));
            RouteTable.Routes.Add(new ServiceRoute("query", new WebServiceHostFactory(), typeof(WCF.MNepal.query)));
            RouteTable.Routes.Add(new ServiceRoute("asterisknow", new WebServiceHostFactory(), typeof(WCF.MNepal.asterisknow)));
            RouteTable.Routes.Add(new ServiceRoute("cash", new WebServiceHostFactory(), typeof(WCF.MNepal.cash)));
            RouteTable.Routes.Add(new ServiceRoute("IVR", new WebServiceHostFactory(), typeof(WCF.MNepal.IVR)));
            RouteTable.Routes.Add(new ServiceRoute("merchant",new WebServiceHostFactory(),typeof(WCF.MNepal.merchant)));
            RouteTable.Routes.Add(new ServiceRoute("coupon", new WebServiceHostFactory(), typeof(WCF.MNepal.coupon)));
            RouteTable.Routes.Add(new ServiceRoute("remit", new WebServiceHostFactory(), typeof(WCF.MNepal.remit)));
            RouteTable.Routes.Add(new ServiceRoute("topup", new WebServiceHostFactory(), typeof(WCF.MNepal.topup)));
            RouteTable.Routes.Add(new ServiceRoute("selfReg", new WebServiceHostFactory(), typeof(WCF.MNepal.selfReg)));
            RouteTable.Routes.Add(new ServiceRoute("forgot", new WebServiceHostFactory(), typeof(WCF.MNepal.forgot)));
            RouteTable.Routes.Add(new ServiceRoute("authlogin", new WebServiceHostFactory(), typeof(WCF.MNepal.authlogin)));
            RouteTable.Routes.Add(new ServiceRoute("reset", new WebServiceHostFactory(), typeof(WCF.MNepal.reset)));
            RouteTable.Routes.Add(new ServiceRoute("requestCode", new WebServiceHostFactory(), typeof(WCF.MNepal.requestCode)));
            RouteTable.Routes.Add(new ServiceRoute("querystmt", new WebServiceHostFactory(), typeof(WCF.MNepal.querystmt)));
            RouteTable.Routes.Add(new ServiceRoute("bankinfo", new WebServiceHostFactory(), typeof(WCF.MNepal.bankinfo)));
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                //These headers are handling the "pre-flight" OPTIONS call sent by the browser
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}