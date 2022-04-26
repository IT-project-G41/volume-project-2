using UnityEngine;

/// <summary>
/// Depending on a range this class will itterate though a series of
/// Images that it is given access to.
/// </summary>
public class ChangeTextureBasedOnMovment : MonoBehaviour
{
    public string filePathToImages = "ImagesForTestingMesh";

    public GameObject MaterialToChange;

    public ConstrainMovement constrainMovement;

    public ConstrainMovement.RangePoints DirectionToConstrainPoints;

    private Texture[] textures;

    // Start is called before the first frame update
    void Start()
    {
        // load all the textures when this page starts up
        textures = Resources.LoadAll<Texture>(filePathToImages);
    }

    // Update is called once per frame
    void Update()
    {
        float percentageValue = constrainMovement.GetDistanceToRangeAsPercentage(DirectionToConstrainPoints);

        if (percentageValue < 0)
        {
            percentageValue = 0;
        }
        else if (percentageValue > 1)
        {
            percentageValue = 1;
        }

        int test = (int)System.Math.Round((1 - percentageValue) * (textures.Length - 1));

        Debug.Log(test);

        MaterialToChange.GetComponent<Renderer>().material.mainTexture = textures[test];
    }

    public int AmountOfTexturesAvalible()
    {
        return textures.Length;
    }
}
