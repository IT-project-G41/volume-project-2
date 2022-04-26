using UnityEngine;

public class VaringIntensityColorVolumeRendering : MonoBehaviour
{
    const int MaxAmountOfColors = 4;

    #region volume

    [SerializeField] protected Shader shader;
    protected Material material;

    [SerializeField] Color color = Color.white;
    // this just exists for an easy way to navigate the editor 
    [SerializeField] VolumetricColorAndIntensityPicker[] colors = new VolumetricColorAndIntensityPicker[4];

    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
    [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
    [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
    public Quaternion axis = Quaternion.identity;

    public Texture volume;

    private bool hasStarted = false;

    public int high;
    public int low;

    private Color[] _colors = new Color[MaxAmountOfColors + 2];
    private float[] _density = new float[MaxAmountOfColors + 2];
    private float[] _intensity = new float[MaxAmountOfColors + 2];

    [SerializeField]
    DicomGrid dataObject = new DicomGrid();

    #endregion

    #region fileReading

    public enum TypeofVolume { PVM, DCM };
    public TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";

    #endregion

    public DicomGrid volumeInfo { get => this.dataObject; }

    public uint minSegmention = 10;

    public bool RunDepthBasedCheck = false;

    [Header("sdfInfo")]
    public CollectDataForSDFInfo sdfInfo;
    public bool renderClyinder = false;



    // Start is called before the first frame update
    void Awake()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load(file);
        if (typeofVolume == TypeofVolume.DCM)
        {
            // logic for a dicom
            dataObject = JsonUtility.FromJson<DicomGrid>(mytxtData.text);
        }
        else if (typeofVolume == TypeofVolume.PVM)
        {
            PVMData temp = JsonUtility.FromJson<PVMData>(mytxtData.text);

            dataObject.buffer = temp.data;
            dataObject.width = temp.width;
            dataObject.height = temp.height;
            dataObject.breath = temp.breath;
        }

        if (this.RunDepthBasedCheck && !volumeInfo.Segmented)
        {
            volumeInfo.SegmentDicom(minSegmention);
        }
    }

    protected virtual void Start()
    {
        volume = GetAsTexture3DOneByte(low, high);

        // set the colors in the array
        SetColorArray();

        // if there is a volume then start to render the object
        if (volume != null)
        {
            this.StartMethod();
        }
    }

    static public int Partition(VolumetricColorAndIntensityPicker[] arr, int left, int right)
    {
        float pivot;
        pivot = arr[left].density;
        int count = 0;
        while (count < arr.Length * arr.Length) // it needs the stop eventually
        {
            while (arr[left].density < pivot)
            {
                left++;
            }
            while (arr[right].density > pivot)
            {
                right--;
            }

            // we found the spot add it
            if (left < right)
            {
                VolumetricColorAndIntensityPicker temp = arr[right];
                arr[right] = arr[left];
                arr[left] = temp;
            }
            else
            {
                return right;
            }
            count++;
        }
        return 0;
    }

    private void QuickSort(VolumetricColorAndIntensityPicker[] arr, int left, int right)
    {
        int pivot;
        if (left < right)
        {
            pivot = Partition(arr, left, right);
            if (pivot > 1)
            {
                QuickSort(arr, left, pivot - 1);
            }
            if (pivot + 1 < right)
            {
                QuickSort(arr, pivot + 1, right);
            }
        }
    }

    private void StartMethod()
    {
        material = new Material(shader);
        material.renderQueue = 3000;
        GetComponent<MeshFilter>().sharedMesh = Build();
        GetComponent<MeshRenderer>().sharedMaterial = material;
        hasStarted = true;
    }

    private void SetColorArray()
    {
        QuickSort(colors, 0, colors.Length - 1);

        int loopend = colors.Length;
        if (loopend > MaxAmountOfColors)
        {
            loopend = MaxAmountOfColors;
        }

        _colors[0] = Color.clear;
        _density[0] = 20;
        _intensity[0] = 0;

        // add all of the color values in to the list
        for (int index = 0; index < loopend; index++)
        {
            _colors[index + 1] = colors[index].color;
            _density[index + 1] = colors[index].density;
            _intensity[index + 1] = colors[index].intensity;
        }

        // if the array isn't 1 then just add some default values on to the end
        for (int index = colors.Length + 1; index < _colors.Length; index++)
        {
            _colors[index] = Color.clear;
            _density[index] = 257;
            _intensity[index] = 1;
        }

    }

    protected void LateUpdate()
    {
        // if there is a volume then start to render the object
        if (volume != null && !hasStarted)
            this.StartMethod();

        // if the volume didn't create then don't do the rest
        if (!hasStarted) return;


        // set the colors in the array
        SetColorArray();

        // get going on with the normal stuff for each frame
        material.SetTexture("_Volume", volume);
        material.SetColorArray("_Colors", _colors);
        material.SetFloatArray("_Density", _density);
        material.SetFloatArray("_Intensity", _intensity);
        material.SetFloat("_Threshold", threshold);
        material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
        material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
        material.SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
        material.SetVector("aabbMin", this.volumeInfo.minAABB);
        material.SetVector("aabbMax", this.volumeInfo.maxAABB);
        material.SetFloat("_MaxDimention", this.volumeInfo.width * 2); // TODO change this

        // program in the SDF's
        SDFEclipeData[] sdfData = sdfInfo.GetAsArray();

        Vector4[] positions = new Vector4[sdfData.Length];
        Vector4[] radius = new Vector4[sdfData.Length];
        Color[] colors = new Color[sdfData.Length];

        for(int index = 0; index < sdfData.Length; index++)
        {
            positions[index] = sdfData[index].position;
            radius[index] = sdfData[index].Radius;
            colors[index] = sdfData[index].Color;
        }

        material.SetVectorArray("_SpherePos", positions);
        material.SetColorArray("_SphereColors", colors);
        material.SetVectorArray("_SphereRadius", radius);

        if (renderClyinder)
        {
            material.SetInt("_StartIndexOfCylinder", 1);
            material.SetInt("_EndIndexOfCylinder", 0); // not working
        }
        else
        {
            material.SetInt("_StartIndexOfCylinder", -1);
            material.SetInt("_EndIndexOfCylinder", -1);
        }

        material.SetColor("_CylinderColor", sdfInfo.CylinderColor);
        material.SetFloat("_CylinderRadius", sdfInfo.cylinderRadius);

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

    public Texture3D GetAsTexture3DOneByte(int lowThreashold = 0, int highThreashhold = 1500)
    {
        Texture3D texture = new Texture3D(dataObject.width, dataObject.height, dataObject.breath, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 0
        };

        Color32[] colors = new Color32[dataObject.buffer.Length];

        for (int index = 0; index < dataObject.buffer.Length; index++)
        {
            float debug = (float)(dataObject.buffer[index] - lowThreashold) / (highThreashhold - lowThreashold) * (float)byte.MaxValue;

            if (debug < byte.MinValue)
                debug = byte.MinValue;
            else if (debug > byte.MaxValue)
                debug = byte.MaxValue;

            try
            {
                colors[index] = new Color32(0, 0, 0, System.Convert.ToByte(debug));
            }
            catch (System.OverflowException ex)
            {
                Debug.Log(debug);
                colors[index] = new Color32(0, 0, 0, byte.MaxValue);
                break;
            }
        }

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        return texture;
    }

    void OnDestroy()
    {
        Destroy(material);
    }
}
