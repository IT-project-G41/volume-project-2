using UnityEngine;

public class TestSegmentionAlgorithm : MonoBehaviour
{
    public BasicVolumeManager volRen;

    private bool didRan = false;

    // Start is called before the first frame update
    void Update()
    {
        if (volRen.volumeInfo != null && volRen.volumeInfo.borderPixels == null)
        {
            volRen.volumeInfo.SegmentDicom(10);
        }
        else
        {
            Debug.Log(volRen.volumeInfo.borderPixels.Length);
        }
        
    }
}
