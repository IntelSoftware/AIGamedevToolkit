using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;


[CreateAssetMenu]
[System.Serializable]
public class InputTexture : ScriptableObject
{
    //public ComputeShader computeShader;

    public RenderTexture renderTexture;

    public InferenceFeatureVision[] inferenceFeatures;

    //public bool useAsync = false;

    //// Contains the input texture that will be sent to the OpenVINO inference engine
    //private Texture2D texture2D;

    //// Stores the raw pixel data for texture2D
    //private byte[] inputData;

    //private bool requiresGpuToCpu = false;

    //public Vector2Int maxDims;

    
    public void OnEnable()
    {
        
    }

       

    public void SetTextureDims(RenderTexture rTex)
    {
        this.renderTexture = rTex;
    }


    public void SetTexture(RenderTexture rTex)
    {
        this.renderTexture = rTex;


        foreach (InferenceFeatureVision inferenceFeature in inferenceFeatures)
        {
            inferenceFeature.Inference(rTex);
        }
    }


    /// <summary>
    /// Called once AsyncGPUReadback has been completed
    /// </summary>
    /// <param name="request"></param>
    //void OnCompleteReadback(AsyncGPUReadbackRequest request)
    //{
    //    if (request.hasError)
    //    {
    //        Debug.Log("GPU readback error detected.");
    //        return;
    //    }

    //    // Fill Texture2D with raw data from the AsyncGPUReadbackRequest
    //    texture2D.LoadRawTextureData(request.GetData<uint>());
    //    // Apply changes to Textur2D
    //    texture2D.Apply();
    //}






    //public void SetTexture(RenderTexture rTex)
    //{
    //    requiresGpuToCpu = false;

    //    this.renderTexture = rTex;

    //    foreach (InferenceFeatureVision inferenceFeature in inferenceFeatures)
    //    {
    //        if (inferenceFeature is IOpenVINOInferenceFeature && inferenceFeature.active)
    //        {
    //            requiresGpuToCpu = true;

    //            if (maxDims.x < inferenceFeature.imageDims.x && maxDims.y < inferenceFeature.imageDims.y)
    //            {
    //                maxDims = inferenceFeature.imageDims;
    //                Debug.Log($"Max Dims: {maxDims}");
    //            }

    //        }
    //    }

    //    if (requiresGpuToCpu)
    //    {
    //        Destroy(texture2D);
    //        this.texture2D = new Texture2D(maxDims.x, maxDims.y, TextureFormat.RGBA32, false);
    //        RenderTexture tempTex = RenderTexture.GetTemporary(maxDims.x, maxDims.y, 24, renderTexture.format);
    //        Graphics.Blit(rTex, tempTex);

    //        // Flip image before sending to DLL
    //        OpenVINOUtils.FlipImage(computeShader, tempTex, "FlipXAxis");

    //        // Download pixel data from GPU to CPU
    //        if (useAsync)
    //        {
    //            AsyncGPUReadback.Request(tempTex, 0, TextureFormat.RGBA32, OnCompleteReadback);
    //        }
    //        else
    //        {
    //            RenderTexture.active = tempTex;
    //            texture2D.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
    //            texture2D.Apply();
    //        }

    //        inputData = texture2D.GetRawTextureData();


    //        foreach (InferenceFeatureVision inferenceFeature in inferenceFeatures)
    //        {
    //            if (inferenceFeature is IOpenVINOInferenceFeature && inferenceFeature.active)
    //            {
    //                inferenceFeature.Inference(inputData);
    //            }
    //        }


    //        // Load the new image data from the DLL to the texture
    //        texture2D.LoadRawTextureData(inputData);
    //        // Apply the changes to the texture
    //        texture2D.Apply();

    //        Graphics.Blit(texture2D, tempTex);

    //        // Flip image before sending to DLL
    //        OpenVINOUtils.FlipImage(computeShader, tempTex, "FlipXAxis");

    //        Graphics.Blit(tempTex, rTex);

    //        RenderTexture.ReleaseTemporary(tempTex);
    //    }



    //    foreach (InferenceFeatureVision inferenceFeature in inferenceFeatures)
    //    {
    //        if (inferenceFeature is IOpenVINOInferenceFeature == false)
    //        {
    //            inferenceFeature.Inference(rTex);
    //        }
    //    }
    //}
}
