

namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(VideoManager))]
    public class EditorVideoManager : Editor
    {
        public override void OnInspectorGUI()
        {
            VideoManager videoManager = (VideoManager)target;

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                videoManager.InitializationSteps();
            }
        }
    }

#endif
}


