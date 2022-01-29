using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace AIGamedevToolkit
{
    [CreateAssetMenu]
    [System.Serializable]
    public class InferenceFeatureOpenVINOStyleTransfer : InferenceFeatureVision, IOpenVINOInferenceFeature
    {

        public ModelOpenVINO[] modelAssets;

        public ComputeShader computeShader;

        public StyleTransferOpenVINO styleTransferOpenVINO;

        [Header("Style Transfer - OpenVINO")]
        [ListToPopup(typeof(InferenceFeatureOpenVINOStyleTransfer), "deviceList")]
        public string Devices = "";
        // 
        public static List<string> deviceList = new List<string>();
        [ListToPopup(typeof(InferenceFeatureOpenVINOStyleTransfer), "modelList")]
        public string Models = "";
        // 
        public static List<string> modelList = new List<string>();


        public string currentDevice;
        public string currentModel;

        // Contains the input texture that will be sent to the OpenVINO inference engine
        private Texture2D inputTex;

        // Stores the raw pixel data for inputTex
        private byte[] inputData;


        public void UpdateModel()
        {
            Debug.Log($"{this.name}: Model asset changed to {Models}");
            Initialize();
        }

        public void UpdateDevice()
        {
            Debug.Log($"{this.name}: Compute device changed to {Devices}");
            styleTransferOpenVINO.SetDeviceIndex(deviceList.IndexOf(Devices));
            Initialize();
        }

        public void UpdateInputDims()
        {
            // Prevent the input dimensions from going too low for the model
            targetDims.x = Mathf.Max(targetDims.x, 64);
            targetDims.y = Mathf.Max(targetDims.y, 64);

            //if (targetDims.x < 64 || targetDims.y < 64) return;

            InitializeTextures();
            Debug.Log($"{this.name}: Input Dimensions changed to {targetDims}");
            Initialize();
        }


        public override void Instantiate()
        {
            styleTransferOpenVINO = new StyleTransferOpenVINO();
        }



        public override void InitializeDropdowns()
        {
            Devices = "CPU";
            deviceList = new List<string>();
            modelList = new List<string>();

            deviceList = new List<string>(styleTransferOpenVINO.GetAvailableDevices());
            Devices = deviceList[0];
            foreach (ModelOpenVINO model in modelAssets) modelList.Add(model.name);
        }


        public string GetCurrentModelPath()
        {
            #if UNITY_EDITOR
            return modelAssets[modelList.IndexOf(Models)].modelPath;

            #else
            string modelPath = modelAssets[modelList.IndexOf(Models)].modelPath;
            string fileName = modelPath.Substring(modelPath.LastIndexOf("/"));
            string streamingPath = Application.streamingAssetsPath + "/" + modelAssets[modelList.IndexOf(Models)].streamingAssetsPath + fileName;
            return streamingPath;
            #endif
        }


        public override void Initialize()
        {
            InitializeTextures();
            // Update inputTex with the new dimensions
            inputTex = new Texture2D(imageDims.x, imageDims.y, TextureFormat.RGBA32, false);

            // Set up the neural network for the OpenVINO inference engine
            styleTransferOpenVINO.SetInputDims(this.imageDims);
            if (Devices.Length > 0 && Models.Length > 0)
            {
                styleTransferOpenVINO.InitializePlugin(GetCurrentModelPath(), deviceList.IndexOf(Devices));
            }
            else
            {
                styleTransferOpenVINO.InitializePlugin(modelAssets[0].modelPath, 0);
            }
        }


        public override void Inference(RenderTexture renderTexture)
        {
            if (!this.active) return;

            RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);

            Graphics.Blit(renderTexture, tempTex);


            // Flip image before sending to DLL
            OpenVINOUtils.FlipImage(computeShader, tempTex, "FlipXAxis");

            RenderTexture.active = tempTex;
            inputTex.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
            inputTex.Apply();

            inputData = inputTex.GetRawTextureData();

            // Send reference to inputData to DLL
            styleTransferOpenVINO.UploadTexture(inputData);

            // Load the new image data from the DLL to the texture
            inputTex.LoadRawTextureData(inputData);
            // Apply the changes to the texture
            inputTex.Apply();

            Graphics.Blit(inputTex, tempTex);

            // Flip image before sending to DLL
            OpenVINOUtils.FlipImage(computeShader, tempTex, "FlipXAxis");

            Graphics.Blit(tempTex, renderTexture);

            RenderTexture.ReleaseTemporary(tempTex);
        }


        public override void CleanUp()
        {
            try
            {
                styleTransferOpenVINO.CleanUp();
            }
            catch
            {

            }
        }

    }

}