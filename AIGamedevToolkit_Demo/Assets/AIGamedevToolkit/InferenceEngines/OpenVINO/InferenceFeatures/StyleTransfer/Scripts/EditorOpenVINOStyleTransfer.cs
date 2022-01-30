using UnityEngine;


namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(InferenceFeatureOpenVINOStyleTransfer))]
    public class EditorOpenVINOStyleTransfer : Editor
    {
        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            InferenceFeatureOpenVINOStyleTransfer scriptableInferenceFeature = (InferenceFeatureOpenVINOStyleTransfer)target;

            scriptableInferenceFeature.DrawUI();
        }
    }

#endif
}


