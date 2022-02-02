using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AIGamedevToolkit
{
    /// <summary>
    /// A scriptable object for creating inference feature assets that 
    /// perform style transfer using OpenVINO
    /// </summary>
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Feature/OpenVINO/Style Transfer")]
    [System.Serializable]
    public class InferenceFeatureOpenVINOStyleTransfer : InferenceFeatureVision, IOpenVINOInferenceFeature
    {
        /// <summary>
        /// Keeps track of the attached model assets
        /// </summary>
        public ModelOpenVINO[] modelAssets;

        /// <summary>
        /// The compute shader containing the required processing steps
        /// </summary>
        public ComputeShader computeShader;

        /// <summary>
        /// The compute device used when performing inference
        /// </summary>
        [ListToPopup(typeof(InferenceFeatureOpenVINOStyleTransfer), "deviceList")]
        public string Devices = "";
        
        /// <summary>
        /// Stores the names of the available compute devices
        /// </summary>
        public static List<string> deviceList = new List<string>();
        
        /// <summary>
        /// The model asset used when performing inference
        /// </summary>
        [ListToPopup(typeof(InferenceFeatureOpenVINOStyleTransfer), "modelList")]
        public string Models = "";

        /// <summary>
        /// Stores the names of the available model assets
        /// </summary>
        public static List<string> modelList = new List<string>();

        /// <summary>
        /// Implements the functionality for performing style transfer inference with OpenVINO
        /// </summary>
        public StyleTransferOpenVINO styleTransferOpenVINO;

        /// <summary>
        /// Contains the input texture that will be sent to the OpenVINO inference engine
        /// </summary>
        private Texture2D inputTex;

        /// <summary>
        /// Stores the raw pixel data for inputTex
        /// </summary>
        private byte[] inputData;

        #pragma warning disable 0414
        /// <summary>
        /// Keeps track of whether to show the settings for the attached model assets
        /// </summary>
        private bool showModelAssetSettings = false;
        
        /// <summary>
        /// The label for the models assets settings section
        /// </summary>
        private string modelAssetSettingsLabel = "Model Asset Settings";
        #pragma warning restore 0414

        /// <summary>
        /// Called whenever a different model asset is selected
        /// </summary>
        public void UpdateModel()
        {
            Debug.Log($"{this.name}: Model asset changed to {Models}");
            Initialize();
        }

        /// <summary>
        /// Called whenver a different compute device is selected
        /// </summary>
        public void UpdateDevice()
        {
            Debug.Log($"{this.name}: Compute device changed to {Devices}");
            styleTransferOpenVINO.DeviceIndex = deviceList.IndexOf(Devices);
            Initialize();
        }

        /// <summary>
        /// Called whenver different input dimensions are entered
        /// </summary>
        public void UpdateInputDims()
        {
            // Prevent the input dimensions from going too low for the model
            if (targetDims.x < 64 || targetDims.y < 64) return;

            InitializeTextures();
            Debug.Log($"{this.name}: Input Dimensions changed to {targetDims}");
            Initialize();
        }


        /// <summary>
        /// Instantiate any objects for InferenceFeatureOpenVINOStyleTransfer asset
        /// </summary>
        public override void Instantiate()
        {
            styleTransferOpenVINO = new StyleTransferOpenVINO();
        }

        /// <summary>
        /// Initialize the dropdown menu for the InferenceFeatureOpenVINOStyleTransfer asset
        /// </summary>
        public override void InitializeDropdowns()
        {
            // Initialzie the selected compute device to CPU
            Devices = "CPU";
            // Initialize list of available compute devices
            deviceList = new List<string>();
            // Initialize list of available model assets
            modelList = new List<string>();

            // Get a list of compute devices available for the OpenVINO plugin
            deviceList = styleTransferOpenVINO.GetAvailableDevices();
            // Set the selected compute device to the first device in the list
            Devices = deviceList[0];
            // Add attached model assets to list of models
            foreach (ModelOpenVINO model in modelAssets) modelList.Add(model.name);
        }

        /// <summary>
        /// Get the path for the currently selected model asset
        /// </summary>
        /// <returns></returns>
        public string GetCurrentModelPath()
        {
            #if UNITY_EDITOR
            // Get path for the model in the Assets folder when in the Editor
            return modelAssets[modelList.IndexOf(Models)].modelPath;
            #else
            // Get the path for the model in the StreamingAssets folder when in build
            string modelPath = modelAssets[modelList.IndexOf(Models)].modelPath;
            string fileName = modelPath.Substring(modelPath.LastIndexOf("/"));
            string streamingPath = Application.streamingAssetsPath + "/" + modelAssets[modelList.IndexOf(Models)].streamingAssetsPath + fileName;
            return streamingPath;
            #endif
        }

        /// <summary>
        /// Perform any initialization steps required for using the InferenceFeatureOpenVINOStyleTransfer asset
        /// </summary>
        public override void Initialize()
        {
            // Initialize the input textures
            InitializeTextures();
            // Update inputTex with the new dimensions
            inputTex = new Texture2D(imageDims.x, imageDims.y, TextureFormat.RGBA32, false);
            // Set up the neural network for the OpenVINO inference engine
            styleTransferOpenVINO.InputDims = this.imageDims;

            // Don't try to initialize styleTransferOpenVINO if it has not been instantiated
            if (styleTransferOpenVINO == null) return;

            // Initialize OpenVINO plugin using the current model and compute device
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

        /// <summary>
        /// Perform inference using pixel data from the provided RenderTexture
        /// </summary>
        /// <param name="renderTexture">Contains the pixel data for the model input</param>
        public override void Inference(RenderTexture renderTexture)
        {
            // Only perform inference when unsafe code is enabled
            #if AIGAMEDEV_UNSAFE
            if (!this.active || styleTransferOpenVINO == null) return;
            // Create a temporary RenderTexture with the desired input resolution
            RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);
            // Copy the input texture data to the temporary texture
            Graphics.Blit(renderTexture, tempTex);
            // Flip image before sending to DLL
            FlipImage(computeShader, tempTex, "FlipXAxis");
            // Set active RenderTexture to temporary RenderTexture
            RenderTexture.active = tempTex;
            // Copy texture data from the GPU to the CPU
            inputTex.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
            // Apply changes to texture
            inputTex.Apply();
            // Get raw data from CPU texture
            inputData = inputTex.GetRawTextureData();
            // Send reference to inputData to DLL
            styleTransferOpenVINO.UploadTexture(inputData);
            // Load the new image data from the DLL to the texture
            inputTex.LoadRawTextureData(inputData);
            // Apply the changes to the texture
            inputTex.Apply();
            // Copy output texture data back to the temporary RenderTexture
            Graphics.Blit(inputTex, tempTex);
            // Flip image before sending to DLL
            FlipImage(computeShader, tempTex, "FlipXAxis");
            // Copy temporary texture data to the input RenderTexture
            Graphics.Blit(tempTex, renderTexture);
            // Release memory resources allocated for the temporary texture
            RenderTexture.ReleaseTemporary(tempTex);
            #else
            // Inform user when unsafe code is not enabled
            Debug.Log("Unsafe code needs to be enabled for OpenVINO inference. Please enable \"Allow 'unsafe' Code\" in Player settings.");
            #endif
        }

        /// <summary>
        /// Draw a custom editor GUI for the InferenceFeatureOpenVINOStyleTransfer asset
        /// </summary>
        public override void DrawUI()
        {
            // Only execute editor code when in the Unity Editor
            #if UNITY_EDITOR
            // Only display property fields when unsafe code is enabled
            #if AIGAMEDEV_UNSAFE

            // Get references to properties
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty m_ActiveProp = serializedObject.FindProperty("active");
            SerializedProperty m_InputTextureProp = serializedObject.FindProperty("inputTexture");
            SerializedProperty m_ModelAssetsProp = serializedObject.FindProperty("modelAssets");
            SerializedProperty m_ComputeShaderProp = serializedObject.FindProperty("computeShader");
            SerializedProperty m_ModelsProp = serializedObject.FindProperty("Models");
            SerializedProperty m_DevicesProp = serializedObject.FindProperty("Devices");
            SerializedProperty m_TargetDimsProp = serializedObject.FindProperty("targetDims");

            // Display property fields
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
            // Check for changes to the Models property field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ModelsProp, new GUIContent("Models"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateModel();
            }
            // Check for changes to the Devices property field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_DevicesProp, new GUIContent("Devices"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateDevice();
            }
            // Check for changes to the input dimensions
            EditorGUI.BeginChangeCheck();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(m_TargetDimsProp, new GUIContent("Input Dimensions"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateInputDims();
            }
            // Provide option to copy all attached model assets to the StreamingAssets folder
            EditorGUILayout.LabelField("Build Preparation", EditorStyles.boldLabel);
            if (GUILayout.Button("Copy Models to StreamingAssets"))
            {
                string streamingAssetsDir = "Assets/StreamingAssets";
                foreach (ModelOpenVINO modelAsset in modelAssets)
                {
                    CustomEditorUtils.CopyToStreamingAssets(modelAsset, streamingAssetsDir);
                }
            }

            // Save changes to properties
            EditorUtility.SetDirty(this);
            #else
            // Inform user when unsafe code is not enabled
            EditorGUILayout.HelpBox("Unsafe code needs to be enabled for OpenVINO inference. Please enable \"Allow 'unsafe' Code\" in Player settings.", MessageType.Warning);
            #endif
            #endif
        }

        /// <summary>
        /// Perform any required cleanup steps
        /// </summary>
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