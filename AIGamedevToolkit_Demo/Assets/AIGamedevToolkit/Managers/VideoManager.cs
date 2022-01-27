using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


namespace AIGamedevToolkit
{
    public class VideoManager : MonoBehaviour
    {

        [Header("Video")]

        //[Tooltip("List of available video files")]
        public VideoClip[] videoClips;
        // Names of the available video files 
        public static List<string> videoNames = new List<string>();
        [ListToPopup(typeof(VideoManager), "videoNames")]
        public string Videos;

        public InputTexture videoTexture;

        private string currentVideo;

        // The dimensions of the current video source
        private Vector2Int videoDims;



        /// <summary>
        /// Called when a model option is selected from the dropdown
        /// </summary>
        public void UpdateVideo()
        {
            if (gameObject.GetComponent<VideoPlayer>().enabled == false) return;

            currentVideo = Videos;

            // Set Initial video clip
            gameObject.GetComponent<VideoPlayer>().clip = videoClips[videoNames.IndexOf(currentVideo)];
            // Update the videoDims.y
            videoDims.y = (int)gameObject.GetComponent<VideoPlayer>().height;
            // Update the videoDims.x
            videoDims.x = (int)gameObject.GetComponent<VideoPlayer>().width;

            //Debug.Log($"Selected Video: {videoNames.IndexOf(currentVideo)}");

        }


        //public void OnVideoInputChange()
        //{
        //    // Create a new videoTexture using the current video dimensions
        //    videoTexture = RenderTexture.GetTemporary(videoDims.x, videoDims.y, 24, RenderTextureFormat.ARGB32);

        //    // Initialize the videoScreen
        //    InitializeVideoScreen(videoDims.x, videoDims.y);
        //    // Adjust the camera based on the source video dimensions
        //    InitializeCamera();
        //}


        public void InitializeVideoPlayer()
        {
            // Set the render mode for the video player
            gameObject.GetComponent<VideoPlayer>().renderMode = VideoRenderMode.RenderTexture;

            // Use new videoTexture for Video Player
            gameObject.GetComponent<VideoPlayer>().targetTexture = videoTexture.renderTexture;
        }


        public void Awake()
        {


            // Get the names of the video clips
            foreach (VideoClip clip in videoClips) videoNames.Add(clip.name);

            UpdateVideo();

            // Create a new videoTexture using the current video dimensions
            videoTexture.renderTexture = RenderTexture.GetTemporary(videoDims.x, videoDims.y, 24, RenderTextureFormat.ARGB32);

            InitializeVideoPlayer();
        }



        // Start is called before the first frame update
        void Start()
        {



        }

        // Update is called once per frame
        void Update()
        {
            //if (currentVideo != Videos)
            //{
            //    UpdateVideo();
            //    OnVideoInputChange();
            //    if (performInference)
            //    {
            //        //InitializeFeatures();
            //    }
            //}
        }
    }

}