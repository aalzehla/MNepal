using CustApp.App_Start;
using System.Web;
using System.Web.Mvc;

namespace CustApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new ValidateAntiForgeryTokenOnPost());
        }
    }
}
