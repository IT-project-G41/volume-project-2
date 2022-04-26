using UnityEngine;

public class RGBVolumeRendering : MonoBehaviour
{
    #region volume

    [SerializeField] protected Shader shader;
    protected Material material;

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

    public DicomGrid volumeInfo { get => this.dataObject; }

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

    void OnDestroy()
    {
        Destroy(material);
    }
}

