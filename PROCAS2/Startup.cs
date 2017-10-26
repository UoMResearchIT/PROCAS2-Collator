using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PROCAS2.Startup))]
namespace PROCAS2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
