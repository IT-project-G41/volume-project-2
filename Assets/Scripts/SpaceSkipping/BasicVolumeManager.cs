using UnityEngine;

public class BasicVolumeManager : MonoBehaviour
{
    #region volume

    [SerializeField] protected Shader shader;
    protected Material material;

    [SerializeField] Color color = Color.white;
    // this just exists for an easy way to navigate the editor 
    [SerializeField] VolumetricColorPicker[] colors = new VolumetricColorPicker[4];

    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0.5f, 5f)] public float intensity = 1.5f;
    [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
    [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
    [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
    public Quaternion axis = Quaternion.identity;

    public Texture volume = null;

    private bool hasStarted = false;

    [SerializeField]
    DicomGrid dataObject = new DicomGrid();

    #endregion

    #region fileReading

    public enum TypeofVolume { PVM, DCM, SDF_Sphere };
    public TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";

    [Header("Optional")]
    [SerializeField]
    private Texture DepthBasedColorMap;

    [SerializeField]
    private bool RunDepthBasedCheck = false;

    #endregion

    public DicomGrid volumeInfo { get => this.dataObject; }

    public uint minSegmention = 10;

    public uint bsfTolerance = 1;

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

        if (volume == null)
        {
            if (typeofVolume == TypeofVolume.SDF_Sphere)
            {
                volume = this.SphereTexture();
            }
            else
            {
                volume = GetAsTexture3DOneByte(1, 256);
            }

        }

        // if there is a volume then start to render the object
        if (volume != null)
        {
            this.StartMethod();
        }
        else
        {
            Debug.Log("Volume == null");
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

    protected void LateUpdate()
    {
        // if there is a volume then start to render the object
        if (volume != null && !hasStarted)
            this.StartMethod();

        // if the volume didn't create then don't do the rest
        if (!hasStarted) return;

        // get going on with the normal stuff for each frame
        material.SetTexture("_Volume", volume);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_Intensity", intensity);
        material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
        material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
        material.SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
        material.SetTexture("_ColorMap", DepthBasedColorMap);
        material.SetVector("aabbMin", this.volumeInfo.minAABB);
        material.SetVector("aabbMax", this.volumeInfo.maxAABB);
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

    public void SetVolume(Color32[] colors, FilterMode TextureFilter = FilterMode.Bilinear)
    {
        Texture3D texture = new Texture3D(dataObject.width, dataObject.height, dataObject.breath, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = TextureFilter,
            anisoLevel = 0
        };

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        this.volume = texture;

        this.StartMethod();
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
            colors[index] = GetVolumeData(index, lowThreashold, highThreashhold);
        }

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        return texture;
    }

    public Color32 GetVolumeData(int index, int lowThreashold = 0, int highThreashhold = 1500, octTree.OctTreeMedicalData octTree = null)
    {
        return GetVolumeData(index, new Vector3(1, 1, 1), lowThreashold, highThreashhold, octTree);
    }

    public Color32 GetVolumeData(int index, Vector3 Correction, int lowThreashold = 0, int highThreashhold = 1500, octTree.OctTreeMedicalData octTree = null)
    {
        float debug = (float)(dataObject.buffer[index] - lowThreashold) / (highThreashhold - lowThreashold) * (float)byte.MaxValue;

        if (debug < byte.MinValue)
            debug = byte.MinValue;
        else if (debug > byte.MaxValue)
            debug = byte.MaxValue;

        byte distanceValue = 1;
        if (RunDepthBasedCheck)
        {
            Vector3? currentPos = this.dataObject.GetPosition(index);
            if (currentPos.HasValue)
            {
                Vector3 position = currentPos.GetValueOrDefault();
                Vector3? d = null;
                // if the volume is segmented use the faster look up if it isn't then use the closer one
                if (volumeInfo.Segmented)
                {
                    DicomGrid.TypeOfVoxel type = volumeInfo.definition[volumeInfo.GetIndex(currentPos.GetValueOrDefault())];


                    switch (type)
                    {
                        case DicomGrid.TypeOfVoxel.border:
                            d = position;
                            break;
                        case DicomGrid.TypeOfVoxel.outside:
                            // convert the pos to a vector3 int to prevent floating point errors
                            Vector3Int iPos = new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));

                            // run the finder
                            if (octTree == null)
                            {
                                d = volumeInfo.GetAsPercentage(volumeInfo.BruteForceGetNearestBorder(iPos)) * 255.0f;
                            }
                            else
                            {
                                d = volumeInfo.GetAsPercentage(octTree.FindClosestBorderPos(iPos.x, iPos.y, iPos.z, Correction).ToVector3Int()) * 255.0f;
                            }

                            break;
                        case DicomGrid.TypeOfVoxel.inside:
                            // get the tolerance between various areas
                            uint value = volumeInfo.Get(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z));

                            // use breath first search to find teh closest position
                            d = dataObject.GetClosestPointWithAThreashold(position, value - bsfTolerance, value + bsfTolerance);
                            break;
                    }
                }
                else
                {
                    // look for the closest object worth more than the threashhold
                    d = dataObject.GetClosestPointWithAThreashold(position, 50);
                }

                // if something was found before
                if (d.HasValue)
                {
                    // convert a vector to a dv
                    float dv = Vector3.Distance(d.GetValueOrDefault(), position);

                    // convert the byte to a bit
                    if (dv < byte.MinValue)
                        dv = byte.MinValue;
                    else if (dv > byte.MaxValue)
                        dv = byte.MaxValue;

                    distanceValue = System.Convert.ToByte(dv);
                }
            }
        }


        try
        {
            return new Color32(distanceValue, 0, 0, System.Convert.ToByte(byte.MaxValue - debug));
        }
        catch (System.OverflowException ex)
        {
            Debug.Log(debug);
            return new Color32(distanceValue, 0, 0, byte.MaxValue);
        }
    }

    public Texture3D SphereTexture()
    {
        Texture3D texture = new Texture3D(256, 256, 256, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };

        // 
        Color32[] colors = new Color32[256 * 256 * 256];
        int index = 0;

        // 
        Vector3 center = new Vector3(128, 128, 128);
        const byte radius = 128;

        for (int z = 0; z < 256; z++)
            for (int y = 0; y < 256; y++)
                for (int x = 0; x < 256; x++)
                {
                    // work out the distance to the center and fomat it to be saved
                    Vector3 current = new Vector3(x, y, z);
                    int d = Mathf.RoundToInt(Vector3.Distance(current, center));


                    //  cacluate the distance to the object
                    byte dist;
                    if (d >= byte.MaxValue)
                    {
                        dist = System.Convert.ToByte(Mathf.Max(byte.MaxValue - radius, 1));
                    }
                    else
                    {
                        dist = System.Convert.ToByte(Mathf.Max(d - radius, 1));
                    }

                    // set color based on distance
                    if (d < 128)
                        colors[index] = new Color32(System.Convert.ToByte(128 - dist), 0, 0, 1);
                    else
                        colors[index] = new Color32(dist, 0, 0, 255);

                    index++;
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
