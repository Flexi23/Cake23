using Cake23.Util;
using System;
using System.Collections.Generic;

namespace Cake23.Connection.Clients.Midi
{
	public class Cake23MidiClient : Cake23Client
	{
		private SanfordMidiTracker midiTracker;

		public override void Setup()
		{
			this.Log("setting up");
			midiTracker = new SanfordMidiTracker(Logger);
			midiTracker.ChannelMsg += midiTracker_ChannelMsg;
		}

		static Dictionary<string, string> midiState = new Dictionary<string, string>();

		void midiTracker_ChannelMsg(string cmd, string channel, string key, string value)
		{
			if (cmd.Equals("Controller"))
			{
				if (midiState.ContainsKey(key))
				{
					midiState[key] = value;
				}
				else
				{
					midiState.Add(key, value);
				}

				if (key == "46" && value == "127")
				{
					this.Log("send all key value pairs");
					foreach (string k in midiState.Keys)
					{
						Invoke("OnMidi", channel, k, midiState[k]);
						//this.Log(String.Format("Midi channel {0}: {1} => {2}", channel, k, midiState[k]));
					}
				}
				else
				{
					Invoke("OnMidi", channel, key, value);
					//this.Log(String.Format("Midi channel {0}: {1} => {2}", channel, key, value));
				}
			}
		}

		protected override string Name
		{
			get { return "Midi"; }
		}

		public override string HubName
		{
			get { return typeof(MidiHub).Name; }
		}

		public override void Connect(object obj)
		{
			if (CanConnect(obj))
			{
				base.Connect(obj);
				midiTracker.StartListening();
			}
		}

		public override void Unconnect(object obj)
		{
			if (CanUnconnect(obj))
			{
				base.Unconnect(obj);
				midiTracker.StopListening();
			}
		}
	}
}
