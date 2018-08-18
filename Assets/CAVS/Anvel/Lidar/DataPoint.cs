using UnityEngine;

namespace CAVS.Anvel.Lidar
{
    public class DataPoint
    {
        
        public Vector3 Point { get; }

        public float TimeStamp { get; }

        public DataPoint (Vector3 point, float timestamp) {
            this.Point = point;
            this.TimeStamp = timestamp;
        }

    }

}