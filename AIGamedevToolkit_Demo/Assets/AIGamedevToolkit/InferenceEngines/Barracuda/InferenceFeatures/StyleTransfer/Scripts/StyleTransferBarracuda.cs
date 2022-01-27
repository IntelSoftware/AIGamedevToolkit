using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;


namespace AIGamedevToolkit
{
    public class StyleTransferBarracuda
    {
        // The interface used to execute the neural network
        private IWorker engine;


        public StyleTransferBarracuda()
        {

        }


        public void InitializeEngine(NNModel modelAsset, WorkerFactory.Type workerType)
        {
            if (engine != null) engine.Dispose();

            // Compile the model asset into an object oriented representation
            Model m_RuntimeModel = ModelLoader.Load(modelAsset);

            // Create a worker that will execute the model with the selected backend
            engine = WorkerFactory.CreateWorker(workerType, m_RuntimeModel);
        }


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



        public void CleanUp()
        {
            engine.Dispose();
        }
    }
}
