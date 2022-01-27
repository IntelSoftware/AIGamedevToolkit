using System;
using System.Collections;
using System.Collections.Generic;
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

