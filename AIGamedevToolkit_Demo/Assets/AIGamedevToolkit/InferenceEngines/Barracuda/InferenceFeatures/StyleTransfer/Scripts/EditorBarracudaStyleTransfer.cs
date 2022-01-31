namespace AIGamedevToolkit
{
    // Only draw custom editor when in the Unity Editor
    #if UNITY_EDITOR
    using UnityEditor;

    /// <summary>
    /// A custom editor for InferenceFeatureBarracudaStyleTransfer assets
    /// </summary>
    [CustomEditor(typeof(InferenceFeatureBarracudaStyleTransfer))]
    public class EditorBarracudaStyleTransfer : Editor
    {
        /// <summary>
        /// Draw a custom Inspector GUI for the inference feature
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Get a reference to the associated inference feature
            InferenceFeatureBarracudaStyleTransfer scriptableInferenceFeature = (InferenceFeatureBarracudaStyleTransfer)target;
            // Draw the editor user interface for the inference feature
            scriptableInferenceFeature.DrawUI();
        }
    }
    #endif
}


