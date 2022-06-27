using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LaplaceOperatorV2 : MonoBehaviour
{
    #region volume

    [SerializeField] protected Shader shader;
    protected Material material;

    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0.5f, 5f)] public float intensity = 1.5f;
    [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
    [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
    [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
    public Quaternion axis = Quaternion.identity;

    public Texture volume = null;

    private bool hasStarted = false;

    public uint min, max;

    [SerializeField]
    DicomGrid dataObject = new DicomGrid();

    #endregion

    #region fileReading

    public enum TypeofVolume { PVM, DCM, SDF_Sphere };
    public StipplingVolumeManager.TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";

    #endregion


    // DicomGrid: 
    // a class that parses a file like Orange.txt to get information about the position, color, etc. of the points to be drawn,
    // which will be stored in the variable DicomGrid volumeInfo.
    public DicomGrid volumeInfo { get => this.dataObject; }




    // VolumetricColorAndIntensityPicker:
    // through the density and intensity in this class to determine the final position of the points to be displayed
    [SerializeField]
    VolumetricColorAndIntensityPicker[] Colors;


    // Write the Compute Shader code, in this shader file, the gray scale (gray) calculation and Laplace Operator for 3-Dimensions Texture
    [SerializeField] private ComputeShader laplaceComputeShader;
    struct ThreadSize
    {
        public uint x;
        public uint y;
        public uint z;

        public ThreadSize(uint x, uint y, uint z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load(file);
        if (typeofVolume == StipplingVolumeManager.TypeofVolume.DCM)
        {
            // logic for a dicom
            dataObject = JsonUtility.FromJson<DicomGrid>(mytxtData.text);
        }
        else if (typeofVolume == StipplingVolumeManager.TypeofVolume.PVM)
        {
            PVMData temp = JsonUtility.FromJson<PVMData>(mytxtData.text);

            dataObject.buffer = temp.data;
            dataObject.width = temp.width;
            dataObject.height = temp.height;
            dataObject.breath = temp.breath;
        }
    }





    protected virtual void Start()
    {
        if (volume == null)
        {
            this.volume = this.CreateStippleTexture(min, max, 255, 0.1f);
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
        material.renderQueue = 3000;     //Change the material queue to 3000
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

        //SaveTexture((Texture2D)volume);
        // get going on with the normal stuff for each frame
        material.SetTexture("_Volume", volume);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_Intensity", intensity);
        material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
        material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
        material.SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
    }






    // this function made a Mesh as the output, it re-constract the model 
    Mesh Build()
    {
        // the verties for the final cube
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







    // CreateStippleTexture():
    // which calculates the parsed dicom data and the parameters set by the user in Unity, such as Colors, 
    // to get the points drawn and the colors of the corresponding points,
    // and saves them as a Texture3-Dimensions variable for the next step of rendering
    //
    //create a RenderTexture 
    //to interact with Texture3-Dimensions, 
    //and save the processed data to the RenderTexture
    public RenderTexture CreateStippleTexture(uint minThreashold, uint maxThreashold, int resulition, float OddsOfStipple = 1f)
    {
        if (OddsOfStipple > 1)
        {
            OddsOfStipple = 1;
        }
        else if (OddsOfStipple < 0)
        {
            OddsOfStipple = 0;
        };

        Texture3D texture = new Texture3D(resulition, resulition, resulition, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };

        int length = resulition * resulition * resulition;
        Color32[] colors = new Color32[length];
        Debug.Log(length);

        for (int index = 0; index < length; index++)
        {
            // Get the position
            //int pos = new Vector3Int(index % height, (index / width) % height, index / (width * height));
            Vector3Int pos = new Vector3Int(index % resulition, (index / resulition) % resulition, index / (resulition * resulition));

            // get the pos as a percentage
            Vector3 percentage = new Vector3((float)pos.x / (float)resulition, (float)pos.y / (float)resulition, (float)pos.z / (float)resulition);
            pos = volumeInfo.GetFromPercentage(percentage);

            // get the point of the data
            uint dataAt = volumeInfo.Get(pos);

            int i = 0;
            if (Colors[i].density > dataAt)
            {
                colors[index] = Color.clear;
                continue;
            }

            // work out when 
            while (i < Colors.Length - 1 &&
                !(Colors[i].density < dataAt && Colors[i + 1].density > dataAt)
                )
            {
                i++;
            }

            // TODO create a 3D gausian calcualtor

            // TODO create a 3D Discrete Laplacian operator  // edge detetion

            // choose a color if it gets picked
            //float chance = ((float)((int)Colors[i].intensity - (int)minThreashold) / (float)(maxThreashold - minThreashold)) * OddsOfStipple;
            //float chance = Colors[i].intensity;
            //if (Random.Range(0.0f, 1f) < chance)
            if (Random.Range(0.0f, 1f) < Colors[i].intensity)
            {
                colors[index] = Colors[i].color;
            }
            else
            {
                colors[index] = Color.clear;
            }
        }

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        RenderTexture laplaceTexture = new RenderTexture(255, 255, 0, RenderTextureFormat.ARGB32);
        laplaceTexture.volumeDepth = 255;
        laplaceTexture.enableRandomWrite = true;
        // This RenderTexture is 3-Dimensions RenderTexture, 
        // can create 3-Dimensions RenderTexture by specifying the dimension of RenderTexture
        laplaceTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;  
        laplaceTexture.filterMode = FilterMode.Point;
        laplaceTexture.wrapMode = TextureWrapMode.Clamp;
        laplaceTexture.useMipMap = false;
        laplaceTexture.Create();


        // Write the Compute Shader code, in this shader file, the gray scale (gray) calculation and Laplace Operator for 3-Dimensions Texture
        var kernelIndex = laplaceComputeShader.FindKernel("LaplaceOperatorV2");
        ThreadSize threadSize = new ThreadSize();
        laplaceComputeShader.GetKernelThreadGroupSizes(kernelIndex, out threadSize.x, out threadSize.y, out threadSize.z);
        laplaceComputeShader.SetTexture(kernelIndex, "Texture", texture);
        laplaceComputeShader.SetTexture(kernelIndex, "Result", laplaceTexture);
        laplaceComputeShader.Dispatch(kernelIndex, resulition / (int)threadSize.x, resulition / (int)threadSize.y, resulition / (int)threadSize.z);

        //AssetDatabase.CreateAsset(texture, "Assets/laplaceTexture.asset");

        return laplaceTexture;
    }







    class AABBInt
    {
        public Vector3Int min;
        public Vector3Int max;

        AABBInt()
        {
            this.min = Vector3Int.zero;
            this.max = Vector3Int.zero;
        }

        AABBInt(Vector3Int max)
        {
            this.min = Vector3Int.zero;
            this.max = max;
        }

        AABBInt(Vector3Int min, Vector3Int max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
