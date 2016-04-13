using Cake23.Connection.Clients;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake23.Connection.Server
{
	public class Cake23Hub : Hub
	{
		public void Register(UserClient uc)
		{			
			Clients.All.Register(uc);
		}
	}
}
