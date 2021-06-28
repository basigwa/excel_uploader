using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExcelUploader2.Startup))]
namespace ExcelUploader2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
