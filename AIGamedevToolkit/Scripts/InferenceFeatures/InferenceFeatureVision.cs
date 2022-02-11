using UnityEngine;


namespace AIGamedevToolkit
{
    [System.Serializable]
    public class InferenceFeatureVision : InferenceFeature
    {
        public InputRenderTexture inputTexture;


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

        public override void ApplyToScene()
        {
            base.ApplyToScene();
            Utils.AddTextureHelper();
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
        }

    }
}


