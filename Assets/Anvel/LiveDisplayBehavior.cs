using AnvelApi;

using System.Threading;

using UnityEngine;


namespace CAVS.Anvel
{

    public class LiveDisplayBehavior : MonoBehaviour
    {
        private ParticleSystem lidarDisplay;

        private ParticleSystem.Particle[] particles;

        private AnvelControlService.Client anvelConnection;

        private Thread pollingThread;

        private string lidarSensorName;

        private string vehicleName;

        private Vector3 centerOffset;

        private Vector3 rotationOffset;

        public void Initialize(AnvelControlService.Client anvelConnection, string lidarSensorName, string vehicleName)
        {
            this.lidarSensorName = lidarSensorName;
            this.vehicleName = vehicleName;
            this.anvelConnection = anvelConnection;
            this.centerOffset = Vector3.zero;
            this.rotationOffset = Vector3.zero;
            particles = new ParticleSystem.Particle[0];
            lidarDisplay = gameObject.GetComponent<ParticleSystem>();
            pollingThread = new Thread(PollLidarPoints);
            pollingThread.Start();
        }

        public void Initialize(AnvelControlService.Client anvelConnection, string lidarSensorName, string vehicleName, Vector3 centerOffset, Vector3 rotationOffset)
        {
            this.lidarSensorName = lidarSensorName;
            this.vehicleName = vehicleName;
            this.anvelConnection = anvelConnection;
            this.centerOffset = centerOffset;
            this.rotationOffset = rotationOffset;
            particles = new ParticleSystem.Particle[0];
            lidarDisplay = gameObject.GetComponent<ParticleSystem>();
            pollingThread = new Thread(PollLidarPoints);
            pollingThread.Start();
        }

        public void UpdateCenterOffset(Vector3 newOffset)
        {
            centerOffset = newOffset;
        }

        public void UpdateRotationOffset(Vector3 newOffset)
        {
            rotationOffset = newOffset;
        }

        void Update()
        {
            var toRender = particles;
            if (toRender.Length > 0)
            {
                lidarDisplay.SetParticles(toRender, toRender.Length);
            }
        }

        private void OnDestroy()
        {
            if(pollingThread != null && pollingThread.IsAlive)
            {
               pollingThread.Abort();
            }
        }

        private Vector3 ModifiedPositionFromRotationalOffset(Vector3 originalPosition, Vector3 pivot, Vector3 rotationalOffset)
        {
            return UnityEngine.Quaternion.Euler(rotationalOffset) * (originalPosition - pivot) + pivot;
        }

        /// <summary>
        /// RAN IN A SEPERATE THREAD
        /// </summary>
        private void PollLidarPoints()
        {
            try
            {
                ObjectDescriptor lidarSensor = anvelConnection.GetObjectDescriptorByName(lidarSensorName);
                ObjectDescriptor vehicle = anvelConnection.GetObjectDescriptorByName(vehicleName);
                while (true)
                {
                    var lidarPoints = anvelConnection.GetLidarPoints(lidarSensor.ObjectKey, 0);

                    Vector3 offset = Vector3.zero;
                    if(anvelConnection.GetProperty(lidarSensor.ObjectKey, "Lidar Global Frame") == "true")
                    {
                        Point3 vehiclePosition = anvelConnection.GetPoseAbs(vehicle.ObjectKey).Position;
                        offset -= new Vector3((float)vehiclePosition.Y, (float)vehiclePosition.Z, (float)vehiclePosition.X);
                    }

                    if (lidarPoints.HasScans)
                    {
                        var newParticles = new ParticleSystem.Particle[lidarPoints.Points.Count];
                        for (int i = 0; i < lidarPoints.Points.Count; i++)
                        {
                            newParticles[i] = new ParticleSystem.Particle
                            {
                                remainingLifetime = float.MaxValue,
                                position = ModifiedPositionFromRotationalOffset(new Vector3(
                                    -(float)lidarPoints.Points[i].Y,
                                    (float)lidarPoints.Points[i].Z,
                                    (float)lidarPoints.Points[i].X
                                ), Vector3.zero, rotationOffset) + centerOffset,
                                startSize = 1f,
                                startColor = Color.white
                            };
                        }
                        particles = newParticles;
                    }
                }
            }
            catch (AnvelException e)
            {
                Debug.Log(string.Format("Anvel Exception: {0} at {1}", e.ErrorMessage, e.Source));
                throw;
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
           
        }

    }

}