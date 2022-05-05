using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LaplaceOperator : MonoBehaviour
{
    [SerializeField]
    DicomGrid dataObject = new DicomGrid();

    public enum TypeofVolume { PVM, DCM };
    public TypeofVolume typeofVolume;

    [SerializeField]
    private string file = "TestDicomData";

    [SerializeField] private Texture3D initTexture3D;

    private RawImage _renderer;
    private Texture _result;

    [SerializeField]
    private ComputeShader computeShader;
    
    public int high;
    public int low;

    [SerializeField] protected Shader shader;
    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0.5f, 5f)] public float intensity = 1.5f;
    [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
    [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
    [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
    public Quaternion axis = Quaternion.identity;

    protected Material material;
    private bool hasStarted = false;

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
    }

    protected virtual void Start()
    {
        if (initTexture3D == null)
            initTexture3D = GetAsTexture3DOneByte(low, high);

        // if there is a volume then start to render the object
        if (initTexture3D != null)
        {
            this.StartMethod();
        }

        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Comppute Shader is not support.");
            return;
        }
        else
        {
            Debug.Log("Start Laplace Operator 1");
        }

        CreateRenderTexture();
        if (_result == null)
        {
            Debug.Log("No Texture3D");
        }
        else
        {
            this.StartMethod();
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

    // Update is called once per frame
    void LateUpdate()
    {
        // if there is a volume then start to render the object
        if (_result != null && !hasStarted)
            this.StartMethod();

        // if the volume didn't create then don't do the rest
        if (!hasStarted) return;

        //SaveTexture((Texture2D)volume);
        // get going on with the normal stuff for each frame
        material.SetTexture("_Volume", _result);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_Intensity", intensity);
        material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
        material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
        material.SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
    }

    void CreateRenderTexture()
    {
        RenderTexture result_1 = new RenderTexture(255, 255, 0, RenderTextureFormat.ARGB32);
        result_1.volumeDepth = 255;
        result_1.enableRandomWrite = true;
        result_1.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        result_1.filterMode = FilterMode.Point;
        result_1.wrapMode = TextureWrapMode.Clamp;
        result_1.useMipMap = false;
        result_1.Create();

        var result2D = new RenderTexture(255, 255, 0, RenderTextureFormat.ARGB32);
        result2D.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
        result2D.enableRandomWrite = true;
        result2D.Create();

        RenderTexture result3D = new RenderTexture(255, 255, 0, RenderTextureFormat.ARGB32);
        result3D.volumeDepth = 255;
        result3D.enableRandomWrite = true;
        result3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        result3D.filterMode = FilterMode.Point;
        result3D.wrapMode = TextureWrapMode.Clamp;
        result3D.useMipMap = false;
        result3D.Create();

        var kernelIndex = computeShader.FindKernel("LaplaceOperator");
        ThreadSize threadSize = new ThreadSize();
        computeShader.GetKernelThreadGroupSizes(kernelIndex, out threadSize.x, out threadSize.y, out threadSize.z);
        computeShader.SetTexture(kernelIndex, "Texture", initTexture3D);
        computeShader.SetTexture(kernelIndex, "Result_1", result_1);
        computeShader.Dispatch(kernelIndex, 255 / (int)threadSize.x, 255 / (int)threadSize.y, 255 / (int)threadSize.z);

        computeShader.SetTexture(kernelIndex, "Result2D", result2D);
        computeShader.SetTexture(kernelIndex, "Result3D", result3D);

        // Get value from compute shader for debug
        int bufferCount = 10;
        ComputeBuffer computeBuffer = new ComputeBuffer(bufferCount, sizeof(float));
        float[] bufferResult = new float[bufferCount];
        computeBuffer.SetData(bufferResult);
        computeShader.SetBuffer(kernelIndex, "buffer", computeBuffer);

        computeShader.Dispatch(kernelIndex, 255 / (int)threadSize.x, 255 / (int)threadSize.y, 255 / (int)threadSize.z);

        //_result = result_1;

        Texture3D texture = new Texture3D(255, 255, 255, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };

        //AssetDatabase.CreateAsset(result2D, "Assets/Result2D.asset");
        //AssetDatabase.CreateAsset(result3D, "Assets/Result3D.asset");
        AssetDatabase.CreateAsset(result_1, "Assets/LaplaceTextureForDebug.asset");

        computeBuffer.GetData(bufferResult);
        for(int i = 0; i < bufferCount; i++)
        {
            Debug.Log(bufferResult[i]);
        }

        computeBuffer.Release();
    }

    private void StartMethod()
    {
        material = new Material(shader);
        material.renderQueue = 3000;
        GetComponent<MeshFilter>().sharedMesh = Build();
        GetComponent<MeshRenderer>().sharedMaterial = material;
        hasStarted = true;
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
        mesh.RecalculateNormals();  // 重新计算法线方向
        mesh.hideFlags = HideFlags.HideAndDontSave;

        return mesh;
    }
}
