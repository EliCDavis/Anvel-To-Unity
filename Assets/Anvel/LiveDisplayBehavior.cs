using AnvelApi;
using System.Threading;
using UnityEngine;

namespace CAVS.Anvel
{

    public class LiveDisplayBehavior : MonoBehaviour
    {
        [System.Serializable]
        public class LidarEntry
        {

            public string sensorName;

            public Color renderColor;

            public LidarEntry(string sensorName, Color renderColor)
            {
                this.sensorName = sensorName;
                this.renderColor = renderColor;
            }

            public LidarEntry(string sensorName)
            {
                this.sensorName = sensorName;
                this.renderColor = Color.white;
            }

        }

        private ParticleSystem lidarDisplay;

        private ParticleSystem.Particle[] particles;

        private AnvelControlService.Client anvelConnection;

        private Thread pollingThread;

        private LidarEntry[] lidarDisplays;

        private string vehicleName;

        private Vector3 centerOffset;

        private Vector3 rotationOffset;

        public void Initialize(ClientConnectionToken connectionToken, string lidarSensorName, string vehicleName)
        {
            this.lidarDisplays = new LidarEntry[] { new LidarEntry(lidarSensorName) };
            this.vehicleName = vehicleName;
            this.anvelConnection = ConnectionFactory.CreateConnection(connectionToken);
            this.centerOffset = Vector3.zero;
            this.rotationOffset = Vector3.zero;
            particles = new ParticleSystem.Particle[0];
            lidarDisplay = gameObject.GetComponent<ParticleSystem>();
            pollingThread = new Thread(PollLidarPoints);
            pollingThread.Start();
        }

        public void Initialize(ClientConnectionToken connectionToken, LidarEntry[] lidarDisplays, string vehicleName, Vector3 centerOffset, Vector3 rotationOffset)
        {
            this.lidarDisplays = lidarDisplays;
            this.vehicleName = vehicleName;
            this.anvelConnection = ConnectionFactory.CreateConnection(connectionToken);
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
                ObjectDescriptor[] lidarSensorDescriptions = new ObjectDescriptor[lidarDisplays.Length];
                for (int i = 0; i < lidarDisplays.Length; i++)
                {
                    lidarSensorDescriptions[i] = anvelConnection.GetObjectDescriptorByName(lidarDisplays[i].sensorName);
                }
                ObjectDescriptor vehicle = anvelConnection.GetObjectDescriptorByName(vehicleName);

                LidarPoints[] allPoints = new LidarPoints[lidarDisplays.Length];
                Vector3[] offsets = new Vector3[lidarDisplays.Length];
                int totalNumberOfPoints = 0;

                float lowestPoint =  float.MaxValue;
                float highestPoint = float.MinValue;
                while (true)
                {
                    Point3 vehiclePosition = anvelConnection.GetPoseAbs(vehicle.ObjectKey).Position;
                    // anvelConnection.
                    totalNumberOfPoints = 0;
                    Vector3 lastPos = Vector3.forward*1000000;
                    for (int i = 0; i < lidarDisplays.Length; i++)
                    {
                        allPoints[i] = anvelConnection.GetLidarPoints(lidarSensorDescriptions[i].ObjectKey, 0);
                        totalNumberOfPoints += allPoints[i].Points.Count;
                        if (anvelConnection.GetProperty(lidarSensorDescriptions[i].ObjectKey, "Lidar Global Frame") == "true")
                        {
                            offsets[i] = new Vector3((float)vehiclePosition.Y, (float)vehiclePosition.Z, (float)vehiclePosition.X);
                        }
                    }

                    var newParticles = new ParticleSystem.Particle[totalNumberOfPoints];
                    int particleIndex = 0;
                    for (int lidarIndex = 0; lidarIndex < lidarDisplays.Length; lidarIndex++)
                    {
                        for (int pointIndex = 0; pointIndex < allPoints[lidarIndex].Points.Count; pointIndex++)
                        {
                            Vector3 position = ModifiedPositionFromRotationalOffset(new Vector3(
                                    -(float)allPoints[lidarIndex].Points[pointIndex].Y,
                                    (float)allPoints[lidarIndex].Points[pointIndex].Z,
                                    (float)allPoints[lidarIndex].Points[pointIndex].X
                                ) - offsets[lidarIndex], Vector3.zero, rotationOffset) + centerOffset;

                            if(position.y > highestPoint) 
                            {
                                highestPoint = position.y;
                            }

                            if(position.y < lowestPoint) 
                            {
                                lowestPoint = position.y;
                            }

                            var colorToRender = lidarDisplays[lidarIndex].renderColor;
                            if(Mathf.Abs(highestPoint - lowestPoint) >0.001f){
                                float H;
                                float S;
                                float V;
                                Color.RGBToHSV(colorToRender, out H, out S, out V);
                                var p =  (position.y-lowestPoint) / (highestPoint-lowestPoint);
                                colorToRender = Color.HSVToRGB(H, p, p);
                                colorToRender.a = lidarDisplays[lidarIndex].renderColor.a; 
                            }

                            if ((position - lastPos).sqrMagnitude > .1)
                            {
                                newParticles[particleIndex] = new ParticleSystem.Particle
                                {
                                    remainingLifetime = float.MaxValue,
                                    position = position,
                                    startSize = .5f,
                                    startColor = colorToRender
                                };
                            }


                            lastPos = position;
                            particleIndex++;
                        }
                    }
                    particles = newParticles;
                }
            }
            catch (AnvelException e)
            {
                Debug.Log(string.Format("Anvel Exception: {0} at {1}", e.ErrorMessage, e.Source));
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogFormat("{0}:{1}", e.GetType(), e.Message);
                throw;
            }
           
        }

    }

}