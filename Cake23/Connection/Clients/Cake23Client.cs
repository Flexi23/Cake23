using Cake23.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cake23.Connection.Clients
{
    public abstract class Cake23Client : Cake23Connection
	{
		protected abstract string Name { get; }

		public override sealed string ClientName
		{
			get { return Name + " Client"; }
		}

		public abstract void Setup();

		private static IEnumerable<Type> clientTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsClass && !type.IsAbstract && typeof(Cake23Client).IsAssignableFrom(type));

		public static List<string> AvailableTypeNames
		{
			get { return clientTypes.Select(type => type.Name).ToList(); }
		}

		public static Cake23Client Create(string userName, string typeName, Logger logger = null)
		{
			if (typeName != null)
			{
				var clientType = clientTypes.FirstOrDefault(type => type.Name.ToLower().Equals(typeName.ToLower()));
				if (clientType != null)
				{
					var client = Activator.CreateInstance(clientType) as Cake23Client;
					if (client != null)
					{
						if (logger != null)
						{
							client.Logger = logger;
						}
						client.UserName = userName;
						client.Setup();
						return client;
					}
				}
				else
				{
					if (logger != null)
					{
						logger.Log(typeName + " is not available");
					}
				}
			}
			return null;
		}
	}

	public class UserClient
	{
		public string UserName;
		public string ClientName;
	}
}
