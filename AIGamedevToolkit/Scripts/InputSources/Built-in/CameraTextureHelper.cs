using UnityEngine;


namespace AIGamedevToolkit
{
    public class CameraTextureHelper : MonoBehaviour
    {

        public InputRenderTexture[] inputTextures;

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            foreach (InputRenderTexture inputTextures in inputTextures)
            {
                inputTextures.SetTexture(source);
            }

            Graphics.Blit(source, destination);

        }
    }

}