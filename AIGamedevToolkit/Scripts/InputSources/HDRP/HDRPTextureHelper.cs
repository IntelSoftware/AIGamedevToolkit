using System;
using System.Collections.Generic;
using UnityEngine;

#if HDPipeline
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

namespace AIGamedevToolkit
{
    #if HDPipeline
    [Serializable, VolumeComponentMenu("Post-processing/Custom/HDRPTextureHelper")]
    public sealed class HDRPTextureHelper : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public InputRTParameter inputTexturesParam = new InputRTParameter(new List<InputRenderTexture>());

        public bool IsActive() => Application.isPlaying;

        public override bool visibleInSceneView => true;

        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

        public RTHandle rtHandle;
        public RenderTexture rTex;

        
        public override void Setup()
        {
            rtHandle = RTHandles.Alloc(
                scaleFactor: Vector2.one,
                filterMode: FilterMode.Point,
                wrapMode: TextureWrapMode.Clamp,
                dimension: TextureDimension.Tex2D
                );


            // Assign a temporary RenderTexture with the new dimensions
            rTex = RenderTexture.GetTemporary(rtHandle.rt.width, rtHandle.rt.height, 24, RenderTextureFormat.ARGBHalf);

        }


        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
        {
            HDUtils.BlitCameraTexture(cmd, source, rtHandle);
            Graphics.Blit(rtHandle.rt, rTex);

            foreach(InputRenderTexture inputTextures in inputTexturesParam.value)
            {
                inputTextures.SetTexture(rTex);
            }

            cmd.Blit(rTex, destination, 0, 0);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            rtHandle.Release();
        }
    }
    #endif
}