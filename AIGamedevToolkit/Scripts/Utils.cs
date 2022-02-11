using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Rendering;
using System.Collections.Generic;

#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

#if GAIA_2_PRESENT
using Gaia;
#endif


namespace AIGamedevToolkit
{
    /// <summary>
    /// Enumeration to differentiate between the available SRPs
    /// </summary>
    public enum RenderPipeline { BuiltIn, Universal, HighDefinition }

    public class Utils
    {
        // Constants for the render pipeline scripting defines
        public const string BuiltinPipelineDefine = "BuiltinPipeline";
        public const string HDPipelineDefine = "HDPipeline";
        public const string UniversalPipelineDefine = "UPPipeline";


        /// <summary>
        /// Returns the inference manager in the scene or creates it, if it does not exist
        /// </summary>
        /// <returns>The Inference Manager in the scene</returns>
        public static InferenceManager GetOrCreateInferenceManager()
        {
            // Look for the game object
            GameObject infManGO = FindObjectDeactivated("Inference Manager", true);
            // if it does not exist yet, create it
            if (infManGO == null)
            {
                infManGO = new GameObject("Inference Manager");
            }
            // Look for the component
            InferenceManager infMan = infManGO.GetComponent<InferenceManager>();
            // if it does not exist yet, create it
            if (infMan == null)
            {
                infMan = infManGO.AddComponent<InferenceManager>();

            }
            // Refresh the reference list of available inference features
            infMan.inferenceFeatureList = new List<InferenceFeature>();
            foreach (InferenceFeature inferenceFeature in InferenceFeature.allFeatures)
            {
                if (inferenceFeature.active)
                {
                    infMan.inferenceFeatureList.Add(inferenceFeature);
                }
            }

            return infMan;
        }

        public static void AddTextureHelper()
        {
            #if HDPipeline
            string helper_string = "AIGamedevToolkit.HDRPTextureHelper, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

            
            RenderPipelineAsset currentSettings = GraphicsSettings.defaultRenderPipeline;
            
            SerializedObject currentSettingsSO = new SerializedObject(currentSettings);

            //SerializedProperty m_DefaultVolumeProfile = currentSettingsSO.FindProperty("m_DefaultVolumeProfile");
            //VolumeProfile volumeProfile = (VolumeProfile)m_DefaultVolumeProfile.objectReferenceValue;

            //if (!volumeProfile.TryGet<AIGamedevToolkit.HDRPTextureHelper>(out var textureHelper))
            //{
            //    textureHelper = volumeProfile.Add<AIGamedevToolkit.HDRPTextureHelper>(true);
            //    textureHelper.inputTexturesParam.overrideState = true;

            //    string inputTexturePath = GetAssetPath("MainCamera_Texture", "asset");
            //    InputRenderTexture inputTexture = (InputRenderTexture)AssetDatabase.LoadAssetAtPath(inputTexturePath, typeof(InputRenderTexture));

            //    inputTexture.inferenceFeatures.Clear();
            //    foreach (InferenceFeatureVision visionFeature in InferenceFeature.allFeatures)
            //    {
            //        inputTexture.inferenceFeatures.Add(visionFeature);
            //    }

            //    textureHelper.inputTexturesParam.value.Add(inputTexture);
            //}
            //else
            //{
            //    Debug.Log("Volume Component already added to Volume Profile");
            //}
            //m_DefaultVolumeProfile.objectReferenceValue = volumeProfile;
            //EditorUtility.SetDirty(volumeProfile);

            //Volume[] volumes = GameObject.FindObjectsOfType<Volume>();
            //foreach(Volume volume in volumes)
            //{
            //    if(volume.sharedProfile.name == "VolumeGlobal")
            //    {
            //        VolumeProfile profile = volume.sharedProfile;
            //        if (!profile.TryGet<AIGamedevToolkit.HDRPTextureHelper>(out var textureHelper))
            //        {
            //            textureHelper = profile.Add<AIGamedevToolkit.HDRPTextureHelper>(true);
            //            textureHelper.inputTexturesParam.overrideState = true;

            //            string inputTexturePath = GetAssetPath("MainCamera_Texture", "asset");
            //            InputRenderTexture inputTexture = (InputRenderTexture)AssetDatabase.LoadAssetAtPath(inputTexturePath, typeof(InputRenderTexture));

            //            inputTexture.inferenceFeatures.Clear();
            //            foreach (InferenceFeatureVision visionFeature in InferenceFeature.allFeatures)
            //            {
            //                inputTexture.inferenceFeatures.Add(visionFeature);
            //            }

            //            textureHelper.inputTexturesParam.value.Add(inputTexture);

            //        }
            //    }
            //}



            SerializedProperty afterPostProcessCustomPostProcesses = currentSettingsSO.FindProperty("afterPostProcessCustomPostProcesses");

            bool isPresent = false;
            for (int i = 0; i < afterPostProcessCustomPostProcesses.arraySize; i++)
            {
                if (afterPostProcessCustomPostProcesses.GetArrayElementAtIndex(i).stringValue == helper_string)
                {
                    isPresent = true;
                    break;
                }
            }

            if (!isPresent)
            {
                int arraySize = afterPostProcessCustomPostProcesses.arraySize;
                afterPostProcessCustomPostProcesses.InsertArrayElementAtIndex(arraySize);
                afterPostProcessCustomPostProcesses.GetArrayElementAtIndex(arraySize).stringValue = helper_string;
            }
            else
            {
                //Debug.Log("HDRPTextureHelper already added to afterPostProcessCustomPostProcesses");
            }
            EditorUtility.SetDirty(currentSettings);
            currentSettingsSO.ApplyModifiedProperties();
            
            #else
            Camera camera = GetCamera();
            CameraTextureHelper cth = camera.gameObject.GetComponent<CameraTextureHelper>();
            if (cth == null)
            {
                cth = camera.gameObject.AddComponent<CameraTextureHelper>();
            }
            #if UNITY_EDITOR
            // Load the default "MainCamera_Texture" if no other textures are set by the user
            if (cth.inputTextures == null || cth.inputTextures.Length == 0)
            {
                string inputTexturePath = GetAssetPath("MainCamera_Texture", "asset");
                InputRenderTexture inputTexture = (InputRenderTexture)AssetDatabase.LoadAssetAtPath(inputTexturePath, typeof(InputRenderTexture));
                cth.inputTextures = new InputRenderTexture[1] { inputTexture };

                inputTexture.inferenceFeatures.Clear();
                foreach (InferenceFeatureVision visionFeature in InferenceFeature.allFeatures)
                {
                    inputTexture.inferenceFeatures.Add(visionFeature);
                }
            }
            #endif
            #endif
        }

        /// <summary>
        /// Sets the correct scripting defines for the current render pipeline
        /// </summary>
        /// <param name="currentRenderPipeline"></param>
        public static void SetScriptingDefines(RenderPipeline currentRenderPipeline)
        {
            #if UNITY_EDITOR

            bool wasChanged = false;
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            switch (currentRenderPipeline)
            {
                case RenderPipeline.HighDefinition:
                    RemoveDefine(ref currBuildSettings, UniversalPipelineDefine, ref wasChanged);
                    AddDefine(ref currBuildSettings, HDPipelineDefine, ref wasChanged);
                    break;
                case RenderPipeline.Universal:
                    RemoveDefine(ref currBuildSettings, HDPipelineDefine, ref wasChanged);
                    AddDefine(ref currBuildSettings, UniversalPipelineDefine, ref wasChanged);
                    break;
                default:
                    RemoveDefine(ref currBuildSettings, UniversalPipelineDefine, ref wasChanged);
                    RemoveDefine(ref currBuildSettings, HDPipelineDefine, ref wasChanged);
                    AddDefine(ref currBuildSettings, BuiltinPipelineDefine, ref wasChanged);
                    break;
            }
            // Only apply an update if we had changes
            if (wasChanged)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
            }
            #endif
        }

        /// <summary>
        /// Adds a scripting define to the current Build Settings string
        /// </summary>
        /// <param name="currBuildSettings"></param>
        /// <param name="define"></param>
        /// <param name="wasUpdated"></param>
        /// <returns>True, if the define was newly added, false if it already existed.</returns>
        public static bool AddDefine(ref string currBuildSettings, string define, ref bool wasUpdated)
        {
            if (!currBuildSettings.Contains(define))
            {
                if (string.IsNullOrEmpty(currBuildSettings))
                {
                    currBuildSettings = define;
                }
                else
                {
                    currBuildSettings += ";" + define;
                }
                wasUpdated = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a scripting define from the current Build Settings string
        /// </summary>
        /// <param name="currBuildSettings"></param>
        /// <param name="define"></param>
        /// <param name="wasUpdated"></param>
        /// <returns>True, if the define was removed, false if there was nothing found to remove</returns>
        public static bool RemoveDefine(ref string currBuildSettings, string define, ref bool wasUpdated)
        {
            if (currBuildSettings.Contains(define))
            {
                currBuildSettings = currBuildSettings.Replace(define + ";", "");
                currBuildSettings = currBuildSettings.Replace(define, "");
                wasUpdated = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the current installed SRP
        /// </summary>
        /// <returns></returns>
        public static RenderPipeline GetActivePipeline()
        {
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                return RenderPipeline.BuiltIn;
            }
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                return RenderPipeline.HighDefinition;
            }
            else
            {
                return RenderPipeline.Universal;
            }
        }

        /// <summary>
        /// Finds a gameobject by name even when it is deactivated.
        /// </summary>
        /// <param name="searchFor">The name of the GO to look for</param>
        /// <param name="fullNameMatch">Whether the name needs to be a full match or if it is sufficient if the name of the GO just contains "searchFor".</param>
        /// <returns></returns>
        public static GameObject FindObjectDeactivated(string searchFor, bool fullNameMatch = true)
        {
            GameObject[] allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allGOs)
            {
                if (go.name == searchFor || (!fullNameMatch && go.name.Contains(searchFor)))
                {
                    return go;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets and returns the main camera in the scene
        /// If check tag is set to ture it will check to see if the camera found tag is marked as MainCamera or Player
        /// </summary>
        /// <returns></returns>
        public static Camera GetCamera(bool checkTag = false)
        {

            #if GAIA_2_PRESENT
            //if Gaia is present in project, look for cameras Gaia set up itself first, those would be the relevant ones before all others
            foreach (Camera gaiaCam in Resources.FindObjectsOfTypeAll(typeof(Camera)))
            {
                CustomGaiaController customCont = gaiaCam.GetComponent<CustomGaiaController>();
                if (customCont != null)
                {
                    //Found a camera set up by Gaia
                    return gaiaCam;
                }
            }
            #endif

            Camera camera = Camera.main;
            if (camera != null)
            {
                if (checkTag)
                {
                    if (camera.tag == "MainCamera" || camera.tag == "Player")
                    {
                        return camera;
                    }
                }
                return camera;
            }

            camera = GameObject.FindObjectOfType<Camera>();
            if (camera != null)
            {
                if (checkTag)
                {
                    if (camera.tag == "MainCamera" || camera.tag == "Player")
                    {
                        return camera;
                    }
                }

                return camera;
            }

            return null;
        }

        /// <summary>
        /// Expects a full file system path starting at a drive letter, and will return a path starting at the projects / games asset folder
        /// </summary>
        /// <param name="inputPath">Full file system path starting at a drive letter</param>
        /// <returns></returns>
        public static string GetPathStartingAtAssetsFolder(string inputPath)
        {
            return inputPath.Substring(Application.dataPath.Length - "Assets".Length);
        }

        /// <summary>
        /// Return the first prefab that exactly matches the given name from within the current project
        /// </summary>
        /// <param name="name">Asset to search for</param>
        /// <returns>Returns the prefab or null</returns>
        public static GameObject GetAssetPrefab(string name)
        {
            #if UNITY_EDITOR
            string path = GetAssetPath(name, "prefab");
            if (!string.IsNullOrEmpty(path))
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            #endif
            return null;
        }


        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <param name="extension">File Extension / Type to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string name, string extension)
        {
            #if UNITY_EDITOR
            string[] assets = AssetDatabase.FindAssets(name, null);
            string[] file;
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
                //Make sure its an exact match
                file = Path.GetFileName(path).Split('.');
                if (file.GetLength(0) != 2)
                {
                    continue;
                }
                if (file[0] != name)
                {
                    continue;
                }
                if (file[1] != extension)
                {
                    continue;
                }
                return path;
            }
            #endif
            return "";
        }

        /// <summary>
        /// Adds the unity event / input system component to the passed in Game Object if they do not exist yet - otherwise the input on the canvas would not work
        /// </summary>
        /// <param name="gameObject"></param>
        public static void AddEventInputSystem(GameObject gameObject)
        {
            EventSystem[] existingEventSystems = (EventSystem[])Resources.FindObjectsOfTypeAll(typeof(EventSystem));
            if (existingEventSystems == null || existingEventSystems.Where(x => x.transform.parent != null).Count() <= 0)
            {
                gameObject.AddComponent<EventSystem>();
            }

            StandaloneInputModule[] existingInputModules = (StandaloneInputModule[])Resources.FindObjectsOfTypeAll(typeof(StandaloneInputModule));
            if (existingInputModules == null || existingInputModules.Where(x => x.transform.parent != null).Count() <= 0)
            {
                gameObject.AddComponent<StandaloneInputModule>();
            }
        }
    }
}