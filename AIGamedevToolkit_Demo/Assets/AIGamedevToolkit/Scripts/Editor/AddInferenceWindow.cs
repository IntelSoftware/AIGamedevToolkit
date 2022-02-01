using UnityEditor;
using UnityEngine;

namespace AIGamedevToolkit
{
    public class AddInferenceWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        
        [MenuItem("Window/AI Gamedev Toolkit/Add Inference Features", false)]
        public static void ShowWindow()
        {
            var inferenceWindow = EditorWindow.GetWindow<AddInferenceWindow>(false, "AI Gamedev Toolkit");
            if (inferenceWindow != null)
            {
                Vector2 initialSize = new Vector2(500f, 450f);
                inferenceWindow.position = new Rect(new Vector2(Screen.currentResolution.width / 2f - initialSize.x / 2f, Screen.currentResolution.height / 2f - initialSize.y / 2f), initialSize);
                inferenceWindow.Show();
            }
        }

        private void OnEnable()
        {
            InferenceFeatureListEditor.RefreshFeatureList();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            InferenceFeatureListEditor.DisplayFeatureList();
            EditorGUILayout.EndScrollView();
        }
    }
}
