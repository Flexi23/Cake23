using Cake23.Connection;
using Cake23.Connection.Clients;
using Cake23.Connection.Server;
using Cake23.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Input;

namespace Cake23
{
	public delegate void SetFaceTracking(bool active);

	public class Cake23Application : ViewModelBase, IHasLogger
	{
		public event SetFaceTracking FaceTrackingChanged;
		public Logger Logger { get; set; }
		public string ClientName
		{
			get { return ""; }
		}

		private string _title;
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				OnPropertyChanged(() => Title);
			}
		}

		private Cake23Host host = Cake23Host.GetInstance();
		private List<Cake23Connection> connections = new List<Cake23Connection>();
		private string _remoteURL = "";

		public bool IsHosting
		{
			get { return host.IsListening; }
		}

		public bool IsConnected
		{
			get { return connections.Any(c => c.IsConnected); }
		}

		public bool CanEditHostname
		{
			get { return !IsHosting && !IsConnected; }
		}

		private string _hostname;
		public string Hostname
		{
			get { return _hostname; }
			set
			{
				_hostname = value;
				Cake23Host.GetInstance().UpdateURL(Hostname, Port);
				OnPropertyChanged(() => Hostname);
			}
		}

		private string _username;
		public string UserName
		{
			get { return _username; }
			set
			{
				Cake23Host.GetInstance().Connection.UserName = _username = value;
				OnPropertyChanged(() => UserName);
			}
		}

		public bool CanEditUsername
		{
			get { return !IsHosting; }
		}

		private int _port;
		public int Port
		{
			get { return _port; }
			set
			{
				_port = value;
				Cake23Host.GetInstance().UpdateURL(Hostname, Port);
				OnPropertyChanged(() => Port);
			}
		}

		private string _log;
		public string Log
		{
			get { return _log; }
			set
			{
				_log = value;
				OnPropertyChanged(() => Log);
			}
		}

		private bool _allowCORS = false;
		public bool AllowCORS
		{
			get { return _allowCORS; }
			set
			{
				if (_allowCORS != value)
				{
					_allowCORS = value;
					OnPropertyChanged(() => AllowCORS);
					host.AllowCORS = AllowCORS;
				}
			}
		}

		private bool _faceTracking = false;
		public bool FaceTracking
		{
			get { return _faceTracking; }
			set
			{
				if (_faceTracking != value)
				{
					_faceTracking = value;
					OnPropertyChanged(() => FaceTracking);
					FaceTrackingChanged?.Invoke(value);
				}
			}
		}

		private ICommand _connectCmd;
		public ICommand ConnectCommand
		{
			get
			{
				if (_connectCmd == null)
				{
					_connectCmd = new DelegateCommand(ConnectAll, CanConnect);
				}
				return _connectCmd;
			}

			private set { _connectCmd = value; }
		}

		private bool CanConnect(object arg)
		{
			return connections.All(c => c.CanConnect(arg));
		}

		private void ConnectAll(object obj)
		{
			if (!host.IsListening)
			{
				// in case we're not self-hosting, we must first establish the "say hi" connection.
				host.Connection.Connect();
			}

			if (CanConnect(obj))
			{
				_remoteURL = "http://" + Hostname + ":" + Port;

				OnPropertyChanged(() => CanEditHostname);
				connections.ForEach(c =>
				{
					c.UserName = UserName;
					c.Connect(obj);
				});
			}
		}

		private ICommand _hostCmd;
		public ICommand HostCommand
		{
			get
			{
				if (_hostCmd == null)
				{
					_hostCmd = new DelegateCommand(Host, CanHost);
				}
				return _hostCmd;
			}
			private set { _hostCmd = value; }
		}

		private bool CanHost(object arg)
		{
			return !IsHosting;
		}

		private void Host(object obj)
		{
			if (!host.IsListening)
			{
				host.StartListening(Hostname);
				OnPropertyChanged(() => IsHosting);
				OnPropertyChanged(() => CanEditHostname);
			}
		}

		private ICommand _stopCmd;
		public ICommand StopCommand
		{
			get
			{
				if (_stopCmd == null)
				{
					_stopCmd = new DelegateCommand(Stop, CanStop);
				}
				return _stopCmd;
			}

			private set { _stopCmd = value; }
		}

		private bool CanStop(object arg)
		{
			return IsHosting || IsConnected;
		}

		private void Stop(object obj)
		{
			if (IsConnected)
			{
				_remoteURL = "";
				connections.ForEach(c => c.Unconnect(obj));
			}

			if (IsHosting)
			{
				host.Stop();
			}

			OnPropertyChanged(() => IsHosting);
			OnPropertyChanged(() => CanEditHostname);
		}

		private ICommand _getPublicIPCmd;
		public ICommand GetPublicIPCommand
		{
			get
			{
				if (_getPublicIPCmd == null)
				{
					_getPublicIPCmd = new DelegateCommand(GetPublicIP);
				}
				return _getPublicIPCmd;
			}
			private set { _getPublicIPCmd = value; }
		}

		private void GetPublicIP(object obj)
		{
			var getPublicIP = new BackgroundWorker();
			getPublicIP.DoWork += getPublicIP_DoWork;
			getPublicIP.RunWorkerAsync();
		}

		public string RequestPublicIP()
		{
			var request = WebRequest.Create("http://checkip.dyndns.org/");
			var response = request.GetResponse();
			var stream = new StreamReader(response.GetResponseStream());
			var direction = stream.ReadToEnd();
			stream.Close(); response.Close();
			int first = direction.IndexOf("Address: ") + 9;
			int last = direction.IndexOf("</body></html>");
			return direction.Substring(first, last - first);
		}

		void getPublicIP_DoWork(object sender, DoWorkEventArgs e)
		{
			this.Log("according to dyndns.org, your public IP is " + RequestPublicIP());
		}

		public Cake23Application()
		{
			host.Logger = this.Logger = new Logger();
			Title = "Cake23 | WebSocket Hub for Kinect v2 and MIDI controllers";
			Hostname = Dns.GetHostName();
			Port = 9000;
			UserName = Environment.UserName + "@" + Hostname;
		}

		internal void Setup()
		{
			foreach (var clientTypeName in Cake23Client.AvailableTypeNames)
			{
				var client = Cake23Client.Create(this, UserName, clientTypeName, Logger);
				if (client != null)
				{
					connections.Add(client);
				}
			}
		}

        // idea: list available client types to start and stop on demand
	}
}
