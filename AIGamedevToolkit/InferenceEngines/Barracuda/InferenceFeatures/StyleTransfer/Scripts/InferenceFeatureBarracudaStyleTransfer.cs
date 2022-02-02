using System.Collections.Generic;
using UnityEngine;

// Only use the Unity.Barracuda namespace if the Barracuda package is installed
#if AIGAMEDEV_BARRACUDA
using Unity.Barracuda;
#endif

// Only use the UnityEditor namespace when in the Unity Editor
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AIGamedevToolkit
{
    /// <summary>
    /// A scriptable object for creating inference feature assets that 
    /// perform style transfer using the Barracuda package
    /// </summary>
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Feature/Barracuda/Style Transfer")]
    [System.Serializable]
    public class InferenceFeatureBarracudaStyleTransfer : InferenceFeatureVision
    {
        /// <summary>
        /// The compute shader containing the required processing steps
        /// </summary>
        public ComputeShader computeShader;

        /// <summary>
        /// Displays a popup list of available onnx models
        /// </summary>
        [ListToPopup(typeof(InferenceFeatureBarracudaStyleTransfer), "modelList")]
        public string Models = "";
        
        /// <summary>
        /// Stores a list of available onnx models
        /// </summary>
        public static List<string> modelList = new List<string>();

        /// <summary>
        /// Implements the functionality for performing inference with Barracuda
        /// </summary>
        public StyleTransferBarracuda styleTransferBarracuda;

        // Only declare variables requiring the Barracuda package if it is installed
        #if AIGAMEDEV_BARRACUDA
        /// <summary>
        /// The compute backend used when performing inference
        /// </summary>
        [Tooltip("The compute backend used when performing inference")]
        public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;
        /// <summary>
        /// The model asset file that will be used when performing inference
        /// </summary>
        [Tooltip("The model asset file that will be used when performing inference")]
        public NNModel[] modelAssets;
        #endif

        /// <summary>
        /// Instantiate any objects for InferenceFeatureBarracudaStyleTransfer asset
        /// </summary>
        public override void Instantiate()
        {
            // Create a new StyleTransferBarracuda instance
            styleTransferBarracuda = new StyleTransferBarracuda();
        }


        /// <summary>
        /// Perform any initialization steps required for using the InferenceFeatureBarracudaStyleTransfer asset
        /// </summary>
        public override void Initialize()
        {
            // Only perform any initialization steps if the Barracuda package is installed
            #if AIGAMEDEV_BARRACUDA
            // Prevent the input dimensions from going too low for the model
            if (targetDims.x < 64 || targetDims.y < 64) return;
            InitializeTextures();

            // Don't try to initialize styleTransferBarracuda if it has not been instantiated
            if (styleTransferBarracuda == null) return;
            // Initialize Barracuda worker
            styleTransferBarracuda.InitializeEngine(modelAssets[modelList.IndexOf(Models)], workerType);
            #endif
        }

        /// <summary>
        /// Initialize the dropdown menu for the InferenceFeatureBarracudaStyleTransfer asset
        /// </summary>
        public override void InitializeDropdowns()
        {
            // Only initialize list of model assets if the Barracuda package is installed
            #if AIGAMEDEV_BARRACUDA
            // Get the names of the model assets
            foreach (NNModel modelAsset in modelAssets) modelList.Add(modelAsset.name);
            #endif
        }

        /// <summary>
        /// Process the provided image using the specified function on the GPU
        /// </summary>
        /// <param name="image">The texture to process</param>
        /// <param name="functionName">The name of the compute shader function to call</param>
        /// <returns>The processed image</returns>
        private void ProcessImage(RenderTexture image, string functionName)
        {
            // Specify the number of threads on the GPU
            int numthreads = 8;
            // Get the index for the specified function in the ComputeShader
            int kernelHandle = computeShader.FindKernel(functionName);
            // Define a temporary HDR RenderTexture
            RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, RenderTextureFormat.ARGBHalf);
            // Enable random write access
            result.enableRandomWrite = true;
            // Create the HDR RenderTexture
            result.Create();

            // Set the value for the Result variable in the ComputeShader
            computeShader.SetTexture(kernelHandle, "Result", result);
            // Set the value for the InputImage variable in the ComputeShader
            computeShader.SetTexture(kernelHandle, "InputImage", image);

            // Execute the ComputeShader
            computeShader.Dispatch(kernelHandle, result.width / numthreads, result.height / numthreads, 1);

            // Copy the result into the source RenderTexture
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
            // Only perform inference when the inference feature is active and initialized
            if (!this.active || styleTransferBarracuda == null) return;

            // Only execute Barracuda code when Barracuda package is installed
            #if AIGAMEDEV_BARRACUDA
            // Create a temporary RenderTexture with the desired input resolution
            RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);
            // Copy the input texture data to the temporary texture
            Graphics.Blit(renderTexture, tempTex);
            // Perform preprocessing steps
            ProcessImage(tempTex, "ProcessInput");
            // Execute model
            styleTransferBarracuda.Exectute(tempTex);
            // Perform postprocessing steps
            ProcessImage(tempTex, "ProcessOutput");
            // Copy temporary texture data to the input RenderTexture
            Graphics.Blit(tempTex, renderTexture);
            // Release memory resources allocated for the temporary texture
            RenderTexture.ReleaseTemporary(tempTex);
            #endif
        }

        /// <summary>
        /// Draw a custom editor GUI for the InferenceFeatureBarracudaStyleTransfer asset
        /// </summary>
        public override void DrawUI()
        {
            // Only execute editor code when in the Unity Editor
            #if UNITY_EDITOR
            // Only display property fields when the Barracuda package is installed
            #if AIGAMEDEV_BARRACUDA

            // Get references to properties
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty m_ActiveProp = serializedObject.FindProperty("active");
            SerializedProperty m_InputTextureProp = serializedObject.FindProperty("inputTexture");
            SerializedProperty m_ModelAssetsProp = serializedObject.FindProperty("modelAssets");
            SerializedProperty m_ComputeShaderProp = serializedObject.FindProperty("computeShader");
            SerializedProperty m_ModelsProp = serializedObject.FindProperty("Models");
            SerializedProperty m_DevicesProp = serializedObject.FindProperty("workerType");
            SerializedProperty m_TargetDimsProp = serializedObject.FindProperty("targetDims");

            // Keep track of GUI changes
            EditorGUI.BeginChangeCheck();
            
            // Display property fields
            EditorGUILayout.PropertyField(m_ActiveProp, new GUIContent("Active"));
            EditorGUILayout.PropertyField(m_InputTextureProp, new GUIContent("Input Texture"));
            EditorGUILayout.LabelField("Image Processing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ComputeShaderProp, new GUIContent("ComputeShader"));
            EditorGUILayout.LabelField("Model", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ModelAssetsProp, new GUIContent("Model Assets"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.LabelField("Style Transfer", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ModelsProp, new GUIContent("Models"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(m_DevicesProp, new GUIContent("Devices"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(m_TargetDimsProp, new GUIContent("Input Dimensions"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
                       
            // Call the Initialize method when the user change a parameter in the Inspector 
            if (EditorGUI.EndChangeCheck())
            {
                Initialize();
            }

            // Save changes to properties
            EditorUtility.SetDirty(this);
            #else
            // Inform user when the Barracuda package is not installed
            EditorGUILayout.HelpBox("Barracuda package not installed", MessageType.Warning);
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
                #if AIGAMEDEV_BARRACUDA
                styleTransferBarracuda.CleanUp();
                #endif
            }
            catch
            {

            }
        }

    }

}