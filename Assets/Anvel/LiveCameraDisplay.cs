using AnvelApi;
using System.Threading;
using UnityEngine;

namespace CAVS.Anvel
{

    public class LiveCameraDisplay : MonoBehaviour
    {
        private Texture2D displayTexture;

        private ObjectDescriptor anvelCamera;

        private AnvelControlService.Client client;

        private Color32[] decodedAnvelFrameData;

        private Thread pollingThread;

        public static LiveCameraDisplay Build(GameObject parent, ClientConnectionToken connectionToken, string cameraName)
        {
			LiveCameraDisplay display = parent.AddComponent<LiveCameraDisplay>();
            display.client = ConnectionFactory.CreateConnection(connectionToken);
            display.anvelCamera = display.client.GetObjectDescriptorByName(cameraName);
			return display;
        }

		void Start() {
			
            displayTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);
            GetComponent<Renderer>().material.mainTexture = displayTexture;

            pollingThread = new Thread(PollCameraThread);
            pollingThread.Start();
		}

        private void Update()
        {
            if (anvelCamera == null)
            {
                return;
            }

            if (decodedAnvelFrameData != null)
            {
                displayTexture.SetPixels32(decodedAnvelFrameData);
                displayTexture.Apply();
            }
        }

        private void OnDestroy()
        {
            if (pollingThread != null && pollingThread.IsAlive)
            {
                pollingThread.Abort();
            }
        }

        private void PollCameraThread()
        {
            while (true)
            {
                Image cameraImage = client.GetCameraFrame(anvelCamera.ObjectKey, 0);

                if (!cameraImage.HasImage)
                {
                    continue;
                }

                const int numColors = 3;
                if (cameraImage.Compression == Codec.RAW && cameraImage.ColorSpace == Colorspace.RGB)
                {
                    Color32[] newDecodedAnvelFrameData = new Color32[cameraImage.Frame.Length / numColors];

                    for (int i = 0; i < cameraImage.Frame.Length; i += numColors)
                    {
                        // Anvel images are inverted
                        newDecodedAnvelFrameData[((cameraImage.Frame.Length - i) / numColors) - 1] = new Color32(
                            cameraImage.Frame[i],
                            cameraImage.Frame[i + 1],
                            cameraImage.Frame[i + 2],
                            1);
                    }

                    decodedAnvelFrameData = newDecodedAnvelFrameData;
                }
                else
                {
                    Debug.LogWarningFormat("Unsupported Image Format: Compression: {0}; ColorSpace: {1}", cameraImage.Compression, cameraImage.ColorSpace);
                }
            }
        }
    }

}