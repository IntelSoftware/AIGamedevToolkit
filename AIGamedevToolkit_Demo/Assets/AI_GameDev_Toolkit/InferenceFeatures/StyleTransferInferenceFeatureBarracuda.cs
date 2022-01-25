using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System;

[CreateAssetMenu]
[System.Serializable]
public class StyleTransferInferenceFeatureBarracuda : InferenceFeatureVision
{

    public ComputeShader computeShader;

    [Header("Style Transfer - Barracuda")]
    [Tooltip("The backend used when performing inference")]
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;
    // 
    public static List<string> deviceList = new List<string>();
    [ListToPopup(typeof(StyleTransferInferenceFeatureBarracuda), "modelList")]
    public string Models = "";
    // 
    public static List<string> modelList = new List<string>();


    public string currentDevice;
    public string currentModel;

    [Tooltip("The model asset file that will be used when performing inference")]
    public NNModel[] modelAssets;

    

    public StyleTransferBarracuda styleTransferBarracuda;
    

    public override void Instantiate()
    {
        styleTransferBarracuda = new StyleTransferBarracuda();
    }


    public override void Initialize()
    {
        styleTransferBarracuda.InitializeEngine(modelAssets[0], workerType);
    }

    public override void InitializeDropdowns()
    {
        currentDevice = WorkerFactory.Type.Auto.ToString("g");
        // Get the names of the model assets
        foreach (NNModel modelAsset in modelAssets) modelList.Add(modelAsset.name);
        
        currentModel = Models;
    }



    /// <summary>
    /// Process the provided image using the specified function on the GPU
    /// </summary>
    /// <param name="image"></param>
    /// <param name="functionName"></param>
    /// <returns>The processed image</returns>
    private void ProcessImage(RenderTexture image, string functionName)
    {
        // Specify the number of threads on the GPU
        int numthreads = 8;
        // Get the index for the specified function in the ComputeShader
        int kernelHandle = computeShader.FindKernel(functionName);
        // Define a temporary HDR RenderTexture
        RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, RenderTextureFormat.ARGBHalf);
        // Enable random write access
        result.enableRandomWrite = true;
        // Create the HDR RenderTexture
        result.Create();

        // Set the value for the Result variable in the ComputeShader
        computeShader.SetTexture(kernelHandle, "Result", result);
        // Set the value for the InputImage variable in the ComputeShader
        computeShader.SetTexture(kernelHandle, "InputImage", image);

        // Execute the ComputeShader
        computeShader.Dispatch(kernelHandle, result.width / numthreads, result.height / numthreads, 1);

        // Copy the result into the source RenderTexture
        Graphics.Blit(result, image);

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(result);
    }



    public override void Inference(RenderTexture renderTexture)
    {
        if (!this.active) return;

        RenderTexture tempTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, renderTexture.format);

        Graphics.Blit(renderTexture, tempTex);
        ProcessImage(tempTex, "ProcessInput");
        styleTransferBarracuda.Exectute(tempTex);
        ProcessImage(tempTex, "ProcessOutput");
        Graphics.Blit(tempTex, renderTexture);
        RenderTexture.ReleaseTemporary(tempTex);
    }


    public override void CleanUp()
    {
        try
        {
            styleTransferBarracuda.CleanUp();
        }
        catch
        {

        }
    }

}
