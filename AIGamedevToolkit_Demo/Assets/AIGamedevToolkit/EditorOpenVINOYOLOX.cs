using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(InferenceFeatureOpenVINOYOLOX))]
    public class EditorOpenVINOYOLOX : Editor
    {
        public TextAsset source;

        public override void OnInspectorGUI()
        {
            InferenceFeatureOpenVINOYOLOX scriptableInferenceFeature = (InferenceFeatureOpenVINOYOLOX)target;

            SerializedProperty m_ActiveProp = serializedObject.FindProperty("active");
            SerializedProperty m_InputTextureProp = serializedObject.FindProperty("inputTexture");
            SerializedProperty m_ModelAssetsProp = serializedObject.FindProperty("modelAssets");
            SerializedProperty m_ClassListProp = serializedObject.FindProperty("classList");
            SerializedProperty m_DisplayBoxesProp = serializedObject.FindProperty("displayBoundingBoxes");
            SerializedProperty m_ComputeShaderProp = serializedObject.FindProperty("computeShader");
            SerializedProperty m_ModelsProp = serializedObject.FindProperty("Models");
            SerializedProperty m_DevicesProp = serializedObject.FindProperty("Devices");
            SerializedProperty m_NmsProp = serializedObject.FindProperty("nmsThreshold");
            SerializedProperty m_MinConfidenceProp = serializedObject.FindProperty("minConfidence");
            SerializedProperty m_TargetDimsProp = serializedObject.FindProperty("targetDims");

            EditorGUILayout.PropertyField(m_ActiveProp, new GUIContent("Active"));
            EditorGUILayout.PropertyField(m_InputTextureProp, new GUIContent("Input Texture"));


            EditorGUILayout.LabelField("Image Processing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ComputeShaderProp, new GUIContent("ComputeShader"));


            EditorGUILayout.LabelField("Model", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ModelAssetsProp, new GUIContent("Model Assets"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("Object Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ClassListProp, new GUIContent("Class List"));
            EditorGUILayout.PropertyField(m_DisplayBoxesProp, new GUIContent("Display Bounding Boxes"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("YOLOX", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ModelsProp, new GUIContent("Models"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                scriptableInferenceFeature.UpdateModel();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_DevicesProp, new GUIContent("Devices"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                scriptableInferenceFeature.UpdateDevice();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_NmsProp, new GUIContent("NMS Threshold"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                scriptableInferenceFeature.UpdateNMSThreshold();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_MinConfidenceProp, new GUIContent("Minimum Confidence"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                scriptableInferenceFeature.UpdateMinConfidence();
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(m_TargetDimsProp, new GUIContent("Input Dimensions"));
            // Apply changes to the serializedProperty
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                scriptableInferenceFeature.UpdateInputDims();
            }


            EditorGUILayout.LabelField("Build Preparation", EditorStyles.boldLabel);
            if (GUILayout.Button("Copy Models to StreamingAssets"))
            {
                string streamingAssetsDir = "Assets/StreamingAssets";
                foreach (ModelOpenVINO modelAsset in scriptableInferenceFeature.modelAssets)
                {
                    InferenceModelEditorUtils.CopyToStreamingAssets(modelAsset, streamingAssetsDir);
                }
            }

        }
    }

#endif
}


