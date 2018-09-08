using UnityEngine;
namespace CAVS.Anvel.Lidar
{

    public class LidarData {

        readonly uint signature;

        readonly uint reserved;

        readonly ulong dataOffset;

        readonly uint dataSize;

        readonly uint version;

        readonly string sensorName;

        readonly string assetName;

        readonly private float startTime;

        readonly private float endTime;

        readonly DataPoint[] dataPoints;

        public LidarData(uint signature, uint reserved, ulong dataOffset, uint dataSize, uint version, string sensorName, string assetName, DataPoint[] dataPoints) {
            this.signature = signature;
            this.reserved = reserved;
            this.dataOffset = dataOffset;
            this.dataSize = dataSize;
            this.version = version;
            this.sensorName = sensorName;
            this.assetName = assetName;
            this.dataPoints = dataPoints;

            startTime = float.MaxValue;
            endTime = float.MinValue;

            foreach (var point in dataPoints)
            {
                if (point.TimeStamp < startTime)
                {
                    startTime = point.TimeStamp;
                }
                if (point.TimeStamp > endTime)
                {
                    endTime = point.TimeStamp;
                }
            }
        }

        public float GetStartTime()
        {
            return startTime;
        }

        public float GetEndTime()
        {
            return endTime;
        }

        public float GetDuration()
        {
            return endTime - startTime;
        }

        public string GetSensorName(){
            return sensorName;
        }

        public string GetAssetName() {
            return assetName;
        }

        public uint GetSignature()
        {
            return signature;
        }

        public uint GetReserved()
        {
            return reserved;
        }

        public ulong GetDataOffset()
        {
            return dataOffset;
        }

        public uint GetDataSize()
        {
            return dataSize;
        }

        public uint GetVersion()
        {
            return version;
        }

        public DataPoint[] GetDataPoints(){
            return dataPoints;
        }

        public override string ToString() {
            return string.Format("Sensor({0}) Asset({1});", sensorName, assetName);
        }

    }

}
