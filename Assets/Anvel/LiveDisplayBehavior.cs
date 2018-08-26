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

        public void Initialize(AnvelControlService.Client anvelConnection, string lidarSensorName, string vehicleName)
        {
            this.lidarSensorName = lidarSensorName;
            this.vehicleName = vehicleName;
            this.anvelConnection = anvelConnection;
            particles = new ParticleSystem.Particle[0];
            lidarDisplay = gameObject.GetComponent<ParticleSystem>();
            pollingThread = new Thread(PollLidarPoints);
            pollingThread.Start();
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
                    var vehiclePosition = anvelConnection.GetPoseAbs(vehicle.ObjectKey).Position;
                    if(lidarPoints.HasScans)
                    {
                        var newParticles = new ParticleSystem.Particle[lidarPoints.Points.Count];
                        for (int i = 0; i < lidarPoints.Points.Count; i++)
                        {
                            newParticles[i] = new ParticleSystem.Particle
                            {
                                remainingLifetime = float.MaxValue,
                                position = new Vector3(
                                    (float)(lidarPoints.Points[i].Y - vehiclePosition.Y),
                                    (float)(lidarPoints.Points[i].Z - vehiclePosition.Z),
                                    (float)(lidarPoints.Points[i].X - vehiclePosition.X)
                                ),
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