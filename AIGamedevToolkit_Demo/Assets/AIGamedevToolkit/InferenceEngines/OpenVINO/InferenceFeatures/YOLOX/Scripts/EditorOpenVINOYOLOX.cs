using UnityEngine;


namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(InferenceFeatureOpenVINOYOLOX))]
    public class EditorOpenVINOYOLOX : Editor
    {
        public override void OnInspectorGUI()
        {
            InferenceFeatureOpenVINOYOLOX scriptableInferenceFeature = (InferenceFeatureOpenVINOYOLOX)target;

            scriptableInferenceFeature.DrawUI();
        }
    }

#endif
}


