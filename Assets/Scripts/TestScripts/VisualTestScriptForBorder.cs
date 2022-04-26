using UnityEngine;

public class VisualTestScriptForBorder : MonoBehaviour
{
    [Range(0, 5000)]
    public uint borderPos = 30;

    public BasicVolumeManager data;
    public RGBVolumeRendering renderer;

    public GameObject collisionTestGameObject;

    public GameObject collisionGuideObject;


    public int indexToTest;
    public Transform IndexTestObject;
    public Vector3Int TestVertex;
    public Transform Vector3TestObject;

    void Update()
    {
        if (data.volumeInfo != null && data.volumeInfo.borderPixels == null && Time.frameCount < 2)
        {
            data.volumeInfo.SegmentDicom(borderPos);

            Debug.Log("Amount Of Segments: " + data.volumeInfo.amountOfSegments);
            Debug.Log("LargestSegment: " + data.volumeInfo.largestSegment);
            Debug.Log("sizeofLargest: " + data.volumeInfo.sizeOfLargestSegment);

            if (data.volumeInfo.definition.Length == data.volumeInfo.buffer.Length && data.volumeInfo.borderPixels != null)
            {
                //
                Texture3D texture = new Texture3D(data.volumeInfo.width, data.volumeInfo.height, data.volumeInfo.breath, TextureFormat.ARGB32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Point,
                    anisoLevel = 0
                };

                // create the colors for the texture;
                Color32[] colors = new Color32[data.volumeInfo.definition.Length];
                // loop though all of the colors and give them each 1 for every differnt case
                for(int index = 0; index < data.volumeInfo.definition.Length; index++)
                {
                    switch (data.volumeInfo.definition[index])
                    {
                        case DicomGrid.TypeOfVoxel.border:
                            if (this.data.volumeInfo.RelatedSegments[index] != null && DicomGrid.ArrayContains(this.data.volumeInfo.largestSegment, this.data.volumeInfo.RelatedSegments[index]))
                            {
                                colors[index] = Color.blue;
                            }
                            break;
                        case DicomGrid.TypeOfVoxel.inside:
                            if (this.data.volumeInfo.RelatedSegments[index] != null && DicomGrid.ArrayContains(this.data.volumeInfo.largestSegment, this.data.volumeInfo.RelatedSegments[index]))
                            {
                                colors[index] = Color.clear;
                            }
                            else
                            {
                                colors[index] = Color.clear;
                            }
                            break;
                        case DicomGrid.TypeOfVoxel.outside:
                            colors[index] = Color.clear;
                            break;
                        default:
                            colors[index] = Color.clear;
                            break;
                    }
                }

                // set the information to the texture
                texture.SetPixels32(colors);
                texture.Apply();

                // apply it to the volume
                renderer.volume = texture;
            }
        }
        else if (Time.frameCount > 3)
        {
            /*
            Vector3Int randomPos = new Vector3Int(
                    Mathf.RoundToInt(Random.Range(0, data.volumeInfo.width)),
                    Mathf.RoundToInt(Random.Range(0, data.volumeInfo.height)),
                    Mathf.RoundToInt(Random.Range(0, data.volumeInfo.breath))
                    );
                    */
            Vector3Int randomPos = this.data.volumeInfo
       .GetFromPercentage(this.collisionGuideObject.transform.localPosition + new Vector3(0.5f, 0.5f, 0.5f));

            Vector3 pos = data.volumeInfo.BruteForceGetNearestBorderAsPercenatage(
                randomPos
                    );

            // Speed test for collision
            Debug.Log(pos);

            // position test for for collision
            collisionTestGameObject.transform.localPosition = pos - new Vector3(0.5f, 0.5f, 0.5f);
            //this.collisionGuideObject.transform.localPosition = data.volumeInfo.GetAsPercentage(randomPos) - new Vector3(0.5f, 0.5f, 0.5f);
            if (randomPos.Equals(data.volumeInfo.GetFromPercentage(data.volumeInfo.GetAsPercentage(randomPos))))
            {
                Debug.LogError(randomPos + ", Percentage: " + data.volumeInfo.GetAsPercentage(randomPos) + ", Intversion: " + data.volumeInfo.GetFromPercentage(data.volumeInfo.GetAsPercentage(randomPos)));
            }
        }

        IndexTestObject.localPosition = data.volumeInfo.GetAsPercentage(data.volumeInfo.GetPosition(indexToTest));
        Vector3TestObject.localPosition = this.data.volumeInfo.GetAsPercentage(TestVertex);
    }
}
