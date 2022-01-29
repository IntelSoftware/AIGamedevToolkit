using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace AIGamedevToolkit
{
    public class InferenceManager : MonoBehaviour
    {
        [Header("OpenVINO")]
        [Tooltip("Performs the preprocessing and postprocessing steps")]
        public ComputeShader imageProcessingShader;

        [Header("Inference")]
        [Tooltip("Turn stylization on and off")]
        public bool inferenceToggle = true;
        [Tooltip("Turn AsyncGPUReadback on and off")]
        public bool useAsync = false;

        public InferenceFeature[] inferenceFeatureList;

        // Keeps track of whether to execute the OpenVINO model
        private bool performInference = true;

        private bool intelHardware;

        string pluginsXmlFileContent = @"<ie>
    <plugins>
        <plugin name=""AUTO"" location=""AutoPlugin.dll"">
        </plugin>
        <plugin name=""GNA"" location=""GNAPlugin.dll"">
        </plugin>
        <plugin name=""HETERO"" location=""HeteroPlugin.dll"">
        </plugin>
        <plugin name=""CPU"" location=""MKLDNNPlugin.dll"">
        </plugin>
        <plugin name=""MULTI"" location=""MultiDevicePlugin.dll"">
        </plugin>
        <plugin name=""GPU"" location=""clDNNPlugin.dll"">
        </plugin>
        <plugin name=""MYRIAD"" location=""myriadPlugin.dll"">
        </plugin>
        <plugin name=""HDDL"" location=""HDDLPlugin.dll"">
        </plugin>
        <plugin name=""VPUX"" location=""VPUXPlugin.dll"">
        </plugin>
    </plugins>
</ie>";


        public void Awake()
        {
        #if UNITY_EDITOR
            
            if (AssetDatabase.IsValidFolder("Assets/StreamingAssets") == false)
            {
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }

            File.WriteAllText("Assets/StreamingAssets/plugins.xml", pluginsXmlFileContent);
        #else

        Debug.Log("Checking for plugins.xml file");

            string sourcePath = $"{Application.streamingAssetsPath}/plugins.xml";
            string targetPath = $"{Application.dataPath}/Plugins/x86_64/plugins.xml";

            if (File.Exists(targetPath))
            {
                Debug.Log("plugins.xml already in folder");
            }
            else
            {
                Debug.Log("Moving plugins.xml file from StreamingAssets to Plugins folder.");
                File.Copy(sourcePath, targetPath);
            }

        #endif


            intelHardware = OpenVINOUtils.IntelHardwarePresent();

            foreach (InferenceFeature inferenceFeature in inferenceFeatureList)
            {
                if (inferenceFeature is IOpenVINOInferenceFeature && intelHardware == false)
                {
                    inferenceFeature.active = false;

                }
                else
                {
                    //Debug.Log($"Instantiating {inferenceFeature.name}");
                    inferenceFeature.Instantiate();
                    inferenceFeature.InitializeDropdowns();
                }
            }

            // Perform the requred 
            InitializeFeatures();
        }


        /// <summary>
        /// Initialize the options for the dropdown menus
        /// </summary>
        private void InitializeDropdowns()
        {
            foreach (InferenceFeature inferenceFeature in inferenceFeatureList)
            {
                inferenceFeature.InitializeDropdowns();
            }
        }


        /// <summary>
        /// Perform the initialization steps
        /// </summary>
        private void InitializeFeatures()
        {
            foreach (InferenceFeatureVision inferenceFeature in inferenceFeatureList)
            {
                if (inferenceFeature is IOpenVINOInferenceFeature && intelHardware == false)
                {
                    inferenceFeature.active = false;

                }
                else
                {
                    inferenceFeature.InitializeTextures();
                }
            }


            foreach (InferenceFeature inferenceFeature in inferenceFeatureList)
            {
                if (inferenceFeature is IOpenVINOInferenceFeature && intelHardware == false)
                {
                    inferenceFeature.active = false;

                }
                else
                {
                    inferenceFeature.Initialize();
                }
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }


        // Update is called once per frame
        void Update()
        {
            
        }


        /// <summary>
        /// Called when the input dimensions are updated in the GUI
        /// </summary>
        public void UpdateInputDims()
        {
            InitializeFeatures();
        }


        /// <summary>
        /// Called when the value for the Inference toggle is updated
        /// </summary>
        public void UpdateInferenceValue()
        {
            performInference = inferenceToggle;

            if (performInference)
            {

                InitializeFeatures();
            }
        }


        private void OnDisable()
        {
            foreach (InferenceFeature inferenceFeature in inferenceFeatureList)
            {
                inferenceFeature.CleanUp();
            }
        }


        /// <summary>
        /// Called when the Quit button is clicked.
        /// </summary>
        public void Quit()
        {
            // Causes the application to exit
            Application.Quit();
        }


        public void OnGUI()
        {

        }
    }
}
