using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace AIGamedevToolkit
{
    public class InferenceManager : MonoBehaviour
    {
        [Tooltip("")]
        public List<InferenceFeature> inferenceFeatureList;

        [HideInInspector]
        public bool showInferenceFeatureSettings = false;

        // Keeps track of whether to execute the OpenVINO model
        private bool performInference = true;

        // 
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



        private bool IntelHardwarePresent()
        {
            // Check if either the CPU of GPU is made by Intel
            string processorType = SystemInfo.processorType.ToString();
            string graphicsDeviceName = SystemInfo.graphicsDeviceName.ToString();
            return processorType.Contains("Intel") || graphicsDeviceName.Contains("Intel");
        }


        public void Awake()
        {
        #if UNITY_EDITOR
            
        #else

            Debug.Log("Checking for plugins.xml file");

            //string sourcePath = $"{Application.streamingAssetsPath}/plugins.xml";
            string targetPath = $"{Application.dataPath}/Plugins/x86_64/plugins.xml";

            if (File.Exists(targetPath))
            {
                Debug.Log("plugins.xml already in folder");
            }
            else
            {
                Debug.Log("Creating plugins.xml file in Plugins folder.");
                File.WriteAllText(targetPath, pluginsXmlFileContent);
            }

        #endif


            intelHardware = IntelHardwarePresent();

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
