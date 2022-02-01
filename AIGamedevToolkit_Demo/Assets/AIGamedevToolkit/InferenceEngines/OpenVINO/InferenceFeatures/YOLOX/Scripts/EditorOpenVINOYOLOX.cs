namespace AIGamedevToolkit
{
    // Only draw custom editor when in the Unity Editor
    #if UNITY_EDITOR
    using UnityEditor;

    /// <summary>
    /// A custom editor for InferenceFeatureOpenVINOYOLOX assets
    /// </summary>
    [CustomEditor(typeof(InferenceFeatureOpenVINOYOLOX))]
    public class EditorOpenVINOYOLOX : Editor
    {
        /// <summary>
        /// Draw a custom Inspector GUI for the inference feature
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Get a reference to the associated inference feature
            InferenceFeatureOpenVINOYOLOX scriptableInferenceFeature = (InferenceFeatureOpenVINOYOLOX)target;
            // Draw the editor user interface for the inference feature
            scriptableInferenceFeature.DrawUI();
        }
    }
    #endif
}


