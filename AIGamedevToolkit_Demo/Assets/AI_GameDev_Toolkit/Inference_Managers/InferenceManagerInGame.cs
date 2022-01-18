using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.IO;



public class InferenceManagerInGame : MonoBehaviour
{
    [Header("OpenVINO")]
    [Tooltip("Performs the preprocessing and postprocessing steps")]
    public ComputeShader imageProcessingShader;
        
    [Header("Inference")]
    [Tooltip("Turn stylization on and off")]
    public bool inferenceToggle = true;
    [Tooltip("Turn AsyncGPUReadback on and off")]
    public bool useAsync = false;
    [Tooltip("The targrt resolution for input images")]
    public Vector2Int targetDims = new Vector2Int(640, 640);

    [Header("YOLOX")]
    public bool yoloxActive = true;
    [ListToPopup(typeof(InferenceManagerInGame), "yoloxDeviceList")]
    public string YOLOXDevices = "";
    // 
    public static List<string> yoloxDeviceList = new List<string>();
    [ListToPopup(typeof(InferenceManagerInGame), "yoloxModelList")]
    public string YOLOXModels = "";
    // 
    public static List<string> yoloxModelList = new List<string>();
    [Tooltip("The Non-maximum supression threshold")]
    [Range(0, 1.0f)]
    public float nmsThreshold = 0.45f;

    [Tooltip("The minimum confidence score needed to keep a model prediction")]
    [Range(0, 1.0f)]
    public float minConfidence = 0.3f;



    [Header("Style Transfer - OpenVINO")]
    public bool styleTransferIntelActive = true;
    [ListToPopup(typeof(InferenceManagerInGame), "styleTransferIntelDeviceList")]
    public string styleTransferIntelDevices = "";
    // 
    public static List<string> styleTransferIntelDeviceList = new List<string>();
    [ListToPopup(typeof(InferenceManagerInGame), "styleTransferIntelModelList")]
    public string styleTransferIntelModels = "";
    // 
    public static List<string> styleTransferIntelModelList = new List<string>();


    private string currentVideo;
    private string currentYOLOXDevice;
    private string currentYOLOXModel;


    private string currentStyleTransferIntelDevice;
    private string currentStyleTransferIntelModel;



    // 
    private YOLOXOpenVINO cocoYOLOX;


    private StyleTransferOpenVINO styleTransferOpenVINO;


    // The texture used for rendering the bounding box on screen
    private Texture2D boxTex;
    
    // The unpadded dimensions of the image being fed to the model
    private Vector2Int imageDims = new Vector2Int(0, 0);

    // The texture used to create input tensor
    private RenderTexture rTex;

    // Contains the input texture that will be sent to the OpenVINO inference engine
    private Texture2D inputTex;

    // Keeps track of whether to execute the OpenVINO model
    private bool performInference = true;

    // Used to scale the input image dimensions while maintaining aspect ratio
    private float aspectRatioScale;

    // Stores the raw pixel data for inputTex
    private byte[] inputData;
        
    


    public void Awake()
    {
        #if UNITY_EDITOR_WIN
                return;
        #else

        Debug.Log("Checking for plugins.xml file");

            string sourcePath = $"{Application.streamingAssetsPath}/plugins.xml";
            string targetPath = $"{Application.dataPath}/Plugins/x86_64/plugins.xml";

            if (File.Exists(targetPath))
            {
                Debug.Log("plugins.xml already in folder");
            }
            else
            {
                Debug.Log("Moving plugins.xml file from StreamingAssets to Plugins folder.");
                File.Copy(sourcePath, targetPath);
            }

        #endif
    }


    

    /// <summary>
    /// Calculate the dimensions for the input image
    /// </summary>
    /// <param name="newVideo"></param>
    private void InitializeTextures()
    {
        // Adjust the input dimensions to maintain the current aspect ratio
        if (imageDims.x != targetDims.x)
        {
            imageDims.x = targetDims.x;
            aspectRatioScale = (float)Screen.height / Screen.width;
            imageDims.y = (int)(targetDims.x * aspectRatioScale);
            targetDims.y = imageDims.y;

        }
        if (imageDims.y != targetDims.y)
        {
            imageDims.y = targetDims.y;
            aspectRatioScale = (float)Screen.width / Screen.height;
            imageDims.x = (int)(targetDims.y * aspectRatioScale);
            targetDims.x = imageDims.x;

        }


        // Initialize the RenderTexture that will store the processed input image
        rTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, RenderTextureFormat.ARGB32);
        // Update inputTex with the new dimensions
        inputTex = new Texture2D(imageDims.x, imageDims.y, TextureFormat.RGBA32, false);

        // Update the values for the width and height input fields
        Debug.Log($"Setting Input Dims to W: {imageDims.x} x H: {imageDims.y}");
    }



    /// <summary>
    /// Initialize the options for the dropdown menus
    /// </summary>
    private void InitializeDropdowns()
    {
        yoloxDeviceList = new List<string>(cocoYOLOX.GetAvailableDevices());
        currentYOLOXDevice = YOLOXDevices;
        yoloxModelList = new List<string>(cocoYOLOX.GetAvailableModels());
        currentYOLOXModel = YOLOXModels;

        styleTransferIntelDeviceList = new List<string>(styleTransferOpenVINO.GetAvailableDevices());
        currentStyleTransferIntelDevice = styleTransferIntelDevices;
        styleTransferIntelModelList = new List<string>(styleTransferOpenVINO.GetAvailableModels());
        currentStyleTransferIntelModel = styleTransferIntelModels;
    }



    /// <summary>
    /// Perform the initialization steps required when the model input is updated
    /// </summary>
    private void InitializationSteps()
    {
        // Initialize the textures that store the model input
        InitializeTextures();
        // Set up the neural network for the OpenVINO inference engine
        cocoYOLOX.SetInputDims(imageDims);
        if (currentYOLOXDevice.Length > 0 && currentYOLOXModel.Length > 0)
        {
            cocoYOLOX.InitializePlugin(yoloxModelList.IndexOf(currentYOLOXModel),
                yoloxDeviceList.IndexOf(currentYOLOXDevice));
        }
        else
        {
            cocoYOLOX.InitializePlugin(0, 0);
        }


        styleTransferOpenVINO.SetInputDims(imageDims);
        if (currentStyleTransferIntelDevice.Length > 0 && currentStyleTransferIntelModel.Length > 0)
        {
            styleTransferOpenVINO.InitializePlugin(styleTransferIntelModelList.IndexOf(currentStyleTransferIntelModel),
                styleTransferIntelDeviceList.IndexOf(currentStyleTransferIntelDevice));
        }
        else
        {
            styleTransferOpenVINO.InitializePlugin(0, 0);
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        boxTex = Texture2D.whiteTexture;

        if (OpenVINOUtils.IntelHardwarePresent())
        {
            cocoYOLOX = new YOLOXOpenVINO();
            styleTransferOpenVINO = new StyleTransferOpenVINO();
        }
        else
        {
            inferenceToggle = performInference = false;
            Debug.Log("No Intel hardware detected");
        }

        // Get the names of the video clips
        //foreach (VideoClip clip in videoClips) videoNames.Add(clip.name);

        // Initialize the dropdown menus
        InitializeDropdowns();
        // Perform the requred 
        InitializationSteps();
    }

    /// <summary>
    /// Called once AsyncGPUReadback has been completed
    /// </summary>
    /// <param name="request"></param>
    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        // Fill Texture2D with raw data from the AsyncGPUReadbackRequest
        inputTex.LoadRawTextureData(request.GetData<uint>());
        // Apply changes to Textur2D
        inputTex.Apply();
    }

    
    
    // Update is called once per frame
    void Update()
    {
        // Toggle the user interface
        if (Input.GetKeyDown("space"))
        {
            
        }


        // Prevent the input dimensions from going too low for the model
        targetDims.x = Mathf.Max(targetDims.x, 64);
        targetDims.y = Mathf.Max(targetDims.y, 64);

        if (targetDims != imageDims)
        {
            UpdateInputDims();
        }

        //if (currentVideo != Videos)
        //{
        //    InitializationSteps();
        //}

        // YOLOX
        if (nmsThreshold != cocoYOLOX.GetNMSThreshold())
        {
            cocoYOLOX.SetInstanceNMSThreshold(nmsThreshold);
        }

        if (minConfidence != cocoYOLOX.GetConfThreshold())
        {
            cocoYOLOX.SetInstanceConfidenceThreshold(minConfidence);
        }

        if (performInference != inferenceToggle)
        {
            UpdateInferenceValue();
        }

        if (currentYOLOXDevice != YOLOXDevices)
        {
            currentYOLOXDevice = YOLOXDevices;
            InitializationSteps();
            
        }

        if (currentYOLOXModel != YOLOXModels)
        {
            currentYOLOXModel = YOLOXModels;
            InitializationSteps();
        }

        // Style Transfer
        if (currentStyleTransferIntelDevice != styleTransferIntelDevices)
        {
            currentStyleTransferIntelDevice = styleTransferIntelDevices;
            InitializationSteps();

        }

        if (currentStyleTransferIntelModel != styleTransferIntelModels)
        {
            currentStyleTransferIntelModel = styleTransferIntelModels;
            InitializationSteps();
        }

        
    }


    /// <summary>
    /// Called when the input dimensions are updated in the GUI
    /// </summary>
    public void UpdateInputDims()
    {
        InitializationSteps();
    }


    /// <summary>
    /// Called when the value for the Inference toggle is updated
    /// </summary>
    public void UpdateInferenceValue()
    {
        performInference = inferenceToggle;

        if (performInference)
        {

            InitializationSteps();
        }
    }


    private void OnDisable()
    {
        cocoYOLOX.CleanUp();
    }


    /// <summary>
    /// Called when the Quit button is clicked.
    /// </summary>
    public void Quit()
    {
        // Causes the application to exit
        Application.Quit();
    }


    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (performInference == true)
        {
            // Copy the source to the rTex RenderTexture
            Graphics.Blit(source, rTex);


            // Flip image before sending to DLL
            OpenVINOUtils.FlipImage(imageProcessingShader, rTex, "FlipXAxis");

            // Download pixel data from GPU to CPU
            if (useAsync)
            {
                AsyncGPUReadback.Request(rTex, 0, TextureFormat.RGBA32, OnCompleteReadback);
            }
            else
            {
                RenderTexture.active = rTex;
                inputTex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
                inputTex.Apply();
            }

            inputData = inputTex.GetRawTextureData();

            if (yoloxActive)
            {
                // Send reference to inputData to DLL
                cocoYOLOX.UploadTexture(inputData);

                // Update bounding boxes with new object info
                cocoYOLOX.UpdateObjectInfo();
            }

            if (styleTransferIntelActive)
            {

                // Send reference to inputData to DLL
                styleTransferOpenVINO.UploadTexture(inputData);

                // Load the new image data from the DLL to the texture
                inputTex.LoadRawTextureData(inputData);
                // Apply the changes to the texture
                inputTex.Apply();

                Graphics.Blit(inputTex, rTex);

                OpenVINOUtils.FlipImage(imageProcessingShader, rTex, "FlipXAxis");

                Graphics.Blit(rTex, source);
            }
        }

        Graphics.Blit(source, destination);
    }

    

    public void OnGUI()
    {
        if (performInference)
        {

            if (yoloxActive)
            {
                foreach (YOLOXUtils.Object objectInfo in cocoYOLOX.objectInfoArray)
                {
                    Rect boxRect = new Rect(objectInfo.x0,
                            Screen.height - objectInfo.y0,
                            objectInfo.width,
                            objectInfo.height);

                    Rect labelRect = boxRect;
                    labelRect.y -= 30;

                    Color color = COCOClasses.coco_classes[objectInfo.label].Item2;
                    string name = COCOClasses.coco_classes[objectInfo.label].Item1;

                    GUIStyle style = new GUIStyle();
                    style.fontSize = (int)(Screen.width * 11e-3); ;
                    style.normal.textColor = color;

                    string labelText = $"{name}: {(objectInfo.prob * 100).ToString("0.##")}%";
                    GUI.Label(labelRect, new GUIContent(labelText), style);

                    int lineWidth = (int)(Screen.width * 1.75e-3);
                    GUI.DrawTexture(boxRect, boxTex, ScaleMode.StretchToFill,
                        true, 0, color, 3, lineWidth);
                }
            }
            
        }
    }
}
