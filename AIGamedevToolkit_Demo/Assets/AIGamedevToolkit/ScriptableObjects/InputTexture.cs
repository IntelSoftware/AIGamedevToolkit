using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;


namespace AIGamedevToolkit
{
    [CreateAssetMenu]
    [System.Serializable]
    public class InputTexture : ScriptableObject
    {
        public RenderTexture renderTexture;
        public InferenceFeatureVision[] inferenceFeatures;

        private void OnEnable()
        {
            renderTexture = RenderTexture.GetTemporary(1920, 1080, 24, RenderTextureFormat.ARGB32);
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

        private void OnDisable()
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }
}


