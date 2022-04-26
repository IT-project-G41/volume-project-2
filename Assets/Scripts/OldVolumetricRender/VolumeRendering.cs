using UnityEngine;

namespace VolumeRendering
{

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class VolumeRendering : MonoBehaviour
    {
        [SerializeField] protected Shader shader;
        protected Material material;

        [SerializeField] Color color = Color.white;
        // this just exists for an easy way to navigate the editor 
        [SerializeField] RangeBasedVolumetricColorPicker[] colors = new RangeBasedVolumetricColorPicker[4];

        [Range(0f, 1f)] public float threshold = 0.5f;
        [Range(0.5f, 5f)] public float intensity = 1.5f;
        [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
        [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
        [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
        public Quaternion axis = Quaternion.identity;

        public DicomData dicomData;

        public Texture volume;

        private bool hasStarted = false;

        public int high;
        public int low;

        private Color[] _colors = new Color[4];
        private float[] mins = new float[4];
        private float[] maxes = new float[4];

        protected virtual void Start()
        {
            dicomData.GetAsTexture3D(low, high);

            int loopend = colors.Length;
            if (loopend > 4)
            {
                loopend = 4;
            }

            // add all of the color values in to the list
            for (int index = 0; index < loopend; index++)
            {
                _colors[index] = colors[index].color;
                mins[index] = colors[index].min;
                maxes[index] = colors[index].max;
            }

            // if the array isn't 1 then just add some default values on to the end
            for (int index = colors.Length; index < 4; index++)
            {
                _colors[index] = this.color;
                mins[index] = 0;
                maxes[index] = 1;
            }

            // if there is a volume then start to render the object
            if (volume != null)
            {
                this.StartMethod();
            }
        }

        private void StartMethod()
        {
            hasStarted = true;
            material = new Material(shader);
            material.renderQueue = 3000;
            GetComponent<MeshFilter>().sharedMesh = Build();
            GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        protected void Update()
        {

            // if there is a volume then start to render the object
            if (volume != null && !this.hasStarted)
            {
                this.StartMethod();
            }


            int loopend = colors.Length;
            if (loopend > 4)
            {
                loopend = 4;
            }

            // add all of the color values in to the list
            for (int index = 0; index < loopend; index++)
            {
                _colors[index] = colors[index].color;
                mins[index] = colors[index].min;
                maxes[index] = colors[index].max;
            }

            // if the array isn't 1 then just add some default values on to the end
            for (int index = colors.Length; index < 4; index++)
            {
                _colors[index] = this.color;
                mins[index] = 0;
                maxes[index] = 1;
            }

            // get some stuff ready for this stuff
            if (volume == null)
                return;
            else if (hasStarted == false)
                this.StartMethod();

            // get going on with the normal stuff for each frame
            material.SetTexture("_Volume", volume);
            material.SetColor("_Color", color);
            material.SetColorArray("_Colors", _colors);
            material.SetFloatArray("_min", mins);
            material.SetFloatArray("_max", maxes);
            material.SetFloat("_Threshold", threshold);
            material.SetFloat("_Intensity", intensity);
            material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
            material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));

            material.SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
        }

        Mesh Build()
        {
            // the verties ofor the final cube
            var vertices = new Vector3[] {
                new Vector3 (-0.5f, -0.5f, -0.5f),
                new Vector3 ( 0.5f, -0.5f, -0.5f),
                new Vector3 ( 0.5f,  0.5f, -0.5f),
                new Vector3 (-0.5f,  0.5f, -0.5f),
                new Vector3 (-0.5f,  0.5f,  0.5f),
                new Vector3 ( 0.5f,  0.5f,  0.5f),
                new Vector3 ( 0.5f, -0.5f,  0.5f),
                new Vector3 (-0.5f, -0.5f,  0.5f),
            };

            // The mesh that contains the rendering.
            var triangles = new int[] {
                0, 2, 1,
                0, 3, 2,
                2, 3, 4,
                2, 4, 5,
                1, 2, 5,
                1, 5, 6,
                0, 7, 4,
                0, 4, 3,
                5, 4, 7,
                5, 7, 6,
                0, 6, 7,
                0, 1, 6
            };

            // constuct the cube the volume will be stored in
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            return mesh;
        }

        void OnValidate()
        {
            Constrain(ref sliceXMin, ref sliceXMax);
            Constrain(ref sliceYMin, ref sliceYMax);
            Constrain(ref sliceZMin, ref sliceZMax);
        }

        void Constrain(ref float min, ref float max)
        {
            const float threshold = 0.025f;
            if (min > max - threshold)
            {
                min = max - threshold;
            }
            else if (max < min + threshold)
            {
                max = min + threshold;
            }
        }

        void OnDestroy()
        {
            Destroy(material);
        }

    }

}


