
using UnityEngine;

using CAVS.Anvel;
using CAVS.Anvel.Lidar;
using CAVS.Anvel.Vehicle;


namespace CAVS
{

    public class Test : MonoBehaviour
    {

        [SerializeField]
        private LiveDisplayBehavior.LidarEntry[] lidarSensors;

        [SerializeField]
        private string vehicleName;

        [SerializeField]
        private GameObject carRepresentation;

        [SerializeField]
        private GameObject cameraDisplay;

        LiveDisplayBehavior liveDisplayBehavior;

        // Use this for initialization
        void Start()
        {
            //var fileDisplayBehavior = gameObject.AddComponent<FileDisplayBehavior>();
            //fileDisplayBehavior.Initialize(
            //    LidarSerialization.Load("360 Lidar-11.pcrp"),
            //    VehicleLoader.LoadVehicleData("vehicle1_pos_2.vprp")
            //);

            var token = new ClientConnectionToken();

            LiveCameraDisplay.Build(cameraDisplay, token, "API Camera-1");

            liveDisplayBehavior = gameObject.AddComponent<LiveDisplayBehavior>();
            liveDisplayBehavior.Initialize(
                token,
                lidarSensors,
                vehicleName,
                Vector3.zero,
                Vector3.zero
            );


        }

        private void Update()
        {
            liveDisplayBehavior.UpdateCenterOffset(carRepresentation.transform.position);
            liveDisplayBehavior.UpdateRotationOffset(carRepresentation.transform.rotation.eulerAngles);
        }

    }

}