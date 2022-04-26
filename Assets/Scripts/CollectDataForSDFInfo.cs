using UnityEngine;

public class CollectDataForSDFInfo : MonoBehaviour
{
    // 
    [Header("Target")]
    public Transform Target;
    public Material TargetColor;
    public float TargetRadius;

    //
    [Header("PositionPlacer")]
    public Transform PositionPlacer;
    public Material PositionPlacerColor;
    public float PositionPlacerRadius;

    //
    [Header("UserObject")]
    public Transform UserObjectPlacement;
    public Transform UserObjectRot;
    public Material UserObjectPlacementColor;
    public Material UserObjectRotColor;
    public float UserObjectRadius;

    [Header("Linking Cylinder Details")]
    [SerializeField]
    private Material cylinderColor;
    public Color CylinderColor { get => cylinderColor.color; }
    public float cylinderRadius;

    public SDFEclipeData SDFSphereDataForTarget()
    {
        Vector3 localCords = this.transform.InverseTransformPoint(Target.position);
        return new SDFEclipeData(localCords, TargetColor.color, TargetRadius, this.transform.localScale);
    }

    public SDFEclipeData SDFSphereDataForPositionPlacer()
    {
        Vector3 localCords = this.transform.InverseTransformPoint(PositionPlacer.position);
        return new SDFEclipeData(localCords, PositionPlacerColor.color, PositionPlacerRadius, this.transform.localScale);
    }

    public SDFEclipeData SDFSphereDataForUserObject()
    {
        Vector3 localCords;
        Color col;

        if (UserObjectPlacement.gameObject.activeSelf)
        {
            localCords = this.transform.InverseTransformPoint(UserObjectPlacement.position);
            col = UserObjectPlacementColor.color;
        }
        else
        {
            localCords = this.transform.InverseTransformPoint(UserObjectRot.position);
            col = UserObjectRotColor.color;
        }

        return new SDFEclipeData(localCords, col, UserObjectRadius, this.transform.localScale);
    }

    /// <summary>
    /// Returns all functions output as a array
    /// </summary>
    /// <returns>
    /// 0 == Target
    /// 1 == Position Placer
    /// 2 == User Object
    /// </returns>
    public SDFEclipeData[] GetAsArray()
    {
        return new SDFEclipeData[]
        {
            SDFSphereDataForTarget(),
            SDFSphereDataForPositionPlacer(),
            SDFSphereDataForUserObject()
        };
    }
}

public class SDFLinkingCylinderData
{
    int indexOfStart;
    int indexOfEnd;
    float radius;
    Color color;

    public SDFLinkingCylinderData(int indexOfStart, int indexOfEnd, float radius, Color color)
    {
        this.indexOfEnd = indexOfEnd;
        this.indexOfStart = indexOfEnd;
        this.radius = radius;
        this.color = color;
    }
}

public class SDFSphereData
{
    public Vector4 positionAndRadius;
    public Color32 Color;

    public SDFSphereData(Vector3 localPosition, Color32 color, float radius)
    {
        this.Color = color;
        this.positionAndRadius = new Vector4
        (
            localPosition.x,
            localPosition.y,
            localPosition.z,
            radius
        );
    }
}


public class SDFEclipeData
{
    public Vector3 position;
    public Vector3 Radius;
    public Color Color;

    public SDFEclipeData(Vector3 localPosition, Color32 color, float radius, Vector3 scale)
    {
        this.Color = color;

        this.position = new Vector3
        (
            (localPosition.x),
            (localPosition.y),
            (localPosition.z)
        );

        this.Radius = new Vector3
        (
            radius / scale.x,
            radius / scale.y,
            radius / scale.z
        );
    }
}