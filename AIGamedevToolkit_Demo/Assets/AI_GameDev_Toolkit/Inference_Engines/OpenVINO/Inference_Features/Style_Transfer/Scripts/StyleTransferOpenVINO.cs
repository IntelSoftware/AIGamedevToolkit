using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
public class StyleTransferEditorTools
{

    [MenuItem("Tools/OpenVINO/Style Transfer/Refresh Asset Database")]
    static void Refresh()
    {
        AssetDatabase.Refresh();
        Debug.Log("Refreshing Asset Database.");
    }



    [MenuItem("Tools/OpenVINO/Style Transfer/Copy Models to StreamingAssets")]
    static void CopyModelsToStreamingAssets()
    {
        string toolKitDir = "Assets/AI_GameDev_Toolkit/";
        string openVINODir = "Inference_Engines/OpenVINO/";
        string styleTransferDir = "Inference_Features/Style_Transfer/";
        string modelsDir = "Style_Transfer_Models";
        string sourcePath = toolKitDir + openVINODir + styleTransferDir + modelsDir;
        string streamingAssetsPath = "Assets/StreamingAssets/Style_Transfer_Models";


        if (AssetDatabase.IsValidFolder("Assets/StreamingAssets") == false)
        {
            Debug.Log("Creating StreamingAssets folder.");
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
        }

        if (AssetDatabase.IsValidFolder(streamingAssetsPath) == false)
        {
            Debug.Log("Copying models folder to StreamingAssets folder.");
            bool success = AssetDatabase.CopyAsset(sourcePath, streamingAssetsPath);
            Debug.Log(success);
        }
        else
        {
            Debug.Log("models folder already exists in StreamingAssets folder");
        }
    }
}
#endif



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
    private string modelsDir = "Style_Transfer_Models";


    // 
    private Vector2Int inputDims;

    // 
    private int deviceIndex;

    // Parsed list of compute devices for OpenVINO
    private List<string> deviceList = new List<string>();
    
    // File paths for the OpenVINO IR models
    private List<string> openVINOPaths = new List<string>();
    
    // Names of the OpenVINO IR model
    private List<string> openvinoModels = new List<string>();


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

        // Get the list of available models
        List<string> openVINOFiles = new List<string>();

        #if UNITY_EDITOR
        string toolKitDir = "Assets/AI_GameDev_Toolkit/";
        string openVINODir = "Inference_Engines/OpenVINO/";
        string styleTransferDir = "Inference_Features/Style_Transfer/";
        string modelsDir = "Style_Transfer_Models";
        string sourcePath = toolKitDir + openVINODir + styleTransferDir + modelsDir;
        openVINOFiles.AddRange(Directory.GetFiles(sourcePath));
        #else
        openVINOFiles.AddRange(Directory.GetFiles(Application.streamingAssetsPath + "/" + modelsDir));
        #endif

        // Get the paths for the .xml files for each model
        foreach (string file in openVINOFiles)
        {
            if (file.EndsWith(".xml"))
            {
                openVINOPaths.Add(file);
                string modelName = file.Split('\\')[1];
                openvinoModels.Add(modelName.Substring(0, modelName.Length));
            }
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
    public string[] GetAvailableModels()
    {
        return openvinoModels.ToArray();
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
    public void InitializePlugin(int modelPathIndex, int deviceIndex)
    {
        // Set up the neural network for the OpenVINO inference engine
        string deviceName = Marshal.PtrToStringAnsi(
            InitOpenVINO(openVINOPaths[modelPathIndex], inputDims.x, inputDims.y, deviceIndex)
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
