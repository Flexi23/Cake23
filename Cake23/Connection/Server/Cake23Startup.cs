using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using System.IO;
using System.Reflection;
using System.Web.Http;

[assembly: OwinStartup(typeof(Cake23.Connection.Server.Cake23Startup))]

namespace Cake23.Connection.Server
{
    public class Cake23Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            app.UseErrorPage();
#endif

            var staticWebContentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\WebStaticContent";
            var physicalFileSystem = new PhysicalFileSystem(staticWebContentPath);
            var options = new FileServerOptions();

            options.EnableDefaultFiles = true;
            options.FileSystem = physicalFileSystem;
            options.EnableDirectoryBrowsing = true;

            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };

            var host = Cake23Host.GetInstance();
            if (host.AllowCORS)
                app.UseCors(CorsOptions.AllowAll);

            app.UseFileServer(options);
            app.MapSignalR();

            HttpConfiguration config = new HttpConfiguration();
            var hostName = Cake23Host.GetInstance().Connection.UserName;
			config.Routes.MapHttpRoute("Templates", "client/{templateName}", new { controller = "Template", templateName = "index", userName = hostName });
            config.Routes.MapHttpRoute("Remoteria", "remote/{templateName}/{userName}", new { controller = "Remote", templateName = "index", userName = hostName });
			app.UseWebApi(config);

        }

    }
}
