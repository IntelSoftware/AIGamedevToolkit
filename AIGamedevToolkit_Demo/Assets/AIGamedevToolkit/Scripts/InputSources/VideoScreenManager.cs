using UnityEngine;


namespace AIGamedevToolkit
{
    public class VideoScreenManager : MonoBehaviour
    {

        [Tooltip("The screen for viewing preprocessed images")]
        public Transform videoScreen;

        public InputRenderTexture inputTexture;

        public Transform targetCamera;

        public Vector2Int videoDims;

        private RenderTexture rTex;


        /// <summary>
        /// Prepares the videoScreen GameObject to display the chosen video source.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mirrorScreen"></param>
        private void InitializeVideoScreen(int width, int height)
        {
            // Apply the new videoTexture to the VideoScreen Gameobject
            videoScreen.gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Texture");
            videoScreen.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", rTex);
            // Adjust the VideoScreen dimensions for the new videoTexture
            videoScreen.localScale = new Vector3(width, height, videoScreen.localScale.z);
            // Adjust the VideoScreen position for the new videoTexture
            videoScreen.position = new Vector3(width / 2, height / 2, 1);
        }


        /// <summary>
        /// Resizes and positions the in-game Camera to accommodate the video dimensions
        /// </summary>
        private void InitializeCamera()
        {

            // Adjust the camera position to account for updates to the VideoScreen
            targetCamera.position = new Vector3(videoDims.x / 2, videoDims.y / 2, -10f);
            // Render objects with no perspective (i.e. 2D)
            targetCamera.GetComponent<Camera>().orthographic = true;
            // Adjust the camera size to account for updates to the VideoScreen
            int orthographicSize;
            if (((float)Screen.width / Screen.height) < ((float)videoDims.x / videoDims.y))
            {
                float scale = ((float)Screen.width / Screen.height) /
                ((float)videoDims.x / videoDims.y);
                orthographicSize = (int)((videoDims.y / 2) / scale);
            }
            else
            {
                orthographicSize = (int)(videoDims.y / 2);
            }

            targetCamera.GetComponent<Camera>().orthographicSize = orthographicSize;
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitializationSteps()
        {
            int width = inputTexture.renderTexture.width;
            int height = inputTexture.renderTexture.height;

            //
            videoDims = new Vector2Int(width, height);

            rTex = RenderTexture.GetTemporary(videoDims.x, videoDims.y, 24, RenderTextureFormat.ARGB32);

            //
            InitializeVideoScreen(videoDims.x, videoDims.y);
            //
            InitializeCamera();
        }


        // Start is called before the first frame update
        void Start()
        {
            InitializationSteps();
        }


        private void Update()
        {
            if (inputTexture.renderTexture.width != videoDims.x || inputTexture.renderTexture.height != videoDims.y)
            {
                InitializationSteps();
            }

            Graphics.Blit(inputTexture.renderTexture, rTex);
        }

        private void OnDisable()
        {
            RenderTexture.ReleaseTemporary(rTex);
        }
    }

}