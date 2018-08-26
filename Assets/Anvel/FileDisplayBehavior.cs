using UnityEngine;
using System.Collections.Generic;
using System.Threading;

using CAVS.Anvel.Lidar;
using CAVS.Anvel.Vehicle;
namespace CAVS.Anvel
{
    public class FileDisplayBehavior : MonoBehaviour
    {

        private ParticleSystem lidarDisplay;

        private ParticleSystem.Particle[] particles;

        private List<GameObject> vehicleRender;

        private TimeEntry[] vehicleData;

        private LidarData lidarData;

        private float startSlice;

        private float timeSlice;

        private float timeForThreadToSleep;

        public void Initialize(LidarData lidarData, TimeEntry[] vehicleData)
        {
            this.vehicleData = vehicleData;
            this.lidarData = lidarData;

            particles = new ParticleSystem.Particle[0];
            lidarDisplay = gameObject.GetComponent<ParticleSystem>();
            vehicleRender = new List<GameObject>();
            
            timeSlice = .2f;

            Render();
            StartCoroutine(Animate());
        }

        private System.Collections.IEnumerator Animate()
        {
            var t = new Thread(LoadingLidarPointsThread);
            t.Start();
            while (startSlice < lidarData.GetEndTime())
            {
                startSlice += Time.deltaTime;
                timeForThreadToSleep = Time.deltaTime;
                Render();
                yield return null;
            }
            t.Abort();
        }

        private void LoadingLidarPointsThread()
        {
            while(true)
            {
                List<ParticleSystem.Particle> newParticles = new List<ParticleSystem.Particle>();

                TimeEntry nearestVehicleEntryNearTime = vehicleData[0];
                float bestDistanceSoFar = Mathf.Abs(vehicleData[0].Time - startSlice);
                foreach(var entry in vehicleData)
                {
                    var curdist = Mathf.Abs(entry.Time - startSlice);
                    if (curdist < bestDistanceSoFar)
                    {
                        bestDistanceSoFar = curdist;
                        nearestVehicleEntryNearTime = entry;
                    }
                }

                foreach (var point in lidarData.GetDataPoints())
                {
                    if (point.TimeStamp > startSlice && point.TimeStamp < startSlice + timeSlice)
                    {
                        newParticles.Add(new ParticleSystem.Particle
                        {
                            remainingLifetime = float.MaxValue,
                            position = point.Point - nearestVehicleEntryNearTime.Transforms[0].Position,
                            startSize = 1f,
                            startColor = Color.white
                        });
                    }
                }
                particles = newParticles.ToArray();
                Thread.Sleep(Mathf.RoundToInt(timeForThreadToSleep * 1000));
            }
        }

        private void RenderLidar()
        {
            var p = particles;
            if (p.Length > 0)
            {
                lidarDisplay.SetParticles(particles, particles.Length);
            }
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

        private void Render()
        {
            RenderLidar();
            //RenderVehicle();
        }

        public void NewStartTime(float percentThrough)
        {
            startSlice = (lidarData.GetDuration() * Mathf.Clamp01(percentThrough)) + lidarData.GetStartTime();
            Render();
        }
    }

}