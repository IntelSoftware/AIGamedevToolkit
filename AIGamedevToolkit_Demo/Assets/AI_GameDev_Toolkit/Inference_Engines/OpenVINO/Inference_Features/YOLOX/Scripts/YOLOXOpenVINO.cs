using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;



#if UNITY_EDITOR
using UnityEditor;
public class YOLOXEditorTools
{

    [MenuItem("Tools/OpenVINO/YOLOX/Refresh Asset Database")]
    static void Refresh()
    {
        AssetDatabase.Refresh();
        Debug.Log("Refreshing Asset Database.");
    }



    [MenuItem("Tools/OpenVINO/YOLOX/Copy Models to StreamingAssets")]
    static void CopyModelsToStreamingAssets()
    {
        string toolKitDir = "Assets/AI_GameDev_Toolkit/";
        string openVINODir = "Inference_Engines/OpenVINO/";
        string yoloxDir = "Inference_Features/YOLOX/";
        string modelsDir = "YOLOX_Models";
        string sourcePath = toolKitDir+openVINODir+yoloxDir+modelsDir;
        string streamingAssetsPath = "Assets/StreamingAssets/YOLOX_Models";


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

    // 
    private string modelsDir = "YOLOX_Models";


    private float nmsThreshold = 0.45f;
    private float confidenceThreshold = 0.3f;

    // 
    private Vector2Int inputDims;

    // 
    private int deviceIndex;

    // Stores information about detected obejcts
    public YOLOXUtils.Object[] objectInfoArray = new YOLOXUtils.Object[0];


    // Parsed list of compute devices for OpenVINO
    private List<string> deviceList = new List<string>();
    
    // File paths for the OpenVINO IR models
    private List<string> openVINOPaths = new List<string>();
    
    // Names of the OpenVINO IR model
    private List<string> openvinoModels = new List<string>();


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

        // Get the list of available models
        // Get the subdirectories containing the available models
        string[] modelDirs;

        #if UNITY_EDITOR
        string toolKitDir = "Assets/AI_GameDev_Toolkit/";
        string openVINODir = "Inference_Engines/OpenVINO/";
        string yoloxDir = "Inference_Features/YOLOX/";
        string sourcePath = toolKitDir + openVINODir + yoloxDir + modelsDir;
        modelDirs = Directory.GetDirectories(sourcePath);
        #else
        modelDirs = Directory.GetDirectories(Application.streamingAssetsPath + "/"  + modelsDir);
        #endif


        // Get the model files in each subdirectory
        List<string> openVINOFiles = new List<string>();
        foreach (string dir in modelDirs)
        {
            openVINOFiles.AddRange(Directory.GetFiles(dir));
        }

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
            // Perform inference
            PerformInference((IntPtr)p);
        }

        // Get the number of detected objects
        int numObjects = GetObjectCount();
        // Initialize the array
        objectInfoArray = new YOLOXUtils.Object[numObjects];

        // Pin memory
        fixed (YOLOXUtils.Object* o = objectInfoArray)
        {
            // Get the detected objects
            PopulateObjectsArray((IntPtr)o);
        }
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
            float scale = (float)minDimension / Mathf.Min(inputDims.x, inputDims.y);

            // Flip the bbox coordinates vertically
            objectInfoArray[i].y0 = inputDims.y - objectInfoArray[i].y0;

            objectInfoArray[i].x0 *= scale;
            objectInfoArray[i].y0 *= scale;
            objectInfoArray[i].width *= scale;
            objectInfoArray[i].height *= scale;
        }
    }

    public void CleanUp()
    {
        FreeResources();
    }
}
