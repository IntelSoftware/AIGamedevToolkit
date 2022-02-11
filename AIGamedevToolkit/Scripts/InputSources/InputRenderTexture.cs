using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Input Sources/Render Texture")]
    [System.Serializable]
    public class InputRenderTexture : ScriptableObject
    {
        // Stores a reference to the input render texture
        public RenderTexture renderTexture;
        // A list of inference feature assets take in the render texture as input
        public List<InferenceFeatureVision> inferenceFeatures;

        private void OnEnable()
        {
            renderTexture = RenderTexture.GetTemporary(1920, 1080, 24, RenderTextureFormat.ARGBHalf);
        }

        public void SetTexture(RenderTexture rTex)
        {
            this.renderTexture = rTex;


            foreach (InferenceFeatureVision inferenceFeature in inferenceFeatures)
            {
                if (inferenceFeature.active) inferenceFeature.Inference(rTex);
            }
        }

        private void OnDisable()
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }
}


