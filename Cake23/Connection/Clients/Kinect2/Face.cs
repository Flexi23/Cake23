using Cake23.Util;
using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Globalization;
using System.Text;

namespace Cake23.Connection.Clients.Kinect2
{
	public delegate void FaceFrameAsJSON(string verticesJSON, string status, ulong TrackingId);

	public class Face : IDisposable, IHasLogger
	{
		public string ClientName
		{
			get { return "Face Mesher"; }
		}

		public Logger Logger { get; set; }

		private HighDefinitionFaceFrameSource highDefinitionFaceFrameSource = null;
		private HighDefinitionFaceFrameReader highDefinitionFaceFrameReader = null;
		private FaceAlignment currentFaceAlignment = null;
		private FaceModel currentFaceModel = null;
		private FaceModelBuilder faceModelBuilder = null;
		private Body currentTrackedBody = null;
		private ulong currentTrackingId = 0;
		private string status = "ready";
		private CultureInfo ci = new CultureInfo("en-US", true);

		public event FaceFrameAsJSON AsJSON;

		private bool _active = false;
		public bool Active
		{
			get { return _active; }
			set
			{
				if (_active != value)
				{
					_active = value;
					this.Log(value ? "Activated" : "Deactivated");
				}
			}
		}

		public void Set(bool active)
		{
			Active = active;
		}

		public Face(KinectSensor sensor, BodyFrameReader bodyReader)
		{
			var bodySource = sensor.BodyFrameSource;
			bodyReader.FrameArrived += BodyReader_FrameArrived;

			highDefinitionFaceFrameSource = new HighDefinitionFaceFrameSource(sensor);
			highDefinitionFaceFrameSource.TrackingIdLost += HdFaceSource_TrackingIdLost;

			highDefinitionFaceFrameReader = highDefinitionFaceFrameSource.OpenReader();
			highDefinitionFaceFrameReader.FrameArrived += HdFaceReader_FrameArrived;

			currentFaceModel = new FaceModel();
			currentFaceAlignment = new FaceAlignment();

			UpdateMesh();

			faceModelBuilder = highDefinitionFaceFrameSource.OpenModelBuilder(FaceModelBuilderAttributes.None);
			faceModelBuilder.BeginFaceDataCollection();
			faceModelBuilder.CollectionCompleted += HdFaceBuilder_CollectionCompleted;
		}

		private float[] vertexData = null;

		private void UpdateMesh()
		{
			var vertices = currentFaceModel.CalculateVerticesForAlignment(currentFaceAlignment);
			var sb = new StringBuilder();
			/*
			sb.Append("[");
			for (int i = 0; i < currentFaceModel.TriangleIndices.Count; i++)
			{
				var index = currentFaceModel.TriangleIndices[i];
				sb.AppendFormat(ci, "{0}", index);
				if (i != currentFaceModel.TriangleIndices.Count - 1)
				{
					sb.Append(",");
				}
			}
			sb.Append("]");
			var indicesJson = sb.ToString();
			 * indicesJson is a static resource which i have exported here: https://gist.github.com/Flexi23/43266257d97f9df249839168390d7159
			 */
			if (vertexData == null)
			{
				vertexData = new float[vertices.Count * 3];
			}
			for (int i = 0; i < vertices.Count; i++)
			{
				var vert = vertices[i];
				vertexData[i * 3 + 0] = vert.X;
				vertexData[i * 3 + 1] = vert.Y;
				vertexData[i * 3 + 2] = vert.Z;
			}

			if (AsJSON != null)
			{
				sb = new StringBuilder();
				sb.Append("[");
				for (int i = 0; i < vertices.Count * 3; i++)
				{
					var intensity = vertexData[i];
					sb.AppendFormat(ci, "{0}", intensity);
					if (i != vertices.Count * 3 - 1)
					{
						sb.Append(",");
					}
				}
				sb.Append("]");
				AsJSON(sb.ToString(), status, currentTrackingId);
			}
		}

		private void HdFaceBuilder_CollectionCompleted(object sender, FaceModelBuilderCollectionCompletedEventArgs e)
		{
			var modelData = e.ModelData;

			currentFaceModel = modelData.ProduceFaceModel();

			faceModelBuilder.Dispose();
			faceModelBuilder = null;

			status = "Capture Complete";
			this.Log(status);
		}

		private void HdFaceSource_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
		{
			var lostTrackingID = e.TrackingId;

			if (currentTrackingId == lostTrackingID)
			{
				currentTrackingId = 0;
				currentTrackedBody = null;
				if (faceModelBuilder != null)
				{
					faceModelBuilder.Dispose();
					faceModelBuilder = null;
				}

				highDefinitionFaceFrameSource.TrackingId = 0;
			}
		}

		private void HdFaceReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
		{
			if (!_active)
				return;

			using (var frame = e.FrameReference.AcquireFrame())
			{
				// We might miss the chance to acquire the frame; it will be null if it's missed.
				// Also ignore this frame if face tracking failed.
				if (frame == null || !frame.IsFaceTracked)
				{
					return;
				}

				frame.GetAndRefreshFaceAlignmentResult(currentFaceAlignment);
				UpdateMesh();
			}
		}

		private static double VectorLength(CameraSpacePoint point)
		{
			var result = Math.Pow(point.X, 2) + Math.Pow(point.Y, 2) + Math.Pow(point.Z, 2);

			result = Math.Sqrt(result);

			return result;
		}

		private static Body FindClosestBody(BodyFrame bodyFrame)
		{
			Body result = null;
			double closestBodyDistance = double.MaxValue;

			Body[] bodies = new Body[bodyFrame.BodyCount];
			bodyFrame.GetAndRefreshBodyData(bodies);

			foreach (var body in bodies)
			{
				if (body.IsTracked)
				{
					var currentLocation = body.Joints[JointType.SpineBase].Position;

					var currentDistance = VectorLength(currentLocation);

					if (result == null || currentDistance < closestBodyDistance)
					{
						result = body;
						closestBodyDistance = currentDistance;
					}
				}
			}

			return result;
		}

		private static Body FindBodyWithTrackingId(BodyFrame bodyFrame, ulong trackingId)
		{
			Body result = null;

			Body[] bodies = new Body[bodyFrame.BodyCount];
			bodyFrame.GetAndRefreshBodyData(bodies);

			foreach (var body in bodies)
			{
				if (body.IsTracked)
				{
					if (body.TrackingId == trackingId)
					{
						result = body;
						break;
					}
				}
			}

			return result;
		}

		private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
		{
			CheckOnBuilderStatus();

			var frameReference = e.FrameReference;
			using (var frame = frameReference.AcquireFrame())
			{
				if (frame == null)
				{
					// We might miss the chance to acquire the frame, it will be null if it's missed
					return;
				}

				if (currentTrackedBody != null)
				{
					currentTrackedBody = FindBodyWithTrackingId(frame, currentTrackingId);

					if (currentTrackedBody != null)
					{
						return;
					}
				}

				Body selectedBody = FindClosestBody(frame);

				if (selectedBody == null)
				{
					return;
				}

				currentTrackedBody = selectedBody;
				currentTrackingId = selectedBody.TrackingId;

				highDefinitionFaceFrameSource.TrackingId = currentTrackingId;
			}
		}

		private static string GetCollectionStatusText(FaceModelBuilderCollectionStatus status)
		{
			string res = string.Empty;

			if ((status & FaceModelBuilderCollectionStatus.FrontViewFramesNeeded) != 0)
			{
				res = "FrontViewFramesNeeded";
				return res;
			}

			if ((status & FaceModelBuilderCollectionStatus.LeftViewsNeeded) != 0)
			{
				res = "LeftViewsNeeded";
				return res;
			}

			if ((status & FaceModelBuilderCollectionStatus.RightViewsNeeded) != 0)
			{
				res = "RightViewsNeeded";
				return res;
			}

			if ((status & FaceModelBuilderCollectionStatus.TiltedUpViewsNeeded) != 0)
			{
				res = "TiltedUpViewsNeeded";
				return res;
			}

			if ((status & FaceModelBuilderCollectionStatus.Complete) != 0)
			{
				res = "Complete";
				return res;
			}

			if ((status & FaceModelBuilderCollectionStatus.MoreFramesNeeded) != 0)
			{
				res = "TiltedUpViewsNeeded";
				return res;
			}

			return res;
		}

		private void CheckOnBuilderStatus()
		{
			if (faceModelBuilder == null)
			{
				return;
			}

			string newStatus = string.Empty;

			var captureStatus = faceModelBuilder.CaptureStatus;
			newStatus += captureStatus.ToString();

			var collectionStatus = faceModelBuilder.CollectionStatus;

			newStatus += ", " + GetCollectionStatusText(collectionStatus);

			status = newStatus;
			//this.Log(status);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (currentFaceModel != null)
				{
					currentFaceModel.Dispose();
					currentFaceModel = null;
				}
			}
		}
	}
}
