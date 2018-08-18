using System;
using System.IO;

namespace CAVS.Anvel.Lidar
{
    public static class LidarSerialization
    {

        public static LidarData Load(string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                int points = (int)((fileStream.Length - 280) / 32);
                DataPoint[] dataPoints = new DataPoint[points];

                byte[] header = new byte[280];
                byte[] body = new byte[points*32];

                fileStream.Read(header, 0, header.Length);
                fileStream.Read(body, 0, body.Length);

                // Data Section
                // 1 to N Blocks of 32 bytes per point sample.
                //      double x; //X coordinate in world position
                //      double y; //Y coordinate in world position
                //      double z; //Z coordinate in world position
                //      double DiscoveryTime; //Simulation time point was captured

                int offset = 0;
                for(int i = 0; i < points; i ++)
                {
                    dataPoints[i] = new DataPoint(
                        new UnityEngine.Vector3(
                            (float)BitConverter.ToDouble(body, offset+ 8),
                            (float)BitConverter.ToDouble(body, offset + 16),
                            (float)BitConverter.ToDouble(body, offset)
                        ),
                        (float)BitConverter.ToDouble(body, offset + 24)
                    );
                    offset += 32;
                }

				// https://wiki.anvelsim.com/3/index.php/Point_Cloud_Replay
				// Header Block: 280 bytes
				// 		uint32 PCRP signature byte = 0xf16e948b
				// 		uint32 Reserved;   //Unused by PCRP
				// 		uint64 DataOffset; //Unused by PCRP
				// 		uint32 DataSize;   //Unused by PCRP
				// 		uint32 Version;     //Unused by PCRP
				// 		char[128] SensorName; //Unique sensor instance name
				// 		char[128] AssetName;  //Name of asset. '360 Lidar', 'API Lidar', etc.
                return new LidarData(
                    BitConverter.ToUInt32(header, 0),
                    BitConverter.ToUInt32(header, 4),
                    BitConverter.ToUInt64(header, 8),
                    BitConverter.ToUInt32(header, 16),
                    BitConverter.ToUInt32(header, 20),
                    System.Text.Encoding.UTF8.GetString(header, 24, 128),
                    System.Text.Encoding.UTF8.GetString(header, 152, 128),
                    dataPoints
                );
            }
        }

    }

}
