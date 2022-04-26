using UnityEngine;

/// <summary>
/// This is a quick test class to determine the use of the 
/// Methods and class built to use this software.
/// </summary>

[RequireComponent(typeof(VolumeRendering.VolumeRendering))]
public class TestScriptForVolumeRendering : MonoBehaviour
{
    public VolumeRendering.VolumeRendering volume;

    public DicomData dicomData;

    public int low = 1000;

    public int high = 1500;

    private void Start()
    {
        volume.volume = dicomData.GetAsTexture3D(low, high);
    }
}
