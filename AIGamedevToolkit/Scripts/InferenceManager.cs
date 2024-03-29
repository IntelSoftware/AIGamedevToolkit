using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#else
using System.IO;
#endif


namespace AIGamedevToolkit
{
    public class InferenceManager : MonoBehaviour
    {
        /// <summary>
        /// The list of inference features to initialize
        /// </summary>
        [Tooltip("The list of inference features to initialize")]
        public List<InferenceFeature> inferenceFeatureList;

        /// <summary>
        /// Keeps track of whether to unfold the list of inference feature settings
        /// </summary>
        [HideInInspector]
        public bool showInferenceFeatureSettings = false;

        /// <summary>
        /// Keeps track of whether Intel hardware is present
        /// </summary>
        private bool intelHardware;

        /// <summary>
        /// Check if either the CPU or main GPU is Intel hardware
        /// </summary>
        /// <returns></returns>
        private bool IntelHardwarePresent()
        {
            // Check if either the CPU of GPU is made by Intel
            string processorType = SystemInfo.processorType.ToString();
            string graphicsDeviceName = SystemInfo.graphicsDeviceName.ToString();
            return processorType.Contains("Intel") || graphicsDeviceName.Contains("Intel");
        }

        /// <summary>
        /// Called when the script instance is being loaded
        /// </summary>
        public void Awake()
        {
            #if UNITY_EDITOR

            #else
            string pluginsXmlFileContent = @"<ie>
    <plugins>
        <plugin name=""AUTO"" location=""openvino_auto_plugin.dll"">
            <properties>
                <property key=""MULTI_WORK_MODE_AS_AUTO"" value=""YES""/>
            </properties>
        </plugin>
        <plugin name=""BATCH"" location=""openvino_auto_batch_plugin.dll"">
        </plugin>
        <plugin name=""CPU"" location=""openvino_intel_cpu_plugin.dll"">
        </plugin>
        <plugin name=""GNA"" location=""openvino_intel_gna_plugin.dll"">
        </plugin>
        <plugin name=""GPU"" location=""openvino_intel_gpu_plugin.dll"">
        </plugin>
        <plugin name=""HETERO"" location=""openvino_hetero_plugin.dll"">
        </plugin>
        <plugin name=""MULTI"" location=""openvino_auto_plugin.dll"">
        </plugin>
        <plugin name=""MYRIAD"" location=""openvino_intel_myriad_plugin.dll"">
        </plugin>
        <plugin name=""HDDL"" location=""openvino_intel_hddl_plugin.dll"">
        </plugin>
        <plugin name=""VPUX"" location=""openvino_intel_vpux_plugin.dll"">
        </plugin>
    </plugins>
</ie>";
                

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

            // Initialized the atteched inference features
            InitializeFeatures();
        }

        /// <summary>
        /// Perform the initialization steps
        /// </summary>
        private void InitializeFeatures()
        {
            foreach (InferenceFeature inferenceFeature in inferenceFeatureList)
            {
                // Only initialize OpenVINO inference features when Intel hardware is detected
                if (inferenceFeature is IOpenVINOInferenceFeature && intelHardware == false)
                {
                    inferenceFeature.active = false;
                }
                else
                {
                    //Debug.Log($"Instantiating {inferenceFeature.name}");
                    inferenceFeature.Instantiate();
                    inferenceFeature.InitializeDropdowns();
                    inferenceFeature.Initialize();
                }
            }
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        void Start()
        {
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
        }
        
        /// <summary>
        /// Called when the monobehavior becomes disabled or inactive
        /// </summary>
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
    }
}
