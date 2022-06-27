using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent (typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
public class LoadTexture3D : MonoBehaviour
{
    [SerializeField] private Texture3D stipplingTexture;
    [SerializeField] protected Shader shader;

    [Range(0f, 1f)] public float threshold = 0.5f;
    [Range(0.5f, 5f)] public float intensity = 1.5f;
    [Range(0f, 1f)] public float sliceXMin = 0.0f, sliceXMax = 1.0f;
    [Range(0f, 1f)] public float sliceYMin = 0.0f, sliceYMax = 1.0f;
    [Range(0f, 1f)] public float sliceZMin = 0.0f, sliceZMax = 1.0f;
    public Quaternion axis = Quaternion.identity;

    protected Material material;
    private bool hasStarted = false;
    // Start is called before the first frame update
    void Start()
    {
        stipplingTexture = AssetDatabase.LoadAssetAtPath("Assets/StippleTextureForDebug.asset", typeof(Texture3D)) as Texture3D;

        if(stipplingTexture == null)
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
        if (stipplingTexture != null && !hasStarted)
            this.StartMethod();

        // if the volume didn't create then don't do the rest
        if (!hasStarted) return;

        //SaveTexture((Texture2D)volume);
        // get going on with the normal stuff for each frame
        material.SetTexture("_Volume", stipplingTexture);
        material.SetFloat("_Threshold", threshold);
        material.SetFloat("_Intensity", intensity);
        material.SetVector("_SliceMin", new Vector3(sliceXMin, sliceYMin, sliceZMin));
        material.SetVector("_SliceMax", new Vector3(sliceXMax, sliceYMax, sliceZMax));
        material.SetMatrix("_AxisRotationMatrix", Matrix4x4.Rotate(axis));
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
