using Cake23.Connection.Clients;
using Cake23.Connection.Server;
using Cake23.Util;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cake23.Connection
{
	public abstract class Cake23Connection : ViewModelBase, IHasLogger
	{
		public Logger Logger { get; set; }

		public abstract string ClientName { get; }
		public abstract string HubName { get; }

		public string UserName { get; set; }

		private HubConnection hubConnection;
		private IHubProxy hubProxy;
		protected async Task Invoke(string method, params object[] args)
		{
			try
			{
				hubProxy.Invoke(method, args);
			}
			catch (Exception x)
			{
				this.Log(x.GetType().Name + ": " + x.Message);
				Reconnect();
			}
		}

		protected IDisposable On<T>(string eventName, Action<T> onData)
		{
			return hubProxy.On<T>(eventName, onData);
		}

		private bool _isConnected = false;
		public bool IsConnected
		{
			get { return _isConnected; }
		}

		private ICommand _connectCommand;
		private ICommand _unconnectCommand;

		public ICommand ConnectCommand
		{
			get
			{
				if (_connectCommand == null)
				{
					_connectCommand = new DelegateCommand(Connect, CanConnect);
				}
				return _connectCommand;
			}
		}

		public ICommand UnconnectCommand
		{
			get
			{
				if (_unconnectCommand == null)
				{
					_unconnectCommand = new DelegateCommand(Unconnect, CanUnconnect);
				}
				return _unconnectCommand;
			}
		}

		public virtual void Connect(object obj = null)
		{
			ConnectAsync();
		}
		
		public virtual void Unconnect(object obj = null)
		{
			if (IsConnected)
			{
				hubConnection.Stop();
			}
		}

		public void Reconnect()
		{
			this.Log("reconnect");
			Unconnect();
			Connect();
		}
		
		public bool CanConnect(object arg = null)
		{
			return !IsConnected;
		}

		public bool CanUnconnect(object arg = null)
		{
			return IsConnected;
		}

		public async Task ConnectAsync()
		{
			if (!IsConnected)
			{
				hubConnection = new HubConnection(Cake23Host.GetInstance().URL);
				hubProxy = hubConnection.CreateHubProxy(HubName);
				await hubConnection.Start().ContinueWith(task =>
				{
					if (task.IsFaulted)
					{
						this.Log("couldn't connect: " + task.Exception.GetBaseException());
					}
					else
					{
						this.Log("connected to " + Cake23Host.GetInstance().URL);
						hubConnection.Closed += hubConnection_Closed;
						hubConnection.ConnectionSlow += hubConnection_ConnectionSlow;
						hubConnection.Error += hubConnection_Error;
						hubConnection.StateChanged += hubConnection_StateChanged;
						_isConnected = true;
					}
				});
				if (_isConnected)
				{
					Cake23Host.GetInstance().Connection.Register(new UserClient() { UserName = UserName, ClientName = ClientName });
				}
			}
		}

		void hubConnection_StateChanged(StateChange sc)
		{
			//this.Log("changed from " + sc.OldState + " to " + sc.NewState);
		}

		void hubConnection_Error(Exception e)
		{
			this.Log("Error: " + e.Message);
		}

		void hubConnection_ConnectionSlow()
		{
			this.Log("is slow");
		}

		void hubConnection_Closed()
		{
			this.Log("unconnected");
			_isConnected = false;
			OnPropertyChanged(() => IsConnected);
		}
	}
}
