using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(InferenceManager))]
    public class EditorInferenceManager : Editor
    {

        private bool unfold = false;
        private string modelInferenceFeatureSettingsLabel = "Inference Feature Settings";


        public override void OnInspectorGUI()
        {
            InferenceManager inferenceManager = (InferenceManager)target;

            List<InferenceFeature> inferenceFeatures = inferenceManager.inferenceFeatureList;

            base.OnInspectorGUI();

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


