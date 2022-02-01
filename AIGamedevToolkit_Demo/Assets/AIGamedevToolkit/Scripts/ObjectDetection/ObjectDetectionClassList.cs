using System;
using UnityEngine;

namespace AIGamedevToolkit
{
    /// <summary>
    /// A base for defining lists of object classes for object detection models
    /// </summary>
    [System.Serializable]
    public class ObjectDetectionClassList : ScriptableObject
    {
        public Tuple<string, Color>[] object_classes;


        public virtual void OnEnable()
        {

        }
    }
}

