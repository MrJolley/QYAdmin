using Microsoft.Owin;
using Owin;
using QY.Admin.Models;

[assembly: OwinStartupAttribute(typeof(QY.Admin.Startup))]
namespace QY.Admin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
