using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{
    #if UNITY_EDITOR
    using UnityEditor;

    /// <summary>
    /// A custom editor for InferenceManager components
    /// </summary>
    [CustomEditor(typeof(InferenceManager))]
    public class EditorInferenceManager : Editor
    {

        private bool unfold = false;
        private string modelInferenceFeatureSettingsLabel = "Inference Feature Settings";

        private List<InferenceFeature> inferenceFeatures;

        /// <summary>
        /// Draw a custom Inspector GUI for the inference manager
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Get a reference to the associated inference manager component
            InferenceManager inferenceManager = (InferenceManager)target;

            if (inferenceFeatures == null)
            {
                inferenceFeatures = inferenceManager.inferenceFeatureList;
            }
            else if (inferenceFeatures != inferenceManager.inferenceFeatureList)
            {
                // Get the list of inference features attached to the inference manager component
                inferenceFeatures = inferenceManager.inferenceFeatureList;
                // Save changes to properties
                EditorUtility.SetDirty(inferenceManager);
            }

            // Draw the default editor user interface for the inference manager
            base.OnInspectorGUI();
            // Draw the custom editors for each attached inference feature in a foldout submenu
            unfold = inferenceManager.showInferenceFeatureSettings;
            unfold = EditorGUILayout.Foldout(unfold, modelInferenceFeatureSettingsLabel);
            if (unfold)
            {
                foreach (InferenceFeature inferenceFeature in inferenceFeatures)
                {
                    EditorGUILayout.LabelField(inferenceFeature.name, EditorStyles.largeLabel);
                    inferenceFeature.DrawUI();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.Space();
                }
            }
            inferenceManager.showInferenceFeatureSettings = unfold;
        }
    }
    #endif
}


