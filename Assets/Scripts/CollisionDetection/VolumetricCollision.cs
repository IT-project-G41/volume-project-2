using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumetricCollision : MonoBehaviour
{
    [SerializeField]
    private Vector3 CollsionOffSet = new Vector3(0.5f, 0.5f, 0.5f);

    [Range(0, 5000)]
    public uint threashold = 30;

    public SetUpLerpBasaedVolume data;

    /// <summary>
    /// This is the object that the user will use to place thier object
    /// </summary>
    [SerializeField]
    Transform UserInteractionObject;

    /// <summary>
    /// The output object that will be saved to
    /// </summary>
    [SerializeField]
    Transform VisibleObject;

    // Update is called once per frame
    void Update()
    {
        // once it is possible try to segment the data if the data is segmented then run the normal behaviour
        if (this.data.volumeInfo.Segmented)
        {
            // work out the place the volume hits
            Vector3 newPos = new Vector3(1, 1, 1) -
                //this.data.volumeInfo.BruteForceGetNearestBorderAsPercenatageUsingOnlyLargestBorderUsingAPointFormedAsPercenatage(UserInteractionObject.localPosition + CollsionOffSet); // slow version
                this.data.volumeInfo.BruteForceGetNearestBorderAsPercenatage(data.volumeInfo.GetFromPercentage(UserInteractionObject.localPosition + CollsionOffSet));

            // sets up the visible object placement
            VisibleObject.localPosition = newPos - CollsionOffSet;

            //Debug.Log(newPos + ", Options: " + data.volumeInfo.borderPixelsOfLargestSegment.Length);
        }
        else
        {
            // segemtent the dicom based on the data
            data.volumeInfo.SegmentDicom(threashold);
        }
    }
}
