using System.Collections.Generic;

namespace Cake23.Util
{
	public delegate void Msg(string msg);

	public class Logger
	{
		public event Msg Msg;
		public IReadOnlyList<string> History
		{
			get
			{
				return _history.AsReadOnly();
			}
		}

		private List<string> _history;

		internal Logger()
		{
			_history = new List<string>();
		}

		internal void Log(string msg)
		{
			if (string.IsNullOrWhiteSpace(msg))
				return;

			_history.Add(msg);

			if (Msg != null)
				Msg(msg);
		}
	}

	public interface IHasLogger
	{
		string ClientName { get; }
		Logger Logger { get; }
	}

	public static class LoggerExtension
	{
		public static void Log(this IHasLogger cake23, string msg)
		{
			if (string.IsNullOrWhiteSpace(cake23.ClientName))
			{
				cake23.Logger.Log(msg);
			}
			else
			{
				cake23.Logger.Log("[" + cake23.ClientName + "] " + msg);
			}
		}
	}
}
