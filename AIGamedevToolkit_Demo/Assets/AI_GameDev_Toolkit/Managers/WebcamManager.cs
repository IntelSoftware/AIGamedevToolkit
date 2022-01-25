using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamManager : MonoBehaviour
{

    [Header("Webcam")]
    [Tooltip("Use webcam feed as input")]
    public bool useWebcam = false;
    [Tooltip("The requested webcam dimensions")]
    public Vector2Int webcamDims = new Vector2Int(1280, 720);
    [Tooltip("The requested webcam frame rate")]
    public int webcamFPS = 60;


    public InputTexture videoTexture;


    // Live video input from a webcam
    private WebCamTexture webcamTexture;


    /// <summary>
    /// Try to initialize and start a webcam
    /// </summary>
    private void InitializeWebcam()
    {

        // Create a new WebCamTexture
        webcamTexture = new WebCamTexture(webcamDims.x, webcamDims.y, webcamFPS);

        // Start the Camera
        webcamTexture.Play();

        //if (webcamTexture.width == 16)
        //{
        //    webcamTexture.Stop();
        //    Debug.Log("\nUnable to initialize a webcam.\n");
        //    useWebcam = false;
        //}
        //else
        //{
        //    // Limit application framerate to the target webcam framerate
        //    Application.targetFrameRate = webcamFPS;

        //    // Deactivate the Video Player
        //    videoScreen.GetComponent<VideoPlayer>().enabled = false;

        //    // Update the videoDims.y
        //    videoDims.y = webcamTexture.height;
        //    // Update the videoDims.x
        //    videoDims.x = webcamTexture.width;
        //}

    }

    /// <summary>
    /// Called when the value for the Use Webcam toggle is updated
    /// </summary>
    public void UseWebcam()
    {
        //if (useWebcam)
        //{
        //    WebCamDevice[] devices = WebCamTexture.devices;
        //    for (int i = 0; i < devices.Length; i++)
        //    {
        //        Debug.Log(devices[i].name);
        //    }

        //    if (WebCamTexture.devices.Length == 0)
        //    {
        //        Debug.Log("No webcam device detected.");
        //        useWebcam = false;
        //    }
        //}
        //else
        //{
        //    // Stop the webcam
        //    webcamTexture.Stop();
        //    // Activate the Video Player
        //    videoScreen.GetComponent<VideoPlayer>().enabled = true;
        //}

        //InitializeFeatures();
    }


    // Start is called before the first frame update
    void Start()
    {
        InitializeWebcam();
    }

    // Update is called once per frame
    void Update()
    {
        //if (useWebcam)
        //{
        //    if (webcamTexture != null && webcamTexture.isPlaying)
        //    {
        //        // Copy webcamTexture to videoTexture if using webcam
        //        Graphics.Blit(webcamTexture, videoTexture);
        //    }
        //    else
        //    {
        //        InitializeWebcam();
        //        OnVideoInputChange();
        //        UseWebcam();
        //    }
        //}
        //else if (webcamTexture != null && webcamTexture.isPlaying)
        //{
        //    OnVideoInputChange();
        //    UseWebcam();
        //}
    }
}
