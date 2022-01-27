using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;
    public class InferenceModelEditorUtils
    {
        public static void DrawModelOpenVINO(ModelOpenVINO scriptableModel)
        {

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
                if (scriptableModel.modelPath.Length != 0)
                {
                    searchPath = scriptableModel.modelPath.Substring(0, scriptableModel.modelPath.LastIndexOf("/"));
                }

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
                    streamingAssetsPath = streamingAssetsPath.Substring(streamingAssetsPath.IndexOf(streamingAssetsDir)+length);
                    scriptableModel.streamingAssetsPath = streamingAssetsPath;
                    AssetDatabase.Refresh();
                }                    
            }

            if (GUILayout.Button("Copy to StreamingAssets"))
            {
                CopyToStreamingAssets(scriptableModel, streamingAssetsDir);
            }
        }


        public static string GetNewModelPath(string newModelPath)
        {
            return newModelPath.Substring(newModelPath.IndexOf("Assets"));
        }

        public static void CopyToStreamingAssets(ModelOpenVINO model, string streamingAssetsDir)
        {
            if (AssetDatabase.IsValidFolder(streamingAssetsDir) == false)
            {
                Debug.Log("Creating StreamingAssets folder.");
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }

            string exportPath = streamingAssetsDir + "/" + model.streamingAssetsPath;

            if (AssetDatabase.IsValidFolder(exportPath) == false)
            {
                AssetDatabase.CreateFolder(streamingAssetsDir, model.streamingAssetsPath);
            }

            Debug.Log("Copying models folder to StreamingAssets folder.");
            string modelPath = model.modelPath;
            string modelDir = modelPath.Substring(0, modelPath.LastIndexOf("/") + 1);

            string fileName = modelPath.Substring(0, modelPath.LastIndexOf("."));
            fileName = fileName.Substring(modelPath.LastIndexOf("/") + 1);

            string xmlFileName = fileName + ".xml";
            AssetDatabase.CopyAsset(modelDir + xmlFileName, exportPath + "/" + xmlFileName);
            string binFileName = fileName + ".bin";
            AssetDatabase.CopyAsset(modelDir + binFileName, exportPath + "/" + binFileName);           
        }
    }
#endif
}
