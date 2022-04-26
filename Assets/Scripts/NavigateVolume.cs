using UnityEngine;

public class NavigateVolume : MonoBehaviour
{
    public SetUpLerpBasaedVolume volume;

    //public Vector3 position = new Vector3(0.5f, 0.5f, 0.5f);
    public GameObject position;

    public uint tolerance;

    public GameObject marker;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(this.volume.volumeInfo.MiniumDistanceToTheExit(position, tolerance));
        Debug.Log(this.volume.volumeInfo.MiniumDistanceAndPosToTheExit(position.transform.localPosition, tolerance));
    }

    // Update is called once per frame
    void Update()
    {
        var obj = this.volume.volumeInfo.MiniumDistanceAndPosToTheExit(position.transform.localPosition, tolerance);
        Debug.Log(obj);
        marker.transform.localPosition = obj.Value;

        //Debug.Log(this.volume.volumeInfo.MiniumDistanceToTheExit(position, tolerance));
    }
}
