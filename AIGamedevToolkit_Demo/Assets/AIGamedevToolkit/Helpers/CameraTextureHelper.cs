using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AIGamedevToolkit
{
    public class CameraTextureHelper : MonoBehaviour
    {

        public InputTexture[] inputTextures;

        private RenderTexture tempTex;

        private Camera cameraComponent;


        public void OnEnable()
        {
            cameraComponent = gameObject.GetComponent<Camera>();

            tempTex = RenderTexture.GetTemporary(1920, 1080);

            foreach (InputTexture inputTextures in inputTextures)
            {
                inputTextures.SetTextureDims(tempTex);
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            foreach (InputTexture inputTextures in inputTextures)
            {
                inputTextures.SetTexture(source);
            }

            Graphics.Blit(source, destination);

        }

        private void OnDisable()
        {
            RenderTexture.ReleaseTemporary(tempTex);
        }
    }

}