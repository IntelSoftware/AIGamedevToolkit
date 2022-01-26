using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


[CreateAssetMenu]
[System.Serializable]
public class StyleTransferInferenceFeature : InferenceFeatureVision, IOpenVINOInferenceFeature
{

    public ComputeShader computeShader;

    public StyleTransferOpenVINO styleTransferOpenVINO;

    [Header("Style Transfer - OpenVINO")]
    [ListToPopup(typeof(StyleTransferInferenceFeature), "deviceList")]
    public string Devices = "";
    // 
    public static List<string> deviceList = new List<string>();
    [ListToPopup(typeof(StyleTransferInferenceFeature), "modelList")]
    public string Models = "";
    // 
    public static List<string> modelList = new List<string>();


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
        //targetDims = new Vector2Int(960, 540);
        styleTransferOpenVINO = new StyleTransferOpenVINO();
    }


    public override void Initialize()
    {
        // Set up the neural network for the OpenVINO inference engine
        styleTransferOpenVINO.SetInputDims(this.imageDims);
        if (currentDevice.Length > 0 && currentModel.Length > 0)
        {
            styleTransferOpenVINO.InitializePlugin(modelList.IndexOf(currentModel),
                deviceList.IndexOf(currentDevice));
        }
        else
        {
            styleTransferOpenVINO.InitializePlugin(0, 0);
        }

        // Update inputTex with the new dimensions
        inputTex = new Texture2D(imageDims.x, imageDims.y, TextureFormat.RGBA32, false);
    }


    public override void InitializeDropdowns()
    {
        deviceList = new List<string>();
        modelList = new List<string>();
        deviceList = new List<string>(styleTransferOpenVINO.GetAvailableDevices());
        //Debug.Log($"First device available for {this.name} is {deviceList[0]}");
        currentDevice = deviceList[0];
        modelList = new List<string>(styleTransferOpenVINO.GetAvailableModels());
        currentModel = modelList[0];
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
        styleTransferOpenVINO.UploadTexture(inputData);

        // Load the new image data from the DLL to the texture
        inputTex.LoadRawTextureData(inputData);
        // Apply the changes to the texture
        inputTex.Apply();

        Graphics.Blit(inputTex, tempTex);

        // Flip image before sending to DLL
        OpenVINOUtils.FlipImage(computeShader, tempTex, "FlipXAxis");

        Graphics.Blit(tempTex, renderTexture);

        RenderTexture.ReleaseTemporary(tempTex);
    }


    //public override void Inference(byte[] inputData)
    //{
    //    if (!this.active) return;

    //    // Send reference to inputData to DLL
    //    styleTransferOpenVINO.UploadTexture(inputData);
    //}



    public override void CleanUp()
    {
        try
        {
            styleTransferOpenVINO.CleanUp();
        }
        catch
        {

        }
    }

}
