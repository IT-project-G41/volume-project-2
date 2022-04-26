using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingRenderer : MonoBehaviour
{
    public SetUpLerpBasaedVolume data;

    public enum MARCHING_MODE { CUBES, TETRAHEDRON };

    public Material m_material;

    public MARCHING_MODE mode = MARCHING_MODE.CUBES;

    List<GameObject> meshes = new List<GameObject>();

    public int toleranceMin;
    public int toleranceMax;

    // Start is called before the first frame update
    void Start()
    {
        MarchingCubesProject.Marching marching = null;
        if (mode == MARCHING_MODE.TETRAHEDRON)
            marching = new MarchingCubesProject.MarchingTertrahedron(toleranceMin, toleranceMax);
        else
            marching = new MarchingCubesProject.MarchingCubes(toleranceMin, toleranceMax);

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(data.volumeInfo.buffer, data.volumeInfo.width, data.volumeInfo.height, data.volumeInfo.breath, ref verts, ref indices);


        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = m_material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = new Vector3(-data.volumeInfo.width / 2, -data.volumeInfo.height / 2, -data.volumeInfo.breath / 2);

            meshes.Add(go);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
