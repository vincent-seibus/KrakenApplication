using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TradeWebApp.Startup))]
namespace TradeWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
