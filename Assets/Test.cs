
using UnityEngine;

using CAVS.Anvel;
using CAVS.Anvel.Lidar;
using CAVS.Anvel.Vehicle;


namespace CAVS
{

    public class Test : MonoBehaviour
    {

        [SerializeField]
        private string lidarSensorName;

        [SerializeField]
        private string vehicleName;

        [SerializeField]
        private GameObject carRepresentation;

        LiveDisplayBehavior liveDisplayBehavior;

        // Use this for initialization
        void Start()
        {
            //var fileDisplayBehavior = gameObject.AddComponent<FileDisplayBehavior>();
            //fileDisplayBehavior.Initialize(
            //    LidarSerialization.Load("360 Lidar-11.pcrp"),
            //    VehicleLoader.LoadVehicleData("vehicle1_pos_2.vprp")
            //);

            liveDisplayBehavior = gameObject.AddComponent<LiveDisplayBehavior>();
            liveDisplayBehavior.Initialize(
                ConnectionFactory.CreateConnection(),
                lidarSensorName,
                vehicleName
            );

        }

        private void Update()
        {
            liveDisplayBehavior.UpdateCenterOffset(carRepresentation.transform.position);
            liveDisplayBehavior.UpdateRotationOffset(carRepresentation.transform.rotation.eulerAngles);
        }

    }

}