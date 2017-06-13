using Cake23.Util;
using Microsoft.Kinect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cake23.Connection.Clients.Kinect2
{
	public class Cake23Kinect2Client : Cake23Client
	{

		protected override string Name
		{
			get { return "Kinect v2 Sensor"; }
		}

		public override string HubName
		{
			get { return typeof(Kinect2Hub).Name; }
		}

		private KinectSensor kinectSensor = null;
		private CoordinateMapper coordinateMapper = null;
		private BodyFrameReader bodyFrameReader = null;
		private Body[] bodies = null;

		private const float InferredZPositionClamp = 0.1f;

		private Face faceTracker = null;

		private Cake23Application cake23;

		public override void Setup(Cake23Application cake23)
		{
			this.cake23 = cake23;
			kinectSensor = KinectSensor.GetDefault();
			coordinateMapper = kinectSensor.CoordinateMapper;
			bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
			kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;
			faceTracker = new Face(kinectSensor, bodyFrameReader);
			faceTracker.AsJSON += faceJSON;
			cake23.FaceTrackingChanged += faceTracker.Set;
			kinectSensor.Open();
			this.Log("open");
			faceTracker.Logger = this.Logger;
		}

		public override void Connect(object obj = null)
		{
			if (CanConnect(obj))
			{
				base.Connect(obj);

				if (bodyFrameReader != null)
				{
					bodyFrameReader.FrameArrived += Reader_FrameArrived;
				}
			}
		}

		private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
		{
			this.Log(kinectSensor.IsAvailable ? "up and running" : "not available");
		}

		void faceJSON(string verticesJSON, string status, ulong TrackingId)
		{
			//this.Log("face vertex json string length: " + verticesJSON.Length + " (" + status + ")");
			Invoke("OnFace", verticesJSON, status, TrackingId);
		}

		private long frame = 0;
		private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
		{
			bool dataReceived = false;

			using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
			{
				if (bodyFrame != null)
				{
					if (bodies == null)
					{
						bodies = new Body[bodyFrame.BodyCount];
					}
					bodyFrame.GetAndRefreshBodyData(bodies);
					dataReceived = true;
					frame++;
				}
			}

			if (dataReceived)
			{
				foreach (Body body in bodies)
				{
					IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

					Dictionary<JointType, Array> jointPoints = new Dictionary<JointType, Array>();

					foreach (JointType jointType in joints.Keys)
					{
						CameraSpacePoint position = joints[jointType].Position;
						if (position.Z < 0)
						{
							position.Z = InferredZPositionClamp;
						}

						DepthSpacePoint depthSpacePoint = coordinateMapper.MapCameraPointToDepthSpace(position);
						jointPoints[jointType] = new float[] { depthSpacePoint.X, depthSpacePoint.Y };
					}

					if (IsConnected)
					{
						var bodyJson = JsonConvert.SerializeObject(body);
						var projectionMappedPointsJson = "";
						if (body.IsTracked)
						{
							projectionMappedPointsJson = JsonConvert.SerializeObject(jointPoints);
							Invoke("OnBody", bodyJson, projectionMappedPointsJson);
						}
					}
				}
				var trackedBodyTrackingIdsJson = JsonConvert.SerializeObject(bodies.Where(b => b.IsTracked).Select(b => b.TrackingId));
				Invoke("OnBodies", trackedBodyTrackingIdsJson, frame);
			}
		}

	}
}
