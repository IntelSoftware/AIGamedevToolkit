using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



namespace AIGamedevToolkit
{
#if UNITY_EDITOR
    using UnityEditor;
    public class EditorTools
    {

        [MenuItem("Tools/OpenVINO/Refresh")]
        static void Refresh()
        {
            AssetDatabase.Refresh();
            Debug.Log("Refreshing Asset Database.");
        }


        [MenuItem("Tools/OpenVINO/Copy to StreamingAssets")]
        static void CopyToStreamingAssets()
        {
            string toolKitDir = "Assets/AIGamedevToolkit/";
            string openVINODir = "Inference_Engines/OpenVINO/";
            string pluginsDir = "Plugins/x86_64/";
            string sourcePath = toolKitDir + openVINODir + pluginsDir + "plugins.xml";
            string targetPath = "Assets/StreamingAssets/plugins.xml";
            AssetDatabase.CopyAsset(sourcePath, targetPath);
        }

    }
#endif
}






