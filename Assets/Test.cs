using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CAVS.Anvel.Lidar;
using CAVS.Anvel.Vehicle;

namespace CAVS
{

    public class Test : MonoBehaviour
    {

		private ParticleSystem particleSystem;


		private List<TimeEntry> vehicleData;

		private List<GameObject> vehicleRender;

        DataPoint[] points;

        float startTime;

        float endTime;

        float startSlice;

        float timeSlice;


        // Use this for initialization
        void Start()
        {
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
				if(point.TimeStamp > endTime) {
					endTime = point.TimeStamp;
				}
            }
			timeSlice = .2f;

			Render();
        }

		private void Render() {
			RenderLidar();
			RenderVehicle();
		}

		public void NewStartTime(float time){
			startSlice = ((endTime - startTime) * time) + startTime;
			Render();
		}

		private void RenderLidar(){
			List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();
			foreach (var point in points)
            {
				if(point.TimeStamp > startSlice && point.TimeStamp < startSlice + timeSlice) {
					var p = new ParticleSystem.Particle();
					p.remainingLifetime = float.MaxValue;
					p.position = point.Point;
					p.startSize = 1f;
					p.startColor = Color.white;
					particles.Add(p);
				}
			}
			particleSystem.Clear();
			particleSystem.SetParticles(particles.ToArray(), particles.Count);
		}

		private void RenderVehicle() {
			
			// Clear current render data
			foreach(var render in vehicleRender) {
				Destroy(render);
			}
			vehicleRender.Clear();

			// Find Entry to Render
			TimeEntry closestEntry = null;
			float closestDistance = float.MaxValue;
			foreach(var entry in vehicleData) {
				float distance = Mathf.Abs(startSlice - entry.Time);
				if(distance < closestDistance) {
					closestDistance = distance;
					closestEntry = entry;
				}
			}

			// Render Entry
			foreach(var trans in closestEntry.Transforms) {
				var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
				obj.transform.position = trans.Position;
				obj.transform.rotation = trans.Rotation;
				vehicleRender.Add(obj);
			}

		}

    }

}