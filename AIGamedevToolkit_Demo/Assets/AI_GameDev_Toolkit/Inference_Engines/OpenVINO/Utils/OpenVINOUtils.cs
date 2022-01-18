using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenVINOUtils
{
    public static bool IntelHardwarePresent()
    {
        // Check if either the CPU of GPU is made by Intel
        string processorType = SystemInfo.processorType.ToString();
        string graphicsDeviceName = SystemInfo.graphicsDeviceName.ToString();
        return processorType.Contains("Intel") || graphicsDeviceName.Contains("Intel");
    }



    /// <summary>
    /// Perform a flip operation of the GPU
    /// </summary>
    /// <param name="image">The image to be flipped</param>
    /// <param name="tempTex">Stores the flipped image</param>
    /// <param name="functionName">The name of the function to execute in the compute shader</param>
    public static void FlipImage(ComputeShader computeShader, RenderTexture image, string functionName)
    {
        // Specify the number of threads on the GPU
        int numthreads = 4;
        // Get the index for the PreprocessResNet function in the ComputeShader
        int kernelHandle = computeShader.FindKernel(functionName);

        /// Allocate a temporary RenderTexture
        RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, image.format);
        // Enable random write access
        result.enableRandomWrite = true;
        // Create the RenderTexture
        result.Create();

        // Set the value for the Result variable in the ComputeShader
        computeShader.SetTexture(kernelHandle, "Result", result);
        // Set the value for the InputImage variable in the ComputeShader
        computeShader.SetTexture(kernelHandle, "InputImage", image);
        // Set the value for the height variable in the ComputeShader
        computeShader.SetInt("height", image.height);
        // Set the value for the width variable in the ComputeShader
        computeShader.SetInt("width", image.width);

        // Execute the ComputeShader
        computeShader.Dispatch(kernelHandle, image.width / numthreads, image.height / numthreads, 1);

        // Copy the flipped image to tempTex
        Graphics.Blit(result, image);

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(result);
    }
}
