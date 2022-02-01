using System.Collections.Generic;
using UnityEngine;



namespace AIGamedevToolkit
{
    [System.Serializable]
    public class InferenceFeature : ScriptableObject
    {
        /// <summary>
        /// static list to cache all the existing features from the project into - other classes can read the available inference features from here.
        /// The list is populated from the editor context in InferenceFeatureListEditor, since it requires editor context to find / read all the inference
        /// features as scriptable objects.
        /// </summary>
        public static List<InferenceFeature> allFeatures = new List<InferenceFeature>();

        /// <summary>
        /// Whether this feature should be active in the Inference Manager or not. Active Features are the ones being processed / applied when the scene runs.
        /// </summary>
        public bool active = true;

        /// <summary>
        /// Bool to store whether advanced options are visible for this inference feature.
        /// </summary>
        [HideInInspector]
        public bool optionsVisible = false;

        public virtual void OnEnable()
        {

        }


        public virtual void Instantiate()
        {

        }


        public virtual void Initialize()
        {

        }

        public virtual void InitializeDropdowns()
        {

        }


        public virtual void Inference()
        {

        }


        public virtual void CleanUp()
        {

        }

        /// <summary>
        /// Applies the actual inference to the scene - what this does depends on the implementation details of the inference. All inferences need the Inference Manager 
        /// but some inferences may require additional components being set up beyond the inference manager which then would be added by this method.
        /// </summary>
        public virtual void ApplyToScene()
        {
            Utils.GetOrCreateInferenceManager();
        }
        
        /// <summary>
        /// Removes the actual inference to the scene - this should remove everything applied in "ApplyToScene"
        /// </summary>
        public virtual void RemoveFromScene()
        {
        }

        /// <summary>
        /// Draws Unity Editor inputs so that the user can enter default values for configuration parameters for this inference feature.
        /// </summary>
        public virtual void DrawUI()
        {

        }

    }

}

