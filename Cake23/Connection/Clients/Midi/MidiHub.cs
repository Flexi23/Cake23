using Microsoft.AspNet.SignalR;

namespace Cake23.Connection.Clients.Midi
{
	public class MidiHub : Hub
	{
		public void OnMidi(string channel, string data1, string data2)
		{
			Clients.All.onMidi(channel, data1, data2);
		}
	}
}
