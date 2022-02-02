using UnityEngine;

namespace AIGamedevToolkit
{
    #if UNITY_EDITOR
    using UnityEditor;
    public class CustomEditorUtils
    {
        /// <summary>
        /// Copies the OpenVINO model files for a ModelOpenVINO asset to the StreamingAssets folder
        /// </summary>
        /// <param name="model"></param>
        /// <param name="streamingAssetsDir"></param>
        public static void CopyToStreamingAssets(ModelOpenVINO model, string streamingAssetsDir)
        {
            // Create a StreamingAssets folder if one does not already exist
            if (AssetDatabase.IsValidFolder(streamingAssetsDir) == false)
            {
                Debug.Log("Creating StreamingAssets folder.");
                AssetDatabase.CreateFolder("Assets", "StreamingAssets");
            }

            string exportPath = streamingAssetsDir + "/" + model.streamingAssetsPath;

            // Create the specified directory in the StreamingAssets folder if it does not exist
            if (AssetDatabase.IsValidFolder(exportPath) == false)
            {
                AssetDatabase.CreateFolder(streamingAssetsDir, model.streamingAssetsPath);
            }

            string modelPath = model.modelPath;
            string modelDir = modelPath.Substring(0, modelPath.LastIndexOf("/") + 1);
            string fileName = modelPath.Substring(0, modelPath.LastIndexOf("."));
            fileName = fileName.Substring(modelPath.LastIndexOf("/") + 1);

            // Copy model files to StreamingAssets folder
            string xmlFileName = fileName + ".xml";
            AssetDatabase.CopyAsset(modelDir + xmlFileName, exportPath + "/" + xmlFileName);
            string binFileName = fileName + ".bin";
            AssetDatabase.CopyAsset(modelDir + binFileName, exportPath + "/" + binFileName);           
        }
    }
    #endif
}
