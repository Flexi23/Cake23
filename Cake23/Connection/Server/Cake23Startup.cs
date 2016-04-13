using Microsoft.Owin;
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

			// considering CORS

			var staticWebContentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\WebStaticContent";
			var physicalFileSystem = new PhysicalFileSystem(staticWebContentPath);
			var options = new FileServerOptions();

			options.EnableDefaultFiles = true;
			options.FileSystem = physicalFileSystem;
			options.EnableDirectoryBrowsing = true;

			options.StaticFileOptions.FileSystem = physicalFileSystem;
			options.StaticFileOptions.ServeUnknownFileTypes = true;
			options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };

			app.UseFileServer(options);
			app.MapSignalR();

			HttpConfiguration config = new HttpConfiguration();
			config.Routes.MapHttpRoute("Templates", "client/{templateName}", new { controller = "Template", templateName = "default" });
			app.UseWebApi(config);

		}

	}
}
