using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(ModelOpenVINO))]
    public class ModelOpenVINOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            ModelOpenVINO scriptableModel = (ModelOpenVINO)target;

            //EditorGUI.BeginChangeCheck();

            InferenceModelEditorUtils.DrawModelOpenVINO(scriptableModel);
            
            //if (EditorGUI.EndChangeCheck())
            //{
            //    scriptableModel.UpdateModel();
            //}
        }

    }
#endif
}


