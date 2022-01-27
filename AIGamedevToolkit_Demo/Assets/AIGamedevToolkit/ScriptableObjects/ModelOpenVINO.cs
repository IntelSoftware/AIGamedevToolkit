using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{
    [CreateAssetMenu(menuName = "AIGamedevToolkit/Inference Model/OpenVINO")]
    [System.Serializable]
    public class ModelOpenVINO : ScriptableObject
    {
        //public string modelName = "";
        public string modelPath = "";

        public string streamingAssetsPath = "";

        public TextAsset modelFile;


        public void UpdateModel()
        {
            Debug.Log($"{this.name}: Model path updated to {this.modelPath}");
        }

    }
}
