using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


namespace AIGamedevToolkit
{
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Feature/OpenVINO/Base")]
    [System.Serializable]
    public class InferenceFeatureOpenVINO : ScriptableObject
    {
        public ModelOpenVINO modelAsset;


        public void UpdateModel()
        {
            Debug.Log($"{this.name}: Model asset changed");
        }
    }
}
