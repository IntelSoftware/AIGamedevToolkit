using System.Runtime.InteropServices;


namespace AIGamedevToolkit
{
    [System.Serializable]
    public class InferenceFeatureObjectDetection2D : InferenceFeatureVision
    {
        public ObjectDetectionClassList classList;

        public bool displayBoundingBoxes = true;

        // Stores information about detected obejcts
        public Object[] objectInfoArray;


        // Indicate that the members of the struct are laid out sequentially
        [StructLayout(LayoutKind.Sequential)]
        /// <summary>
        /// Stores the information for a single object
        /// </summary> 
        public struct Object
        {
            // The X coordinate for the top left bounding box corner
            public float x0;
            // The Y coordinate for the top left bounding box cornder
            public float y0;
            // The width of the bounding box
            public float width;
            // The height of the bounding box
            public float height;
            // The object class index for the detected object
            public int label;
            // The model confidence score for the object
            public float prob;

            public Object(float x0, float y0, float width, float height, int label, float prob)
            {
                this.x0 = x0;
                this.y0 = y0;
                this.width = width;
                this.height = height;
                this.label = label;
                this.prob = prob;
            }
        }
    }
}
