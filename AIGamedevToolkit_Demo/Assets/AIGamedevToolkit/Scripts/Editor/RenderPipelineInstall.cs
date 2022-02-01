using System;
using UnityEditor;
using UnityEngine;

namespace AIGamedevToolkit
{
    public class RenderPipelineInstall
    {
        [MenuItem("Window/AI Gamedev Toolkit/Render Pipeline Setup...", false)]
        public static void CheckRenderPipelineInstallation()
        {
            RenderPipeline currentRenderPipeline = Utils.GetActivePipeline();
            RenderPipeline installedPipeline = RenderPipeline.BuiltIn;
            #if HDPipeline
            installedPipeline = RenderPipeline.HighDefinition;
            #elif UPPipeline
            installedPipeline = RenderPipeline.Universal;
            #endif
            if (currentRenderPipeline != installedPipeline)
            {
                string message = "It looks like the AI GameDev Toolkit is not set up for the current render pipeline yet.\r\n\r\n" +
                                 "Detected render pipeline in use:\r\n" +
                                 $"{currentRenderPipeline}\r\n\r\n" +
                                 "Installed render pipeline:\r\n" +
                                 $"{installedPipeline}\r\n\r\n" +
                                 $"Do you want to setup the toolkit for the {currentRenderPipeline} pipeline now?";



                if (EditorUtility.DisplayDialog("Pipeline Setup Required", message, "Yes", "No, Cancel"))
                {
                    try
                    {
                        Utils.SetScriptingDefines(currentRenderPipeline);
                        EditorUtility.DisplayDialog("Pipeline Setup OK", "The pipeline setup was successfull, the project will recompile with the new settings when you click 'OK'.", "OK");
                    }
                    catch (Exception ex)
                    {
                        EditorUtility.DisplayDialog("Error During Pipeline Setup", $"The pipeline setup ran into an error: {ex.Message}", "OK");
                        Debug.LogError($"Error During Pipeline Setup, Exception: {ex.Message}, Stack Trace: {ex.StackTrace}");
                    }


                }
            }
            else
            {
                string message = "The AI GameDev Toolkit is correctly set up for the used render pipeline.\r\n\r\n" +
                               "Detected render pipeline in use:\r\n" +
                               $"{currentRenderPipeline}\r\n\r\n";
                EditorUtility.DisplayDialog("Pipeline Setup OK", message, "OK");
            }
        }
    }
}