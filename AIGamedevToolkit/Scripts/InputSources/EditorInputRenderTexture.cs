using UnityEngine;


namespace AIGamedevToolkit
{
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(InputRenderTexture))]
    public class EditorInputRenderTexture : Editor
    {
        public override void OnInspectorGUI()
        {
            InputRenderTexture inputRenderTexture = (InputRenderTexture)target;
            List<InferenceFeatureVision> inferenceFeatures = inputRenderTexture.inferenceFeatures;

            base.OnInspectorGUI();

            foreach (InferenceFeatureVision inferenceFeature in inferenceFeatures)
            {
                if (inferenceFeature == null) continue;

                if (inferenceFeature.inputTexture != inputRenderTexture)
                {
                    Debug.Log($"{inferenceFeature.name}: Updating input texture to {inputRenderTexture.name}");
                    inferenceFeature.inputTexture = inputRenderTexture;
                }
            }
        }
    }

#endif
}


