using Cake23.Util;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using System;

namespace Cake23.Connection.Clients.Midi
{
	public delegate void ChannelMsg(string cmd, string channel, string data1, string data2);

	public class SanfordMidiTracker : IHasLogger
	{
		public string ClientName { get { return "Sanford.Multimedia.Midi"; } }
		public Logger Logger {get; set;}

		private InputDevice inDevice = null;
		public event ChannelMsg ChannelMsg;

		public SanfordMidiTracker(Logger logger = null)
		{
			if (logger == null)
			{
				logger = new Logger();
			}

			this.Logger = logger;

			this.Log("Number of Midi devices: " + InputDevice.DeviceCount);

			if (InputDevice.DeviceCount > 0)
			{
				try
				{
					inDevice = new InputDevice(0);
					inDevice.ChannelMessageReceived += HandleChannelMessageReceived;
					inDevice.Error += new EventHandler<ErrorEventArgs>(inDevice_Error);
				}
				catch (Exception ex)
				{
					this.Log("Exception: " + ex.Message);
				}
			}
		}

		public void StartListening()
		{
			if (inDevice != null)
			{
				inDevice.StartRecording();
			}
		}

		public void StopListening()
		{
			if (inDevice != null)
			{
				inDevice.StopRecording();
				inDevice.Reset();
			}
		}

		private void inDevice_Error(object sender, ErrorEventArgs e)
		{
			this.Log("Device error: " + e.Error.Message);
		}

		private void HandleChannelMessageReceived(object sender, ChannelMessageEventArgs e)
		{
			if (ChannelMsg != null)
			{
				ChannelMsg(
					e.Message.Command.ToString(),
					e.Message.MidiChannel.ToString(),
					e.Message.Data1.ToString(),
					e.Message.Data2.ToString()
					);
			}
		}
	}
}
