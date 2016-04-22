using Microsoft.AspNet.SignalR;

namespace Cake23.Connection.Clients.Kinect2
{
	public class Kinect2Hub : Hub
	{
		public void OnBody(string bodyJson, string projectionMappedPointsJson)
		{
			Clients.All.onBody(bodyJson, projectionMappedPointsJson);
		}

		public void OnBodies(string trackedBodyTrackingIdsJson, long frame)
		{
			Clients.All.onBodies(trackedBodyTrackingIdsJson, frame);
		}

		public void OnFace(string verticesJSON, string status, ulong TrackingId)
		{
			Clients.All.onFace(verticesJSON, status, TrackingId);
		}
	}
}
