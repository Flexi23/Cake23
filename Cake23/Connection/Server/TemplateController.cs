using RazorEngine;
using RazorEngine.Templating;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace Cake23.Connection.Server
{
	public class TemplateController : ApiController
	{
		public HttpResponseMessage Get(string templateName)
		{
			try
			{

				var templatesPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\WebTemplates";
				var path = templatesPath + "\\" + templateName + ".cshtml";
				if (File.Exists(path))
				{
					var response = new HttpResponseMessage(HttpStatusCode.OK);
					var template = File.ReadAllText(path);
					var result = Engine.Razor.RunCompile(template, templateName, null, new { Host = Cake23Host.GetInstance() });
					response.Content = new StringContent(result, System.Text.Encoding.UTF8, "text/html");
					return response;
				}
				else
				{
					// TODO: perhaps list all available templates
					// Directory.GetFiles(templatesPath, "*.cshtml").Select(filepath => Path.GetFileNameWithoutExtension(filepath)); 
					return new HttpResponseMessage(HttpStatusCode.NotFound);
				}
			}
			catch (Exception e)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}
		}
	}
}