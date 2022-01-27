using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;


[CreateAssetMenu]
[System.Serializable]
public class InputTexture : ScriptableObject
{
    public RenderTexture renderTexture;
    public InferenceFeatureVision[] inferenceFeatures;

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
}
