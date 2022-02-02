using UnityEngine;


// Only use the Unity.Barracuda namespace if the Barracuda package is installed
#if AIGAMEDEV_BARRACUDA
using Unity.Barracuda;
#endif


namespace AIGamedevToolkit
{
    /// <summary>
    /// Implements the functionality required to perform style transfer inference with Barracuda library
    /// </summary>
    public class StyleTransferBarracuda
    {
        // Only compile Barracuda code if the Barracuda package is installed
        #if AIGAMEDEV_BARRACUDA

        /// <summary>
        /// The interface used to execute the neural network
        /// </summary>
        private IWorker engine;


        public StyleTransferBarracuda()
        {
        }

        /// <summary>
        /// Initialize the Barracuda worker using the provided model asset and compute backend
        /// </summary>
        /// <param name="modelAsset">The .onnx file containing the model architecture and weights</param>
        /// <param name="workerType">Indicates which compute device and method to use for inference</param>
        public void InitializeEngine(NNModel modelAsset, WorkerFactory.Type workerType)
        {
            // Release any memory resources already allocated to the Barracuda worker
            if (engine != null) engine.Dispose();

            // Compile the model asset into an object oriented representation
            Model m_RuntimeModel = ModelLoader.Load(modelAsset);

            // Set the channel order of the compute backend to channel-first
            ComputeInfo.channelsOrder = ComputeInfo.ChannelsOrder.NCHW;
            // Create a worker that will execute the model with the selected backend
            engine = WorkerFactory.CreateWorker(workerType, m_RuntimeModel);
        }

        /// <summary>
        /// Execute the Barracuda model using the provided RenderTexture as input
        /// and store the model output in the same RenderTexture
        /// </summary>
        /// <param name="inputTexture">Contains the pixel data for the model input</param>
        public void Exectute(RenderTexture inputTexture)
        {
            // Create a Tensor of shape [1, inputTexture.height, inputTexture.width, 3]
            Tensor inputTensor = new Tensor(inputTexture, channels: 3);

            // Execute neural network with the provided input
            engine.Execute(inputTensor);
            // Get the raw model output
            Tensor prediction = engine.PeekOutput();
            // Release GPU resources allocated for the Tensor
            inputTensor.Dispose();

            // Make sure inputTexture is not the active RenderTexture
            RenderTexture.active = null;
            // Copy the model output to inputTexture
            prediction.ToRenderTexture(inputTexture);
            // Release GPU resources allocated for the Tensor
            prediction.Dispose();
        }


        /// <summary>
        /// Perform any required clean up steps
        /// </summary>
        public void CleanUp()
        {
            // Release resources allocated for the Barracuda worker
            engine.Dispose();
        }
        #endif
    }
}
