using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{

    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Feature/OpenVINO/YOLOX")]
    [System.Serializable]
    public class InferenceFeatureOpenVINOYOLOX : InferenceFeatureObjectDetection2D, IOpenVINOInferenceFeature
    {

        public ModelOpenVINO[] modelAssets;
        

        public ComputeShader computeShader;
        

        [ListToPopup(typeof(InferenceFeatureOpenVINOYOLOX), "deviceList")]
        public string Devices = "";
        // 
        public static List<string> deviceList = new List<string>();
        [ListToPopup(typeof(InferenceFeatureOpenVINOYOLOX), "modelList")]
        public string Models = "";
        // 
        public static List<string> modelList = new List<string>();
        [Tooltip("The Non-maximum supression threshold")]
        [Range(0, 1.0f)]
        public float nmsThreshold = 0.45f;

        [Tooltip("The minimum confidence score needed to keep a model prediction")]
        [Range(0, 1.0f)]
        public float minConfidence = 0.3f;

        public YOLOXOpenVINO yoloxOpenVINO;


        // Contains the input texture that will be sent to the OpenVINO inference engine
        private Texture2D inputTex;

        // Stores the raw pixel data for inputTex
        private byte[] inputData;


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


        public void UpdateModel()
        {
            Debug.Log($"{this.name}: Model asset changed to {Models}");
            Initialize();
        }

        public void UpdateDevice()
        {
            Debug.Log($"{this.name}: Compute device changed to {Devices}");
            yoloxOpenVINO.SetDeviceIndex(deviceList.IndexOf(Devices));
            Initialize();
        }

        public void UpdateNMSThreshold()
        {
            Debug.Log($"{this.name}: NMS Threshold changed to {nmsThreshold}");
            yoloxOpenVINO.SetInstanceNMSThreshold(nmsThreshold);
        }

        public void UpdateMinConfidence()
        {
            Debug.Log($"{this.name}: Minimum Confidence changed to {minConfidence}");
            yoloxOpenVINO.SetInstanceConfidenceThreshold(minConfidence);
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
            yoloxOpenVINO = new YOLOXOpenVINO();
            imageDims = new Vector2Int();
        }


        public override void InitializeDropdowns()
        {
            Devices = "CPU";
            deviceList = new List<string>();
            modelList = new List<string>();

            deviceList = new List<string>(yoloxOpenVINO.GetAvailableDevices());
            Devices = deviceList[0];
            foreach (ModelOpenVINO model in modelAssets) modelList.Add(model.name);
        }


        public override void Initialize()
        {
            objectInfoArray = new Object[0];
                        
            InitializeTextures();
            // Update inputTex with the new dimensions
            inputTex = new Texture2D(imageDims.x, imageDims.y, TextureFormat.RGBA32, false);

            // Set up the neural network for the OpenVINO inference engine
            yoloxOpenVINO.SetInputDims(this.imageDims);

                        
            if (Devices.Length > 0 && Models.Length > 0)
            {
                yoloxOpenVINO.InitializePlugin(GetCurrentModelPath(), deviceList.IndexOf(Devices));
            }
            else
            {
                yoloxOpenVINO.InitializePlugin(modelAssets[0].modelPath, 0);
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
            objectInfoArray = yoloxOpenVINO.UploadTexture(inputData);

            // Update bounding boxes with new object info
            UpdateObjectInfo();

            RenderTexture.ReleaseTemporary(tempTex);
        }


        /// <summary>
        /// Update the list of bounding boxes based on the latest output from the model
        /// </summary>
        public void UpdateObjectInfo()
        {
            // Process new detected objects
            for (int i = 0; i < objectInfoArray.Length; i++)
            {
                // The smallest dimension of the screen
                int minDimension = Mathf.Min(Screen.width, Screen.height);
                // The value used to scale the bbox locations up to the source resolution
                float scale = (float)minDimension / Mathf.Min(this.imageDims.x, this.imageDims.y);

                // Flip the bbox coordinates vertically
                objectInfoArray[i].y0 = this.imageDims.y - objectInfoArray[i].y0;

                objectInfoArray[i].x0 *= scale;
                objectInfoArray[i].y0 *= scale;
                objectInfoArray[i].width *= scale;
                objectInfoArray[i].height *= scale;
            }
        }


        public override void CleanUp()
        {
            try
            {
                yoloxOpenVINO.CleanUp();
            }
            catch
            {

            }
        }
    }

}