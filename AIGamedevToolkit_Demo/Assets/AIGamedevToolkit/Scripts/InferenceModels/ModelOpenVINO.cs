using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace AIGamedevToolkit
{
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Model/OpenVINO")]
    [System.Serializable]
    public class ModelOpenVINO : ScriptableObject
    {
        // Path to the xml file for the OpenVINO model
        public string modelPath = "";
        // Path in the StreamingAssets directory to export the xml and bin files
        // Need to copy the model files to the StreamingAssets folder before building project
        public string streamingAssetsPath = "";
        // Contains a reference to the xml file for the OpenVINO model
        public TextAsset modelFile;

        /// <summary>
        /// Draw a custom editor for the ModelOpenVINO asset
        /// </summary>
        /// <param name="scriptableModel"></param>
        public void DrawUI(ModelOpenVINO scriptableModel)
        {
            // Only execute editor code when in the Unity Editor
            #if UNITY_EDITOR
            TextAsset modelFileAsset = (TextAsset)EditorGUILayout.ObjectField("Model File", scriptableModel.modelFile, typeof(TextAsset), true);

            if (modelFileAsset != null)
            {
                string modelFilePath = AssetDatabase.GetAssetPath(modelFileAsset);
                string fileExtension = modelFilePath.Substring(modelFilePath.Length - 3);

                if (fileExtension != "xml")
                {
                    Debug.Log("Invalid file, XML file required");
                    return;
                }

                if (modelFileAsset != scriptableModel.modelFile)
                {
                    scriptableModel.modelFile = modelFileAsset;
                    scriptableModel.modelPath = GetNewModelPath(modelFilePath);

                }

                EditorGUILayout.TextField(label: "Model Path", scriptableModel.modelPath);
            }


            if (GUILayout.Button("Browse"))
            {
                string searchPath = "Assets/";
                string newModelPath = EditorUtility.OpenFilePanel("Select OpenVINO Model", searchPath, "xml");

                if (newModelPath.Length != 0 && newModelPath != scriptableModel.modelPath)
                {
                    string assetPath = GetNewModelPath(newModelPath);

                    TextAsset newModelFile = (TextAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset));
                    scriptableModel.modelFile = newModelFile;
                    scriptableModel.modelPath = assetPath;
                }
                else
                {
                    Debug.Log($"{scriptableModel.name}: Did not update model path");
                }
            }

            EditorGUILayout.Space();
            string streamingAssetsDir = "Assets/StreamingAssets";
            EditorGUILayout.TextField(label: "Streaming Assets Path", scriptableModel.streamingAssetsPath);
            if (GUILayout.Button("Browse"))
            {

                string streamingAssetsPath = EditorUtility.OpenFolderPanel("Select OpenVINO Model",
                    streamingAssetsDir, scriptableModel.name);
                if (streamingAssetsPath.Length != 0 && streamingAssetsPath != scriptableModel.streamingAssetsPath)
                {
                    int length = streamingAssetsDir.Length + 1;
                    streamingAssetsPath = streamingAssetsPath.Substring(streamingAssetsPath.IndexOf(streamingAssetsDir) + length);
                    scriptableModel.streamingAssetsPath = streamingAssetsPath;
                    AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Copy to StreamingAssets"))
            {
                CustomEditorUtils.CopyToStreamingAssets(scriptableModel, streamingAssetsDir);
            }

            EditorUtility.SetDirty(scriptableModel);
            #endif
        }

        /// <summary>
        /// Trims the full file path down to the relative path for the Unity project
        /// </summary>
        /// <param name="newModelPath"></param>
        /// <returns></returns>
        private string GetNewModelPath(string newModelPath)
        {
            return newModelPath.Substring(newModelPath.IndexOf("Assets"));
        }

    }
}
