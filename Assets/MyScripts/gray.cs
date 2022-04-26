using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class gray : MonoBehaviour
{
    private Texture3D initTexture3D;

    [SerializeField] private RawImage _renderer;
    [SerializeField] private Texture _result;

    [SerializeField]
    private ComputeShader computeShader;

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
    void Start()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogError("Comppute Shader is not support.");
            return;
        }
        else
        {
            Debug.Log("Start Laplace Operator 1");
        }
        initTexture3D = AssetDatabase.LoadAssetAtPath("Assets/StippleTextureForDebug.asset", typeof(Texture3D)) as Texture3D;
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

        //AssetDatabase.CreateAsset(result2D, "Assets/Result2D.asset");
        AssetDatabase.CreateAsset(result3D, "Assets/Result3D.asset");
        //AssetDatabase.CreateAsset(result_1, "Assets/LaplaceTextureForDebug.asset");

        computeBuffer.GetData(bufferResult);
        for (int i = 0; i < bufferCount; i++)
        {
            Debug.Log(bufferResult[i]);
        }

        computeBuffer.Release();
    }

    private void StartMethod()
    {
        material = new Material(shader);
        material.renderQueue = 3000;     //将材质队列修改为3000
        GetComponent<MeshFilter>().sharedMesh = Build();
        GetComponent<MeshRenderer>().sharedMaterial = material;
        hasStarted = true;
    }

    // this function made a Mesh as the output, it re-constract the model 
    Mesh Build()
    {
        // the verties for the final cube
        // 定义一个Vector3数组，分别指向一个正方体的四个顶点
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
