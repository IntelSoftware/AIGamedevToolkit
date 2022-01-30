using System;
using UnityEngine;

namespace AIGamedevToolkit
{
    [System.Serializable]
    public class ObjectDetectionClassList : ScriptableObject
    {
        public Tuple<string, Color>[] object_classes;


        public virtual void OnEnable()
        {

        }
    }
}

