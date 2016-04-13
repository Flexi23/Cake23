using Cake23.Connection.Clients;
using Cake23.Util;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake23.Connection.Server
{
	public class Cake23HostConnection : Cake23Connection
	{
		public override string ClientName
		{
			get { return "Cake23"; }
		}

		public override string HubName
		{
			get { return typeof(Cake23Hub).Name; }
		}

		public override void Connect(object obj = null)
		{
			if (CanConnect())
			{
				base.Connect(obj);
				On<UserClient>("Register", OnRegister);
			}
		}

		public override void Unconnect(object obj = null)
		{
			if (CanUnconnect())
			{
				base.Unconnect(obj);
			}
		}

		private void OnRegister(UserClient uc)
		{
			if (!userClientsMap.ContainsKey(uc.UserName))
			{
				userClientsMap.Add(uc.UserName, new List<string>());
				this.Log("username " + uc.UserName + " registered");
			}
			userClientsMap[uc.UserName].Add(uc.ClientName);
			this.Log(uc.ClientName + " registered for user " + uc.UserName);
		}

		private Dictionary<string, List<string>> userClientsMap = new Dictionary<string, List<string>>();

		internal void Register(UserClient uc)
		{
			Invoke("Register", uc);
		}
	}
}
