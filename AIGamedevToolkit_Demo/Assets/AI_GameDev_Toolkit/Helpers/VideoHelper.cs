using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoHelper : MonoBehaviour
{

    public InputTexture[] inputTextures;
    private VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = gameObject.GetComponent<VideoPlayer>();


        //foreach (InputTexture inputTextures in inputTextures)
        //{
        //    inputTextures.SetTexture(videoPlayer.targetTexture);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Video Player Clip: {videoPlayer.clip.name}");

        foreach (InputTexture inputTextures in inputTextures)
        {
            inputTextures.SetTexture(videoPlayer.targetTexture);
        }
    }
}
