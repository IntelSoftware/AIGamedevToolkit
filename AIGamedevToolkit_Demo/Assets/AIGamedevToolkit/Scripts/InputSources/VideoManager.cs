using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


namespace AIGamedevToolkit
{
    public class VideoManager : MonoBehaviour
    {

        [Tooltip("List of available video files")]
        public VideoClip[] videoClips;
        // Names of the available video files 
        public static List<string> videoNames = new List<string>();
        [ListToPopup(typeof(VideoManager), "videoNames")]
        public string Videos;
        [Tooltip("")]
        public InputRenderTexture videoTexture;

        // The dimensions of the current video source
        private Vector2Int videoDims;


        /// <summary>
        /// Called when a model option is selected from the dropdown
        /// </summary>
        public void UpdateVideo()
        {
            if (gameObject.GetComponent<VideoPlayer>().enabled == false) return;
            if (videoNames.Count <= 0) return;

            // Set Initial video clip
            gameObject.GetComponent<VideoPlayer>().clip = videoClips[videoNames.IndexOf(Videos)];
            // Update the videoDims.y
            videoDims.y = (int)gameObject.GetComponent<VideoPlayer>().height;
            // Update the videoDims.x
            videoDims.x = (int)gameObject.GetComponent<VideoPlayer>().width;

            videoTexture.renderTexture = RenderTexture.GetTemporary(videoDims.x,
                videoDims.y, 24, RenderTextureFormat.ARGB32);
        }
        

        /// <summary>
        /// 
        /// </summary>
        public void InitializeVideoPlayer()
        {
            // Set the render mode for the video player
            gameObject.GetComponent<VideoPlayer>().renderMode = VideoRenderMode.RenderTexture;

            // Use new videoTexture for Video Player
            gameObject.GetComponent<VideoPlayer>().targetTexture = videoTexture.renderTexture;
        }

        /// <summary>
        /// 
        /// </summary>
        public void InitializationSteps()
        {
            //
            UpdateVideo();
            //
            InitializeVideoPlayer();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Awake()
        {
            // Get the names of the video clips
            foreach (VideoClip clip in videoClips) videoNames.Add(clip.name);

            InitializationSteps();
        }
    }

}