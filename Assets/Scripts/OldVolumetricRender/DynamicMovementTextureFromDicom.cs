using UnityEngine;

public class DynamicMovementTextureFromDicom : MonoBehaviour
{
    public enum Orientation
    {
        EastOrWest,
        NorthOrSouth,
        UpOrDown
    }

    public Orientation orientation = Orientation.EastOrWest;

    public string filePathToImages = "ImagesForTestingMesh";

    public ConstrainMovement constrainMovement;

    public ConstrainMovement.RangePoints DirectionToConstrainPoints;

    public Material otherMaterial;

    private Texture2D texture;

    public DicomData GridBasedData = null;

    int lastIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        // set the material made before
        texture = GridBasedData.IntializeTextureFor(this.orientation);
        otherMaterial.mainTexture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        // work out the percentage value required
        float percentageValue = constrainMovement.GetDistanceToRangeAsPercentage(DirectionToConstrainPoints);

        // Check to make sure the value isn't outside of valid reason
        if (percentageValue < 0)
            percentageValue = 0;
        else if (percentageValue > 1)
            percentageValue = 1;

        // get the current index
        int newIndex = (int)System.Math.Round((1 - percentageValue) * (GridBasedData.GetLengthFor(this.orientation) - 1));

        // if the index has changed update the texture
        if (newIndex != lastIndex)
        {
            // work out what percentage the index is and use that as the new texture
            texture.SetPixels32(GridBasedData.GetSliceFor(newIndex, this.orientation));

            // update the new index
            lastIndex = newIndex;

            // apply the changes to the texture
            texture.Apply();
        }
    }

    /// <summary>
    /// Gets the required resolution for this object
    /// </summary>
    /// <returns>
    /// A float array with a size of two. 
    /// The first value (0) with be the x value 
    /// and the second value(1) will contrain the y value
    /// </returns>
    public float[] GetResultion()
    {
        return GridBasedData.GetResolutionFor(this.orientation);
    }

    /// <summary>
    /// Gets the physical distance in mm for this object
    /// </summary>
    /// <returns>
    /// This will return a float primitive that is greater than zero unless it is invalid.
    /// the value -1 is reserved for invalid orientations
    /// </returns>
    public float GetPhysicalDistanceTraveled()
    {
        return GridBasedData.getDistanceOf(this.orientation);
    }

    /// <summary>
    /// Sets the constrain movement to in the area that is approiate for this value
    /// </summary>
    /// <param name="value">
    /// a float value for the distance that can be traveled across this object 
    /// between -value and +value.
    /// </param>
    public void SetConstrainMovement(float value)
    {
        switch (orientation)
        {
            case Orientation.NorthOrSouth:
                constrainMovement.RangeOnZ = value;
                break;
            case Orientation.EastOrWest:
                constrainMovement.RangeOnX = value;
                break;
            case Orientation.UpOrDown:
                constrainMovement.RangeOnY = value;
                break;
        }
    }
}
