using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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


        private bool showModelAssetSettings = false;
        private string modelAssetSettingsLabel = "Model Asset Settings";


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
            if (targetDims.x < 64 || targetDims.y < 64) return;

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

            if (yoloxOpenVINO == null) return;
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
#if AIGAMEDEV_UNSAFE
            if (!this.active || yoloxOpenVINO == null) return;

            RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);

            Graphics.Blit(renderTexture, tempTex);


            // Flip image before sending to DLL
            FlipImage(computeShader, tempTex, "FlipXAxis");

            RenderTexture.active = tempTex;
            inputTex.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
            inputTex.Apply();


            inputData = inputTex.GetRawTextureData();

            // Send reference to inputData to DLL
            objectInfoArray = yoloxOpenVINO.UploadTexture(inputData);

            // Update bounding boxes with new object info
            UpdateObjectInfo();

            RenderTexture.ReleaseTemporary(tempTex);
#else
            Debug.Log("Unsafe code needs to be enabled for OpenVINO inference. Please enable \"Allow 'unsafe' Code\" in Player settings.");
#endif
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

        public override void ApplyToScene()
        {
            // make sure base gets called so we got an inference manager
            base.ApplyToScene();

            // Add the bounding box manager Game Object
            GameObject bbmGO = GameObject.Find("Bounding Box Manager");
            if (bbmGO == null)
            {
                bbmGO = new GameObject("Bounding Box Manager");
            }

            // Attach the bounding box component
            BoundingBoxManager boundingBoxManager = bbmGO.GetComponent<BoundingBoxManager>();
            if (boundingBoxManager == null)
            {
                boundingBoxManager = bbmGO.AddComponent<BoundingBoxManager>();
            }

            if (boundingBoxManager.objectDetectors == null || boundingBoxManager.objectDetectors.Length <= 0)
            {
#if UNITY_EDITOR
                string objectDetectorPath = Utils.GetAssetPath(this.name, "asset");
                InferenceFeatureObjectDetection2D objectDetection2D = (InferenceFeatureObjectDetection2D)AssetDatabase.LoadAssetAtPath(objectDetectorPath, typeof(InferenceFeatureObjectDetection2D));
                boundingBoxManager.objectDetectors = new InferenceFeatureObjectDetection2D[1] { objectDetection2D };
#endif
            }

        }

        public override void DrawEditorOptions()
        {
#if UNITY_EDITOR
            SerializedObject serializedObject = new SerializedObject(this);

            SerializedProperty m_ActiveProp = serializedObject.FindProperty("active");
            SerializedProperty m_InputTextureProp = serializedObject.FindProperty("inputTexture");
            SerializedProperty m_ModelAssetsProp = serializedObject.FindProperty("modelAssets");
            SerializedProperty m_ClassListProp = serializedObject.FindProperty("classList");
            SerializedProperty m_DisplayBoxesProp = serializedObject.FindProperty("displayBoundingBoxes");
            SerializedProperty m_ComputeShaderProp = serializedObject.FindProperty("computeShader");
            SerializedProperty m_ModelsProp = serializedObject.FindProperty("Models");
            SerializedProperty m_DevicesProp = serializedObject.FindProperty("Devices");
            SerializedProperty m_NmsProp = serializedObject.FindProperty("nmsThreshold");
            SerializedProperty m_MinConfidenceProp = serializedObject.FindProperty("minConfidence");
            SerializedProperty m_TargetDimsProp = serializedObject.FindProperty("targetDims");

            EditorGUILayout.PropertyField(m_ActiveProp, new GUIContent("Active"));
            EditorGUILayout.PropertyField(m_InputTextureProp, new GUIContent("Input Texture"));


            EditorGUILayout.LabelField("Image Processing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ComputeShaderProp, new GUIContent("ComputeShader"));


            EditorGUILayout.LabelField("Model", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ModelAssetsProp, new GUIContent("Model Assets"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("Object Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ClassListProp, new GUIContent("Class List"));
            EditorGUILayout.PropertyField(m_DisplayBoxesProp, new GUIContent("Display Bounding Boxes"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("YOLOX", EditorStyles.boldLabel);
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
            EditorGUILayout.PropertyField(m_NmsProp, new GUIContent("NMS Threshold"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateNMSThreshold();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_MinConfidenceProp, new GUIContent("Minimum Confidence"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateMinConfidence();
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
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(14 * EditorGUI.indentLevel);
            if (GUILayout.Button("Copy Models to StreamingAssets"))
            {
                string streamingAssetsDir = "Assets/StreamingAssets";
                foreach (ModelOpenVINO modelAsset in modelAssets)
                {
                    CustomEditorUtils.CopyToStreamingAssets(modelAsset, streamingAssetsDir);
                }
            }
            EditorGUILayout.EndHorizontal();
#endif
        }


        public override void DrawUI()
        {
#if UNITY_EDITOR


            SerializedObject serializedObject = new SerializedObject(this);


            SerializedProperty m_ActiveProp = serializedObject.FindProperty("active");
            SerializedProperty m_InputTextureProp = serializedObject.FindProperty("inputTexture");
            SerializedProperty m_ModelAssetsProp = serializedObject.FindProperty("modelAssets");
            SerializedProperty m_ClassListProp = serializedObject.FindProperty("classList");
            SerializedProperty m_DisplayBoxesProp = serializedObject.FindProperty("displayBoundingBoxes");
            SerializedProperty m_ComputeShaderProp = serializedObject.FindProperty("computeShader");
            SerializedProperty m_ModelsProp = serializedObject.FindProperty("Models");
            SerializedProperty m_DevicesProp = serializedObject.FindProperty("Devices");
            SerializedProperty m_NmsProp = serializedObject.FindProperty("nmsThreshold");
            SerializedProperty m_MinConfidenceProp = serializedObject.FindProperty("minConfidence");
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

            EditorGUILayout.LabelField("Object Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ClassListProp, new GUIContent("Class List"));
            EditorGUILayout.PropertyField(m_DisplayBoxesProp, new GUIContent("Display Bounding Boxes"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("YOLOX", EditorStyles.boldLabel);
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
            EditorGUILayout.PropertyField(m_NmsProp, new GUIContent("NMS Threshold"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateNMSThreshold();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_MinConfidenceProp, new GUIContent("Minimum Confidence"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                UpdateMinConfidence();
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
    }
}