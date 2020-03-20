using System;
using System.Web;
using System.Web.Routing;
using System.ServiceModel.Activation;


namespace MNepalWCF
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new ServiceRoute("ft", new WebServiceHostFactory(), typeof(MNepalWCF.ft)));
            RouteTable.Routes.Add(new ServiceRoute("pt", new WebServiceHostFactory(), typeof(MNepalWCF.pt)));
            RouteTable.Routes.Add(new ServiceRoute("query", new WebServiceHostFactory(), typeof(MNepalWCF.query)));
            RouteTable.Routes.Add(new ServiceRoute("asterisknow", new WebServiceHostFactory(), typeof(MNepalWCF.asterisknow)));
            RouteTable.Routes.Add(new ServiceRoute("cash", new WebServiceHostFactory(), typeof(MNepalWCF.cash)));
            RouteTable.Routes.Add(new ServiceRoute("IVR", new WebServiceHostFactory(), typeof(MNepalWCF.IVR)));
            RouteTable.Routes.Add(new ServiceRoute("merchant",new WebServiceHostFactory(),typeof(MNepalWCF.merchant)));
            RouteTable.Routes.Add(new ServiceRoute("coupon", new WebServiceHostFactory(), typeof(MNepalWCF.coupon)));
            RouteTable.Routes.Add(new ServiceRoute("remit", new WebServiceHostFactory(), typeof(MNepalWCF.remit)));
            RouteTable.Routes.Add(new ServiceRoute("topup", new WebServiceHostFactory(), typeof(MNepalWCF.Topup)));
            RouteTable.Routes.Add(new ServiceRoute("balanceQueryService", new WebServiceHostFactory(), typeof(MNepalWCF.BalanceQueryService)));
            RouteTable.Routes.Add(new ServiceRoute("selfReg", new WebServiceHostFactory(), typeof(MNepalWCF.selfReg)));
            RouteTable.Routes.Add(new ServiceRoute("forgot", new WebServiceHostFactory(), typeof(MNepalWCF.forgot)));
            RouteTable.Routes.Add(new ServiceRoute("authlogin", new WebServiceHostFactory(), typeof(MNepalWCF.authlogin)));
            RouteTable.Routes.Add(new ServiceRoute("reset", new WebServiceHostFactory(), typeof(MNepalWCF.reset)));
            RouteTable.Routes.Add(new ServiceRoute("requestCode", new WebServiceHostFactory(), typeof(MNepalWCF.requestCode)));
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