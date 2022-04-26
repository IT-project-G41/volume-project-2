using UnityEngine;

public class DicomData : MonoBehaviour
{
    public enum TypeofVolume { PVM, DCM };
    public TypeofVolume volume;

    [SerializeField]
    private string file = "TestDicomData";

    private readonly static Color32 lowestColor = new Color32(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue);
    private readonly static Color32 hightestColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

    [SerializeField]
    DicomGrid dataObject = new DicomGrid();
    Color32[][] NorthSouthBuffer = null;
    Color32[][] EastWestBuffer = null;
    Color32[][] UpDownBuffer = null;

    // Start is called before the first frame update
    void Awake()
    {
        TextAsset mytxtData = (TextAsset)Resources.Load(file);
        if (volume == TypeofVolume.DCM) {
            // logic for a dicom
            dataObject = JsonUtility.FromJson<DicomGrid>(mytxtData.text);

            NorthSouthBuffer = new Color32[dataObject.breath][];
            for (int index = 0; index < NorthSouthBuffer.Length; index++)
            {
                NorthSouthBuffer[index] = dataObject.GetXYSliceColorArray(index, 1500, 1000);
            }

            EastWestBuffer = new Color32[dataObject.width][];
            for (int index = 0; index < EastWestBuffer.Length; index++)
            {
                EastWestBuffer[index] = dataObject.GetZYSliceImageColorArray(index, 1500, 1000);
            }

            UpDownBuffer = new Color32[dataObject.height][];
            for (int index = 0; index < UpDownBuffer.Length; index++)
            {
                UpDownBuffer[index] = dataObject.GetXZSliceImageColorArray(index, 1500, 1000);
            }
        }
        else if (volume == TypeofVolume.PVM)
        {
            //Debug.Log(mytxtData.text);
            PVMData temp = JsonUtility.FromJson<PVMData>(mytxtData.text);
            //PVMData temp = JsonUtility.FromJson<PVMData>(s);

            
            dataObject.buffer = temp.data;
            dataObject.width = temp.width;
            dataObject.height = temp.height;
            dataObject.breath = temp.breath;
        }
    }

    public Texture2D IntializeTextureFor(DynamicMovementTextureFromDicom.Orientation orientation)
    {
        switch (orientation)
        {
            case DynamicMovementTextureFromDicom.Orientation.NorthOrSouth:
                return new Texture2D(dataObject.width, dataObject.height);
            case DynamicMovementTextureFromDicom.Orientation.EastOrWest:
                return new Texture2D(dataObject.breath * dataObject.GetThicknessValue(), dataObject.height);
            case DynamicMovementTextureFromDicom.Orientation.UpOrDown:
                return new Texture2D(dataObject.breath * dataObject.GetThicknessValue(), dataObject.width);
            default:
                return null;
        }
    }

    public Color32[] GetSliceFor(int index, DynamicMovementTextureFromDicom.Orientation orientation)
    {
        switch (orientation)
        {
            case DynamicMovementTextureFromDicom.Orientation.NorthOrSouth:
                return this.NorthSouthBuffer[index];
            case DynamicMovementTextureFromDicom.Orientation.EastOrWest:
                return this.EastWestBuffer[index];
            case DynamicMovementTextureFromDicom.Orientation.UpOrDown:
                return this.UpDownBuffer[index];
            default:
                return null;
        }
    }

    /// <summary>
    /// Returns the amount of slides contained within the required array
    /// </summary>
    /// <param name="orientation"></param>
    /// <returns></returns>
    public int GetLengthFor(DynamicMovementTextureFromDicom.Orientation orientation)
    {
        switch (orientation)
        {
            case DynamicMovementTextureFromDicom.Orientation.NorthOrSouth:
                return this.NorthSouthBuffer.Length;
            case DynamicMovementTextureFromDicom.Orientation.EastOrWest:
                return this.EastWestBuffer.Length;
            case DynamicMovementTextureFromDicom.Orientation.UpOrDown:
                return this.UpDownBuffer.Length;
            default:
                return -1;
        }
    }

    /// <summary>
    /// Get the physical distance the images between the range were they can sit
    /// </summary>
    /// <param name="orientation"></param>
    /// <returns>
    /// This will return a float primitive that is greater than zero unless it is invalid.
    /// the value -1 is reserved for invalid orientations
    /// </returns>
    public float getDistanceOf(DynamicMovementTextureFromDicom.Orientation orientation)
    {
        switch (orientation)
        {
            case DynamicMovementTextureFromDicom.Orientation.NorthOrSouth:
                return dataObject.PhysicalDistanceAlongZ();
            case DynamicMovementTextureFromDicom.Orientation.EastOrWest:
                return dataObject.PhysicalDistanceAlongX();
            case DynamicMovementTextureFromDicom.Orientation.UpOrDown:
                return dataObject.PhysicalDistanceAlongY();
            default:
                return -1;
        }
    }

    /// <summary>
    /// this funciton will return the apprate resultion for this image
    /// </summary>
    /// <param name="orientation"></param>
    /// <returns>
    /// This will return a a float array with the length of two.
    /// value zero will represent the x value and value one will represent the y cord
    /// will return null if the orienation is invalid
    /// </returns>
    public float[] GetResolutionFor(DynamicMovementTextureFromDicom.Orientation orientation)
    {
        switch (orientation)
        {
            case DynamicMovementTextureFromDicom.Orientation.NorthOrSouth:
                return dataObject.getXYResultion();
            case DynamicMovementTextureFromDicom.Orientation.EastOrWest:
                return dataObject.getZYResultion();
            case DynamicMovementTextureFromDicom.Orientation.UpOrDown:
                return dataObject.getXZResultion();
            default:
                return null;
        }
    }

    public Texture3D GetAsTexture3D(int lowThreashold = 0, int highThreashhold = 1500)
    {
        Texture3D texture = new Texture3D(dataObject.width, dataObject.height, dataObject.breath, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 0
        };

        Color32[] colors = new Color32[dataObject.buffer.Length];

        for (int index = 0; index < dataObject.buffer.Length; index++)
        {
            float debug = (float)(dataObject.buffer[index] - lowThreashold) / (highThreashhold - lowThreashold) * (float)byte.MaxValue;
            
            if (debug < byte.MinValue)
                debug = byte.MinValue;
            else if (debug > byte.MaxValue)
                debug = byte.MaxValue;

            try
            {
                colors[index] = new Color32(0, 0, 0, System.Convert.ToByte(debug));
            }
            catch(System.OverflowException ex)
            {
                Debug.Log(debug);
                colors[index] = new Color32(0, 0, 0, byte.MaxValue);
                break;
            }
        }

        // set the information we created before
        texture.SetPixels32(colors, 0);
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// TODO delete this
    /// </summary>
    /// <returns></returns>
    public DicomGrid GetDicomData()
    {
        return this.dataObject;
    }
}
