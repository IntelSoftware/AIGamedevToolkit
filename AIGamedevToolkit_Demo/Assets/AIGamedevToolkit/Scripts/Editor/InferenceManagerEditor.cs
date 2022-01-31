using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AIGamedevToolkit
{
    /// <summary>
    /// Custom Editor for the Inference Manager - draws the public fields as usual, but then
    /// displays all inference features with their custom options / GUIs.
    /// </summary>
    [CustomEditor(typeof(InferenceManager))]
    public class InferenceManagerEditor : Editor
    {
        public GUIStyle m_headerStyle;

        private void OnEnable()
        {
            InferenceFeatureListEditor.RefreshFeatureList();
        }

        public override void OnInspectorGUI()
        {
            if (m_headerStyle == null)
            {
                m_headerStyle = new GUIStyle(GUI.skin.label);
                m_headerStyle.fontStyle = FontStyle.Bold;
            }

            base.DrawDefaultInspector();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            EditorGUILayout.LabelField("Inference Features", m_headerStyle);
            InferenceFeatureListEditor.DisplayFeatureList(false);
        }
    }
}