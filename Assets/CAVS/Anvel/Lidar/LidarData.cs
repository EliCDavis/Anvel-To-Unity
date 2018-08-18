using System;

namespace CAVS.Anvel.Lidar
{

    public class LidarData {

        uint signature;

        uint reserved;
        
        ulong dataOffset;
        
        uint dataSize;

        uint version;

        string sensorName;

        string assetName;

        DataPoint[] dataPoints;

        public LidarData(uint signature, uint reserved, ulong dataOffset, uint dataSize, uint version, string sensorName, string assetName, DataPoint[] dataPoints) {
            this.signature = signature;
            this.reserved = reserved;
            this.dataOffset = dataOffset;
            this.dataSize = dataSize;
            this.version = version;
            this.sensorName = sensorName;
            this.assetName = assetName;
            this.dataPoints = dataPoints;
        }

        public string GetSensorName(){
            return this.sensorName;
        }

        public string GetAssetName() {
            return this.assetName;
        }

        public DataPoint[] GetDataPoints(){
            return this.dataPoints;
        }

        public override string ToString() {
            return string.Format("Sensor({0}) Asset({1});", sensorName, assetName);
        }

    }

}
