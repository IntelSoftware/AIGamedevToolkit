using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


namespace AIGamedevToolkit
{
    [CreateAssetMenu]
    [System.Serializable]
    public class InferenceFeatureVision : InferenceFeature
    {
        public InputTexture inputTexture;


        [Tooltip("The target resolution for input images")]
        public Vector2Int targetDims = new Vector2Int(640, 640);


        // The unpadded dimensions of the image being fed to the model
        public Vector2Int imageDims = new Vector2Int(0, 0);

        // Used to scale the input image dimensions while maintaining aspect ratio
        protected float aspectRatioScale;


        public virtual void Inference(RenderTexture renderTexture)
        {

        }


        public virtual void Inference(byte[] inputData)
        {

        }


        /// <summary>
        /// Calculate the dimensions for the input image
        /// </summary>
        /// <param name="newVideo"></param>
        public void InitializeTextures()
        {

            // Adjust the input dimensions to maintain the current aspect ratio
            if (imageDims.x != targetDims.x)
            {
                imageDims.x = targetDims.x;
                aspectRatioScale = (float)inputTexture.renderTexture.height / inputTexture.renderTexture.width;
                imageDims.y = (int)(targetDims.x * aspectRatioScale);
                targetDims.y = imageDims.y;

            }
            if (imageDims.y != targetDims.y)
            {
                imageDims.y = targetDims.y;
                aspectRatioScale = (float)inputTexture.renderTexture.width / inputTexture.renderTexture.height;
                imageDims.x = (int)(targetDims.y * aspectRatioScale);
                targetDims.x = imageDims.x;

            }

            // Update the values for the width and height input fields
            Debug.Log($"Setting Input Dims for {this.name} to W: {imageDims.x} x H: {imageDims.y}");
        }

    }
}


