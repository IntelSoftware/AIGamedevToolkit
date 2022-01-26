using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
[System.Serializable]
public class YOLOXInferenceFeature : InferenceFeatureObjectDetection, IOpenVINOInferenceFeature
{

    public ComputeShader computeShader;

    public YOLOXOpenVINO yoloxOpenVINO;


    [Header("YOLOX")]
    [ListToPopup(typeof(YOLOXInferenceFeature), "deviceList")]
    public string Devices = "";
    // 
    public static List<string> deviceList = new List<string>();
    [ListToPopup(typeof(YOLOXInferenceFeature), "modelList")]
    public string Models = "";
    // 
    public static List<string> modelList = new List<string>();
    [Tooltip("The Non-maximum supression threshold")]
    [Range(0, 1.0f)]
    public float nmsThreshold = 0.45f;

    [Tooltip("The minimum confidence score needed to keep a model prediction")]
    [Range(0, 1.0f)]
    public float minConfidence = 0.3f;


    public string currentDevice;
    public string currentModel;

    //public bool requiresGpuToCpu = true;
    //public bool RequiresGpuToCpu
    //{
    //    get
    //    {
    //        return requiresGpuToCpu;
    //    }
    //    set
    //    {
    //        requiresGpuToCpu = value;
    //    }
    //}


    // Contains the input texture that will be sent to the OpenVINO inference engine
    private Texture2D inputTex;

    // Stores the raw pixel data for inputTex
    private byte[] inputData;


    public override void Instantiate()
    {
        yoloxOpenVINO = new YOLOXOpenVINO();

        //this.imageDims = new Vector2Int(640, 360);
    }


    public override void Initialize()
    {
        // Set up the neural network for the OpenVINO inference engine
        yoloxOpenVINO.SetInputDims(this.imageDims);
        if (currentDevice.Length > 0 && currentModel.Length > 0)
        {
            yoloxOpenVINO.InitializePlugin(modelList.IndexOf(currentModel),
                deviceList.IndexOf(currentDevice));
        }
        else
        {
            yoloxOpenVINO.InitializePlugin(0, 0);
        }

        // Update inputTex with the new dimensions
        inputTex = new Texture2D(imageDims.x, imageDims.y, TextureFormat.RGBA32, false);
    }


    public override void InitializeDropdowns()
    {
        Devices = "";
        Models = "";
        currentDevice = "";
        currentModel = "";
        deviceList = new List<string>(yoloxOpenVINO.GetAvailableDevices());
        //Debug.Log($"First device available for {this.name} is {deviceList[0]}");
        currentDevice = Devices;
        modelList = new List<string>(yoloxOpenVINO.GetAvailableModels());
        currentModel = Models;
    }


    public override void Inference(RenderTexture renderTexture)
    {
        if (!this.active) return;

        RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);

        Graphics.Blit(renderTexture, tempTex);


        // Flip image before sending to DLL
        OpenVINOUtils.FlipImage(computeShader, tempTex, "FlipXAxis");

        // Download pixel data from GPU to CPU
        //if (useAsync)
        //{
        //    AsyncGPUReadback.Request(rTex, 0, TextureFormat.RGBA32, OnCompleteReadback);
        //}
        //else
        //{
        //    RenderTexture.active = rTex;
        //    inputTex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        //    inputTex.Apply();
        //}
        RenderTexture.active = tempTex;
        inputTex.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
        inputTex.Apply();


        inputData = inputTex.GetRawTextureData();

        // Send reference to inputData to DLL
        yoloxOpenVINO.UploadTexture(inputData);

        // Update bounding boxes with new object info
        yoloxOpenVINO.UpdateObjectInfo();

        RenderTexture.ReleaseTemporary(tempTex);
    }


    //public override void Inference(byte[] inputData)
    //{
    //    if (!this.active) return;

        
    //    // Send reference to inputData to DLL
    //    yoloxOpenVINO.UploadTexture(inputData);

    //    // Update bounding boxes with new object info
    //    yoloxOpenVINO.UpdateObjectInfo();       
    //}



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


}
