using System.Text;
using UnityEngine;

public class ClosetPointOnVolumeData
{
    [SerializeField]
    Vector3Int originPosition;

    [SerializeField]
    Vector3 relitiveOriginPosition;

    [SerializeField]
    Vector3Int voxelPositionOfClosestPoint;

    [SerializeField]
    Vector3 reltivePositionOfClosestPoint;

    [SerializeField]
    float distanceAwayFromOrigin;

    public ClosetPointOnVolumeData(Vector3Int voxelPosition, Vector3 relitivePosition, float distanceAwayFromOrigin, Vector3Int originVoxel, Vector3 relitiveOriginPosition)
    {
        this.voxelPositionOfClosestPoint = voxelPosition;
        this.reltivePositionOfClosestPoint = relitivePosition;
        this.distanceAwayFromOrigin = distanceAwayFromOrigin;
        this.originPosition = originVoxel;
        this.relitiveOriginPosition = relitiveOriginPosition;
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }

    public string ToCSV(string deliminator = ",")
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(this.originPosition);
        sb.Append(deliminator);
        sb.Append(this.relitiveOriginPosition);
        sb.Append(deliminator);
        sb.Append(this.voxelPositionOfClosestPoint);
        sb.Append(deliminator);
        sb.Append(this.reltivePositionOfClosestPoint);
        sb.Append(deliminator);
        sb.Append(this.distanceAwayFromOrigin);
        sb.Append(deliminator);
        sb.AppendLine();

        return sb.ToString();
    }

    public static string GetCSVHeader(string deliminator = ",")
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("originPosition");
        sb.Append(deliminator);
        sb.Append("relitiveOriginPosition");
        sb.Append(deliminator);
        sb.Append("voxelPositionOfClosestPoint");
        sb.Append(deliminator);
        sb.Append("reltivePositionOfClosestPoint");
        sb.Append(deliminator);
        sb.Append("distanceAwayFromOrigin");
        sb.Append(deliminator);
        sb.AppendLine();

        return sb.ToString();
    }

}
