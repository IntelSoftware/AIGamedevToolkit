using UnityEngine;

namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;
    public class CustomEditorUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="streamingAssetsDir"></param>
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
