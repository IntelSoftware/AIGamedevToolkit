using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;



namespace AIGamedevToolkit
{
    public class StyleTransferOpenVINO
    {
        // Name of the DLL file
        const string dll = "OpenVINO_Style_Transfer_DLL";

        [DllImport(dll)]
        private static extern int FindAvailableDevices();

        [DllImport(dll)]
        private static extern IntPtr GetDeviceName(int index);

        [DllImport(dll)]
        private static extern IntPtr InitOpenVINO(string model, int width, int height, int device);

        [DllImport(dll)]
        private static extern void PerformInference(IntPtr inputData);

        [DllImport(dll)]
        private static extern void FreeResources();

        // 
        private Vector2Int inputDims;

        // 
        private int deviceIndex;

        // Parsed list of compute devices for OpenVINO
        private List<string> deviceList = new List<string>();


        /// <summary>
        /// 
        /// </summary>
        public StyleTransferOpenVINO()
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


        /// <summary>
        /// Pin memory for the input data and send it to OpenVINO for inference
        /// </summary>
        /// <param name="inputData"></param>
        public unsafe void UploadTexture(byte[] inputData)
        {
            //Pin Memory
            fixed (byte* p = inputData)
            {
                // Perform inference with OpenVINO
                PerformInference((IntPtr)p);
            }
        }



        public void CleanUp()
        {
            FreeResources();
        }
    }

}