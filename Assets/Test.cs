using Thrift.Protocol;
using Thrift.Transport;
using AnvelApi;

using System.Collections.Generic;
using UnityEngine;

using CAVS.Anvel.Lidar;
using CAVS.Anvel.Vehicle;
using System.Threading;


namespace CAVS
{

    public class Test : MonoBehaviour
    {

        [SerializeField]
        private string lidarSensorName;

        private ParticleSystem particleSystem;


        private List<TimeEntry> vehicleData;


        private List<GameObject> vehicleRender;

        DataPoint[] points;

        float startTime;

        float endTime;

        float startSlice;

        float timeSlice;

        private AnvelControlService.Client anvelConnection;

        private ObjectDescriptor lidarSensor;

        private LidarPoints lidarPoints;

        // Use this for initialization
        void Start()
        {
            particles = new ParticleSystem.Particle[0];
            lidarPoints = null;
            anvelConnection = ConnectToAnvel();
            lidarSensor = anvelConnection.GetObjectDescriptorByName(lidarSensorName);


            // ======================= Rest of the shit =======================

            particleSystem = gameObject.GetComponent<ParticleSystem>();
            vehicleRender = new List<GameObject>();

            var lidarData = LidarSerialization.Load("360 Lidar-11.pcrp");
            points = lidarData.GetDataPoints();

            vehicleData = VehicleLoader.LoadVehicleData("vehicle1_pos_2.vprp");

            startTime = float.MaxValue;
            endTime = float.MinValue;

            foreach (var point in points)
            {
                if (point.TimeStamp < startTime)
                {
                    startTime = point.TimeStamp;
                    startSlice = point.TimeStamp;
                }
                if (point.TimeStamp > endTime)
                {
                    endTime = point.TimeStamp;
                }
            }
            timeSlice = .2f;

            Render();
            new Thread(PollLidarPoints).Start();
        }

        ParticleSystem.Particle[] particles;

        private void PollLidarPoints (){

            while (true) {
                var lidarPoints = anvelConnection.GetLidarPoints(lidarSensor.ObjectKey, 0);

                var newParticles = new ParticleSystem.Particle[lidarPoints.Points.Count];

                for (int i = 0; i < lidarPoints.Points.Count; i++)
                {
                    newParticles[i] = new ParticleSystem.Particle
                    {
                        remainingLifetime = float.MaxValue,
                        position = new Vector3(
                            (float)lidarPoints.Points[i].Y,
                            (float)lidarPoints.Points[i].Z,
                            (float)lidarPoints.Points[i].X),
                        startSize = 1f,
                        startColor = Color.white
                    };
                }
                particles = newParticles;
            }
        }

        private void Update()
        {
            var toRender = particles;
            if (toRender.Length > 0)
            {
                particleSystem.SetParticles(toRender, toRender.Length);
            }
        }


        public static AnvelControlService.Client ConnectToAnvel(string ipAddress = "127.0.0.1", int port = 9094)
        {
            var transport = new TSocket(ipAddress, port);
            var protocol = new TBinaryProtocol(transport);
            var client = new AnvelControlService.Client(protocol);

            try
            {
                transport.Open();
                transport.TcpClient.NoDelay = true;
                return client;
            }
            catch (TTransportException e)
            {
                Debug.Log(string.Format("Error: {e.Message}"));
                return null;
            }
        }

        private void Render()
        {
            RenderLidar();
            RenderVehicle();
        }

        public void NewStartTime(float time)
        {
            startSlice = ((endTime - startTime) * time) + startTime;
            Render();
        }

        private void RenderLidar()
        {
            List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
            foreach (var point in points)
            {
                if (point.TimeStamp > startSlice && point.TimeStamp < startSlice + timeSlice)
                {
                    var p = new ParticleSystem.Particle
                    {
                        remainingLifetime = float.MaxValue,
                        position = point.Point,
                        startSize = 1f,
                        startColor = Color.white
                    };
                    particles.Add(p);
                }
            }
            particleSystem.Clear();
            particleSystem.SetParticles(particles.ToArray(), particles.Count);
        }

        private void RenderVehicle()
        {
            // Clear current render data
            foreach (var render in vehicleRender)
            {
                Destroy(render);
            }
            vehicleRender.Clear();

            // Find Entry to Render
            TimeEntry closestEntry = null;
            float closestDistance = float.MaxValue;
            foreach (var entry in vehicleData)
            {
                float distance = Mathf.Abs(startSlice - entry.Time);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEntry = entry;
                }
            }

            // Render Entry
            foreach (var trans in closestEntry.Transforms)
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = trans.Position;
                obj.transform.rotation = trans.Rotation;
                vehicleRender.Add(obj);
            }

        }

    }

}