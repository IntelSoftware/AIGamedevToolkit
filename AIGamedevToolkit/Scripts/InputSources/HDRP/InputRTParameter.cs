using System.Collections.Generic;
using UnityEngine;

#if HDPipeline
using UnityEngine.Rendering;
#endif

namespace AIGamedevToolkit
{
    #if HDPipeline
    [System.Serializable]
    public class InputRTParameter : VolumeParameter<List<InputRenderTexture>>
    {
        public InputRTParameter(List<InputRenderTexture> value, bool overrideState = false)
            : base(value, overrideState)
        {

        }
    }
    #endif
}