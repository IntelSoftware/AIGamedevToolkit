using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;


namespace AIGamedevToolkit
{
    public class YOLOXOpenVINO
    {
        /// <summary>
        /// Name of the DLL file
        /// </summary>
        const string dll = "OpenVINO_YOLOX_DLL";

        /// <summary>
        /// DLL function that determines what compute devices are currently available for OpenVINO
        /// </summary>
        /// <returns>The number of available compute devices</returns>
        [DllImport(dll)]
        private static extern int FindAvailableDevices();

        /// <summary>
        /// DLL function that returns the device name at the speficied index
        /// in the list of available compute devices
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [DllImport(dll)]
        private static extern IntPtr GetDeviceName(int index);

        /// <summary>
        /// DLL function that initializes the OpenVINO inference engine
        /// </summary>
        /// <param name="model">The path to the .xml file for an OpenVINO model</param>
        /// <param name="width">The width value for the model input resolution</param>
        /// <param name="height">The height value for the model input resolution</param>
        /// <param name="device">The index of the target OpenVINO compute device</param>
        /// <returns></returns>
        [DllImport(dll)]
        private static extern IntPtr InitOpenVINO(string model, int width, int height, int device);

        /// <summary>
        /// DLL function that performs inference using the provided pixel data as input
        /// </summary>
        /// <param name="inputData"></param>
        [DllImport(dll)]
        private static extern void PerformInference(IntPtr inputData);

        /// <summary>
        /// DLL function populates an array with the latest object predictions
        /// </summary>
        /// <param name="objects"></param>
        [DllImport(dll)]
        private static extern void PopulateObjectsArray(IntPtr objects);

        /// <summary>
        /// DLL function returns the number of objects detected in the latest input image
        /// </summary>
        /// <returns></returns>
        [DllImport(dll)]
        private static extern int GetObjectCount();

        /// <summary>
        /// DLL function updates the non-maximum suppression threshold value for the plugin
        /// </summary>
        /// <param name="threshold"></param>
        [DllImport(dll)]
        public static extern void SetNMSThreshold(float threshold);

        /// <summary>
        /// DLL function updates the minimum confidence value for the plugin
        /// </summary>
        /// <param name="threshold"></param>
        [DllImport(dll)]
        public static extern void SetConfidenceThreshold(float threshold);

        /// <summary>
        /// DLL function that releases the memory resources allocated by the plugin
        /// </summary>
        [DllImport(dll)]
        public static extern void FreeResources();


        /// <summary>
        /// Keeps track of the current inpute dimensions for the OpenVINO model
        /// </summary>
        private Vector2Int inputDims;

        /// <summary>
        /// Property for getting and setting inputDims
        /// </summary>
        public Vector2Int InputDims
        {
            get => inputDims;
            set
            {
                if ((value.x >= 0) && (value.y > 0))
                {
                    inputDims = value;
                }
            }
        }

        /// <summary>
        /// Keeps track of the current device index
        /// </summary>
        private int deviceIndex;

        /// <summary>
        /// Parsed list of compute devices for OpenVINO
        /// </summary>
        private List<string> deviceList = new List<string>();

        /// <summary>
        /// Propery for getting and setting deviceIndex
        /// </summary>
        public int DeviceIndex
        {
            get => deviceIndex;
            set
            {
                if ((value >= 0) && (value < deviceList.Count))
                {
                    deviceIndex = value;
                }
            }
        }

        /// <summary>
        /// Keeps track of the current non-maximum suppression threshold value for the plugin
        /// </summary>
        private float nmsThreshold = 0.45f;

        /// <summary>
        /// Propery for getting and setting nmsThreshold
        /// </summary>
        public float NMSThreshold
        {
            get => nmsThreshold;
            set
            {
                if ((value >= 0) && (value <= 1))
                {
                    nmsThreshold = value;
                    SetNMSThreshold(value);
                }
            }
        }

        /// <summary>
        /// Keeps track of the current minimum confidence value
        /// </summary>
        private float confidenceThreshold = 0.3f;

        /// <summary>
        /// Propery for getting and setting confidenceThreshold
        /// </summary>
        public float ConfidenceThreshold
        {
            get => confidenceThreshold;
            set
            {
                if ((value >= 0) && (value <= 1))
                {
                    confidenceThreshold = value;
                    SetConfidenceThreshold(value);
                }
            }
        }

        /// <summary>
        /// Constructor for YOLOXOpenVINO
        /// </summary>
        public YOLOXOpenVINO()
        {
            deviceIndex = 0;

            int deviceCount = FindAvailableDevices();
            for (int i = 0; i < deviceCount; i++)
            {
                deviceList.Add(Marshal.PtrToStringAnsi(GetDeviceName(i)));
            }
        }

        /// <summary>
        /// Return the associated device name for the current device index
        /// </summary>
        /// <returns></returns>
        public string GetDeviceName()
        {
            return deviceList[deviceIndex];
        }

        /// <summary>
        /// Return a list containing the names of the available compute devices
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableDevices()
        {
            return deviceList;
        }

        /// <summary>
        /// Initialize the OpenVINO plugin with the specified model, compute device and input dimensions
        /// </summary>
        /// <param name="modelPathIndex"></param>
        /// <param name="deviceIndex"></param>
        public void InitializePlugin(string modelPath, int deviceIndex)
        {
            // Set up the neural network for the OpenVINO inference engine
            string deviceName = Marshal.PtrToStringAnsi(
                InitOpenVINO(modelPath, inputDims.x, inputDims.y, deviceIndex)
                );
            // Update the current compute device index
            this.deviceIndex = deviceList.IndexOf(deviceName);
        }

        #if AIGAMEDEV_UNSAFE
        /// <summary>
        /// Pin memory for the input data and send it to OpenVINO for inference
        /// </summary>
        /// <param name="inputData"></param>
        public unsafe InferenceFeatureObjectDetection2D.Object[] UploadTexture(byte[] inputData)
        {
            //Pin Memory
            fixed (byte* p = inputData)
            {
                // Perform inference
                PerformInference((IntPtr)p);
            }

            // Get the number of detected objects
            int numObjects = GetObjectCount();

            // Initialize the array
            InferenceFeatureObjectDetection2D.Object[] objectInfoArray = new InferenceFeatureObjectDetection2D.Object[numObjects];

            // Pin memory
            fixed (InferenceFeatureObjectDetection2D.Object* o = objectInfoArray)
            {
                // Get the detected objects
                PopulateObjectsArray((IntPtr)o);
            }

            return objectInfoArray;
        }
        #endif

        /// <summary>
        /// Perform any required cleanup steps
        /// </summary>
        public void CleanUp()
        {
            FreeResources();
        }
    }

}