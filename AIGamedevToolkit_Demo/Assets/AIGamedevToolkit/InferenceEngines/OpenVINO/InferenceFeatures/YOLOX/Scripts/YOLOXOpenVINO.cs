using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;


namespace AIGamedevToolkit
{
    public class YOLOXOpenVINO
    {
        // Name of the DLL file
        const string dll = "OpenVINO_YOLOX_DLL";

        [DllImport(dll)]
        private static extern int FindAvailableDevices();

        [DllImport(dll)]
        private static extern IntPtr GetDeviceName(int index);

        [DllImport(dll)]
        private static extern IntPtr InitOpenVINO(string model, int width, int height, int device);

        [DllImport(dll)]
        private static extern void PerformInference(IntPtr inputData);

        [DllImport(dll)]
        private static extern void PopulateObjectsArray(IntPtr objects);

        [DllImport(dll)]
        private static extern int GetObjectCount();

        [DllImport(dll)]
        public static extern void SetNMSThreshold(float threshold);

        [DllImport(dll)]
        public static extern void SetConfidenceThreshold(float threshold);

        [DllImport(dll)]
        public static extern void FreeResources();

        
        private float nmsThreshold = 0.45f;
        private float confidenceThreshold = 0.3f;

        // 
        private Vector2Int inputDims;

        // 
        private int deviceIndex;

        // Parsed list of compute devices for OpenVINO
        private List<string> deviceList = new List<string>();


        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <returns></returns>
        public float GetNMSThreshold()
        {
            return this.nmsThreshold;
        }


        public float GetConfThreshold()
        {
            return this.confidenceThreshold;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetDeviceName()
        {
            return deviceList[deviceIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] GetAvailableDevices()
        {
            return deviceList.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector2Int GetInputDims()
        {
            return inputDims;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetDeviceIndex()
        {
            return this.deviceIndex;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="threshold"></param>
        public void SetInstanceNMSThreshold(float threshold)
        {
            SetNMSThreshold(threshold);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threshold"></param>
        public void SetInstanceConfidenceThreshold(float threshold)
        {
            SetConfidenceThreshold(threshold);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceIndex"></param>
        public void SetDeviceIndex(int deviceIndex)
        {
            this.deviceIndex = deviceIndex;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="newDims"></param>
        public void SetInputDims(Vector2Int newDims)
        {
            //Debug.Log($"New Dims: {newDims}");
            inputDims = newDims;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelPathIndex"></param>
        /// <param name="deviceIndex"></param>
        public void InitializePlugin(string modelPath, int deviceIndex)
        {
            // Set up the neural network for the OpenVINO inference engine
            string deviceName = Marshal.PtrToStringAnsi(
                InitOpenVINO(modelPath, inputDims.x, inputDims.y, deviceIndex)
                );
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

        public void CleanUp()
        {
            FreeResources();
        }
    }

}