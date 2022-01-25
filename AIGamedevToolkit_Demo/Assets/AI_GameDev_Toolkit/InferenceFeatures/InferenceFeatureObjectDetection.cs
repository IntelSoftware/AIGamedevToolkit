using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


[CreateAssetMenu]
[System.Serializable]
public class InferenceFeatureObjectDetection : InferenceFeatureVision
{
    public bool displayBoundingBoxes = true;

}
