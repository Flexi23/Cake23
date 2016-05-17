using Cake23.Connection.Server;
using Cake23.Util;
using System;

namespace Cake23.Connection.Server
{
	public class Cake23Host : IHasLogger
	{
		public string ClientName { get { return "Self-Host"; } }

		private Logger _logger;
		public Logger Logger
		{
			get { return _logger; }
			set
			{
				_logger = value;
				_connection.Logger = _logger;
			}
		}

		private Cake23HostConnection _connection;
		public Cake23HostConnection Connection
		{
			get { return _connection; }
		}

		private static Cake23Host _singleton;

		public static Cake23Host GetInstance()
		{
			if (_singleton == null)
				_singleton = new Cake23Host();

			return _singleton;
		}

		private IDisposable owin;

		private Cake23Host()
		{
			URL = "";
			_connection = new Cake23HostConnection();
		}

		public bool IsListening
		{
			get { return owin != null; }
		}

		public void StartListening(string hostname, int port = 9000)
		{
			if (string.IsNullOrWhiteSpace(hostname))
			{
				hostname = "localhost";
			}

			UpdateURL(hostname, port);

			if (owin == null)
			{
				try
				{
					owin = Microsoft.Owin.Hosting.WebApp.Start<Cake23Startup>(URL);
					this.Log("server started: " + URL);
					_connection.Connect();
				}
				catch (Exception x)
				{
					this.Log("couldn't start: " + x);
				}
			}
		}

		public void UpdateURL(string hostname, int port)
		{
			URL = "http://" + hostname + ":" + port;
		}

		public void Stop()
		{
			if (owin != null)
			{
				_connection.Unconnect();
				owin.Dispose();
				owin = null;
				this.Log("server stopped");
			}
		}

		public string URL { get; private set; }
	}
}
