using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AIGamedevToolkit
{
    /// <summary>
    /// Displays a list of inference features with available options. The intention is to make this list easily available in different UIs, either standalone, or embedded in another application
    /// </summary>
    public class InferenceFeatureListEditor : MonoBehaviour
    {
        /// <summary>
        /// Color for the "Apply now" action button for the "regular" (bright) themed editor skin
        /// </summary>
        private static Color m_actionButtonColor = new Color(0.4666667f, 0.6666667f, 0.2352941f);
        /// <summary>
        /// Color for the "Apply now" action button for the "pro" (dark) themed editor skin
        /// </summary>
        private static Color m_actionButtonProColor = new Color(0.2117647f, 0.3176471f, 0.09019608f);

        /// <summary>
        /// Color to cache / remember the regular background color of the UI before overriding it temporarily
        /// </summary>
        private static Color m_regularBackgroundColor;

        /// <summary>
        /// GUI Content for the "Apply" button
        /// </summary>
        public static GUIContent m_GCapplyButton = new GUIContent("Apply now", "Apply the selected inference features to the scene.");

        /// <summary>
        /// Loads all inference features available in the project into a cached list. This list can be displayed in an editor GUI with "DisplayFeatureList".
        /// </summary>
        public static void LoadFeatureList()
        {
            // Get all the available inference features in the projects by finding their GUIDs
            string[] allFeatureGUIDs = AssetDatabase.FindAssets("t:InferenceFeature");

            foreach (string guid in allFeatureGUIDs)
            {
                // Load the feature by GUID
                InferenceFeature feature = (InferenceFeature)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(InferenceFeature));
                if (feature != null)
                {
                    InferenceFeature.allFeatures.Add(feature);
                }
            }
        }

        /// <summary>
        /// Refreshes the cached list of Inference Features in the project
        /// </summary>
        public static void RefreshFeatureList()
        {
            InferenceFeature.allFeatures.Clear();
            LoadFeatureList();
        }

        /// <summary>
        /// Displays a list of inference features in an Editor GUI.
        /// </summary>
        /// <param name="displayApplyButton">Whether a button should be displayed that applies the selected setup to the scene.</param>
        public static void DisplayFeatureList(bool displayApplyButton = true)
        {
            if (InferenceFeature.allFeatures.OfType<IOpenVINOInferenceFeature>().Any())
            {
                #if AIGAMEDEV_UNSAFE
                #else
                EditorGUILayout.HelpBox("Unsafe code needs to be enabled for OpenVINO inference. Please enable \"Allow 'unsafe' Code\" in Player settings.", MessageType.Warning);
                #endif
            }

            // Iterate over all the features
            foreach (InferenceFeature feature in InferenceFeature.allFeatures)
            {
                if (feature != null)
                {
                    // Display the feature
                    feature.active = EditorGUILayout.ToggleLeft(feature.name, feature.active);
                    EditorGUI.indentLevel++;
                    feature.optionsVisible = EditorGUILayout.Foldout(feature.optionsVisible, "Options...");
                    if (feature.optionsVisible)
                    {
                        EditorGUI.indentLevel++;
                        feature.DrawUI();
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }
            }
            // Apply button
            if (displayApplyButton)
            {
                m_regularBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = EditorGUIUtility.isProSkin ? m_actionButtonProColor : m_actionButtonColor;
                GUILayout.BeginHorizontal();
                {

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(m_GCapplyButton))
                    {
                        // call the apply to scene function for all active features
                        foreach (InferenceFeature activeFeature in InferenceFeature.allFeatures.Where(x => x.active == true))
                        {
                            activeFeature.ApplyToScene();
                        }
                        // call the remove function for all inactive features
                        foreach (InferenceFeature activeFeature in InferenceFeature.allFeatures.Where(x => x.active == false))
                        {
                            activeFeature.RemoveFromScene();
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUI.backgroundColor = m_regularBackgroundColor;
            }
        }    
    }
}