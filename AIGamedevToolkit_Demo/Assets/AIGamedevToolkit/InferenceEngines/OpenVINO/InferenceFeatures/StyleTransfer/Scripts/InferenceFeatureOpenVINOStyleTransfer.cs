using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AIGamedevToolkit
{
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Feature/OpenVINO/Style Transfer")]
    [System.Serializable]
    public class InferenceFeatureOpenVINOStyleTransfer : InferenceFeatureVision, IOpenVINOInferenceFeature
    {

        public ModelOpenVINO[] modelAssets;

        public ComputeShader computeShader;

        public StyleTransferOpenVINO styleTransferOpenVINO;

        //[Header("Style Transfer - OpenVINO")]
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


        private bool showModelAssetSettings = false;
        private string modelAssetSettingsLabel = "Model Asset Settings";


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
            if (targetDims.x < 64 || targetDims.y < 64) return;

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


        /// <summary>
        /// Perform a flip operation of the GPU
        /// </summary>
        /// <param name="image">The image to be flipped</param>
        /// <param name="tempTex">Stores the flipped image</param>
        /// <param name="functionName">The name of the function to execute in the compute shader</param>
        private void FlipImage(ComputeShader computeShader, RenderTexture image, string functionName)
        {
            // Specify the number of threads on the GPU
            int numthreads = 8;
            // Get the index for the PreprocessResNet function in the ComputeShader
            int kernelHandle = computeShader.FindKernel(functionName);

            /// Allocate a temporary RenderTexture
            RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, image.format);
            // Enable random write access
            result.enableRandomWrite = true;
            // Create the RenderTexture
            result.Create();

            // Set the value for the Result variable in the ComputeShader
            computeShader.SetTexture(kernelHandle, "Result", result);
            // Set the value for the InputImage variable in the ComputeShader
            computeShader.SetTexture(kernelHandle, "InputImage", image);
            // Set the value for the height variable in the ComputeShader
            computeShader.SetInt("height", image.height);
            // Set the value for the width variable in the ComputeShader
            computeShader.SetInt("width", image.width);

            // Execute the ComputeShader
            computeShader.Dispatch(kernelHandle, image.width / numthreads, image.height / numthreads, 1);

            // Copy the flipped image to tempTex
            Graphics.Blit(result, image);

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(result);
        }

        public override void Inference(RenderTexture renderTexture)
        {
            if (!this.active) return;

            RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);

            Graphics.Blit(renderTexture, tempTex);


            // Flip image before sending to DLL
            FlipImage(computeShader, tempTex, "FlipXAxis");

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
            FlipImage(computeShader, tempTex, "FlipXAxis");

            Graphics.Blit(tempTex, renderTexture);

            RenderTexture.ReleaseTemporary(tempTex);
        }


        public override void DrawUI()
        {
#if UNITY_EDITOR
            SerializedObject serializedObject = new SerializedObject(this);

            SerializedProperty m_ActiveProp = serializedObject.FindProperty("active");
            SerializedProperty m_InputTextureProp = serializedObject.FindProperty("inputTexture");
            SerializedProperty m_ModelAssetsProp = serializedObject.FindProperty("modelAssets");
            SerializedProperty m_ComputeShaderProp = serializedObject.FindProperty("computeShader");
            SerializedProperty m_ModelsProp = serializedObject.FindProperty("Models");
            SerializedProperty m_DevicesProp = serializedObject.FindProperty("Devices");
            SerializedProperty m_TargetDimsProp = serializedObject.FindProperty("targetDims");

            EditorGUILayout.PropertyField(m_ActiveProp, new GUIContent("Active"));
            EditorGUILayout.PropertyField(m_InputTextureProp, new GUIContent("Input Texture"));


            EditorGUILayout.LabelField("Image Processing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ComputeShaderProp, new GUIContent("ComputeShader"));


            EditorGUILayout.LabelField("Model", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ModelAssetsProp, new GUIContent("Model Assets"));
            showModelAssetSettings = EditorGUILayout.Foldout(showModelAssetSettings, modelAssetSettingsLabel);
            if (showModelAssetSettings)
            {
                foreach (ModelOpenVINO model in modelAssets)
                {
                    EditorGUILayout.LabelField(model.name, EditorStyles.boldLabel);
                    model.DrawUI(model);
                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();
            }
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();



            EditorGUILayout.LabelField("Style Transfer", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ModelsProp, new GUIContent("Models"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateModel();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_DevicesProp, new GUIContent("Devices"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateDevice();
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(m_TargetDimsProp, new GUIContent("Input Dimensions"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateInputDims();
            }


            EditorGUILayout.LabelField("Build Preparation", EditorStyles.boldLabel);
            if (GUILayout.Button("Copy Models to StreamingAssets"))
            {
                string streamingAssetsDir = "Assets/StreamingAssets";
                foreach (ModelOpenVINO modelAsset in modelAssets)
                {
                    CustomEditorUtils.CopyToStreamingAssets(modelAsset, streamingAssetsDir);
                }
            }

            EditorUtility.SetDirty(this);
#endif
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