
namespace AIGamedevToolkit
{
    // Only draw custom editor when in the Unity Editor
    #if UNITY_EDITOR
    using UnityEditor;

    /// <summary>
    /// A custom editor for ModelOpenVINO assets
    /// </summary>
    [CustomEditor(typeof(ModelOpenVINO))]
    public class EditorModelOpenVINO : Editor
    {
        /// <summary>
        /// Draw a custom Inspector GUI for the model asset
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Get reference to the ModelOpenVINO .asset
            ModelOpenVINO modelAsset = (ModelOpenVINO)target;
            // Draw custom editor UI for the ModelOpenVINO
            modelAsset.DrawUI(modelAsset);
        }

    }
    #endif
}


