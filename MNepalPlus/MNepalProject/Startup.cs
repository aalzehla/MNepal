using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MNepalProject.Startup))]
namespace MNepalProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
