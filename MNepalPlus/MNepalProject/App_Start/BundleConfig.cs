using System.Web.Optimization;

namespace MNepalProject
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"                  
                      ));


            //Require Js and Style bundle for ClipOne
            bundles.Add(new ScriptBundle("~/ClipOne").Include(

                //"~/Scripts/ClipOneJS/*.js" ,
                 "~/Scripts/ClipOneJS/jquery-ui-1.10.2.custom.min.js",
                 "~/Scripts/ClipOneJS/bootstrap.min.js",
                 "~/Scripts/ClipOneJS/perfect-scrollbar.js",
                "~/Scripts/ClipOneJS/main.js",
                "~/Scripts/ClipOneJS/jquery.blockUI.js",
                "~/Scripts/ClipOneJS/jquery.icheck.min.js",
                "~/Scripts/ClipOneJS/jquery.mousewheel.js"
                ));

            bundles.Add(new StyleBundle("~/Content/ClipOneCss/css").Include(

                "~/Content/ClipOneCss/bootstrap.min.css",
                "~/Content/ClipOneCss/font-awesome.min.css",
                "~/Content/ClipOneCss/style.css",
                 "~/Content/ClipOneCss/main.css",
                "~/Content/ClipOneCss/main-responsive.css",
                "~/Content/ClipOneCss/all.css",
                "~/Content/ClipOneCss/perfect-scrollbar.css",
                "~/Content/ClipOneCss/theme_light.css",
                "~/Content/ClipOneCss/CustomCss.css"
                ));

            //Script for Angular
            bundles.Add(new ScriptBundle("~/angjs").Include(
                "~/Scripts/Angular/angular.min.js",
                "~/Scripts/Angular/angular-route.min.js",
                "~/Scripts/Angular/angular-resource.min.js",
                "~/Scripts/Angular/angular-cookies.min.js",

                "~/Scripts/Angular/modules/module.js",
                "~/Scripts/Angular/filter/pagination.js",
                 //"~/Scripts/Angular/app.js",

                "~/Scripts/Angular/services/service.js",
                "~/Scripts/Angular/services/account/accountServices.js",
                "~/Scripts/Angular/services/Service.js",
                "~/Scripts/Angular/services/TransactionService.js",
                "~/Scripts/Angular/services/dashboardService.js",

                "~/Scripts/Angular/controller/controller.js",
                "~/Scripts/Angular/controller/account/accountController.js",
                "~/Scripts/Angular/controller/TransactionController.js",
                 "~/Scripts/Angular/controller/dashboardController.js",


                 //directive
                 "~/Scripts/Angular/directive/balanceDirective.js"
                ));
            
    

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}
