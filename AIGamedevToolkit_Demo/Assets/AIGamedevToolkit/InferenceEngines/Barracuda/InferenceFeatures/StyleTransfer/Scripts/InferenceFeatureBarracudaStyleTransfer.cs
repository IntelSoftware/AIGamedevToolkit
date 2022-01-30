using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AIGamedevToolkit
{
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Feature/Barracuda/Style Transfer")]
    [System.Serializable]
    public class InferenceFeatureBarracudaStyleTransfer : InferenceFeatureVision
    {

        public ComputeShader computeShader;

        //[Header("Style Transfer - Barracuda")]
        [Tooltip("The backend used when performing inference")]
        public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;
        // 
        public static List<string> deviceList = new List<string>();
        [ListToPopup(typeof(InferenceFeatureBarracudaStyleTransfer), "modelList")]
        public string Models = "";
        // 
        public static List<string> modelList = new List<string>();

        [Tooltip("The model asset file that will be used when performing inference")]
        public NNModel[] modelAssets;



        public StyleTransferBarracuda styleTransferBarracuda;


        public override void Instantiate()
        {
            styleTransferBarracuda = new StyleTransferBarracuda();
        }


        public override void Initialize()
        {
            // Prevent the input dimensions from going too low for the model
            if (targetDims.x < 64 || targetDims.y < 64) return;
            InitializeTextures();

            if (styleTransferBarracuda == null) return;

            styleTransferBarracuda.InitializeEngine(modelAssets[modelList.IndexOf(Models)], workerType);
        }

        public override void InitializeDropdowns()
        {
            // Get the names of the model assets
            foreach (NNModel modelAsset in modelAssets) modelList.Add(modelAsset.name);
        }



        /// <summary>
        /// Process the provided image using the specified function on the GPU
        /// </summary>
        /// <param name="image"></param>
        /// <param name="functionName"></param>
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



        public override void Inference(RenderTexture renderTexture)
        {
            if (!this.active) return;

            RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);

            Graphics.Blit(renderTexture, tempTex);
            ProcessImage(tempTex, "ProcessInput");
            styleTransferBarracuda.Exectute(tempTex);
            ProcessImage(tempTex, "ProcessOutput");
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
            SerializedProperty m_DevicesProp = serializedObject.FindProperty("workerType");
            SerializedProperty m_TargetDimsProp = serializedObject.FindProperty("targetDims");

            EditorGUI.BeginChangeCheck();
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
            

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(m_TargetDimsProp, new GUIContent("Input Dimensions"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
                       

            if (EditorGUI.EndChangeCheck())
            {
                Initialize();
            }

            EditorUtility.SetDirty(this);
#endif
        }


        public override void CleanUp()
        {
            try
            {
                styleTransferBarracuda.CleanUp();
            }
            catch
            {

            }
        }

    }

}