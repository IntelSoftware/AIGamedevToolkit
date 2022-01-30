
namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(ModelOpenVINO))]
    public class EditorModelOpenVINO : Editor
    {
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


