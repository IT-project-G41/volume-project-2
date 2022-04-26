using UnityEngine;

public class FindClosestPointOnMesh
{
    BoundingVolumeHierarchy3D pointsTree;
    BoundingVolumeHierarchy3D TriangleTree;

    public FindClosestPointOnMesh(Mesh mesh)
    {
        BVHPoints[] points = new BVHPoints[mesh.vertices.Length];
        BVHTriangle[] triangles = new BVHTriangle[mesh.triangles.Length / 3];

        // wrap up all the verticies
        for(int index = 0; index < points.Length; index++)
        {
            points[index] = new BVHPoints(mesh.vertices[index]);
        }

        int counter = 0;
        for (int index = 0; index < mesh.triangles.Length - 2; index += 3)
        {
            try
            {
                BVHTriangle temp = new BVHTriangle(
                mesh.vertices[mesh.triangles[index]],
                mesh.vertices[mesh.triangles[index + 1]],
                mesh.vertices[mesh.triangles[index + 2]]);
                triangles[counter] = temp;
                counter++;
            }
            catch (System.IndexOutOfRangeException)
            {
                Debug.Log("outofRanges");
            }
        }

        this.pointsTree = new BoundingVolumeHierarchy3D(points);
        this.TriangleTree = new BoundingVolumeHierarchy3D(triangles);
    }

    public Vector3 GetClosestPoint(Vector3 point)
    {
        Vector3 closest = pointsTree.GetClosestPoint(point);
        return TriangleTree.GetClosestPoint(closest);
    }
}

public class BVHPoints : IBVHWraper3D
{
    readonly Vector3 point;

    public BVHPoints(Vector3 point)
    {
        this.point = point;
    }

    public Vector3 Avg()
    {
        return point;
    }

    public Vector3 GetClosest(Vector3 closest)
    {
        return point;
    }

    public Vector3 GetMax()
    {
        return point;
    }

    public Vector3 GetMin()
    {
        return point;
    }
}

// note a look up table would be cheaper but this is eaiser to comprehend atm
public class BVHTriangle : IBVHWraper3D
{
    Vector3 a, b, c;

    public BVHTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public Vector3 Avg()
    {
        return (a + b + c) / 3;
    }

    public Vector3 GetClosest(Vector3 point)
    {
        // make a plane that represents the triangle
        Plane plane = new Plane(a, b, c);
        Vector3 closestPoint = plane.ClosestPointOnPlane(point);

        // see if the point is inside of the triangle and if it is send the point
        if (PointInTriangle(a, b, c, closestPoint))
        {
            return closestPoint;
        }

        // if it isn't then we need to send the distance between the closest point on a line and the vector
        Vector3 c1 = ClosestPointOnLine(a, b, point);
        Vector3 c2 = ClosestPointOnLine(b, c, point);
        Vector3 c3 = ClosestPointOnLine(c, a, point);

        float distA = Vector3.Distance(point, c1);
        float distB = Vector3.Distance(point, c2);
        float distC = Vector3.Distance(point, c3);

        // find the smallest value
        float min = Mathf.Min(distA, distB);
        min = Mathf.Min(min, distC);

        // return  the result that related to the value
        if (min == distA)
        {
            return c1;
        }
        else if (min == distB)
        {
            return c2;
        }
        return c3;
    }

    Vector3 ClosestPointOnLine(Vector3 v1, Vector3 v2, Vector3 target)
    {
        Vector3 endA = target - v1;
        Vector3 endB = (v2 - v1).normalized;

        float d = Vector3.Distance(v1, v2);
        float t = Vector3.Dot(endB, endA);

        if (t <= 0)
            return v1;

        if (t >= d)
            return v2;

        Vector3 closestPoint = v1 + (endB * t);

        return closestPoint;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    bool PointInTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 target)
    {
        Vector3 d, e;
        float w1, w2;
        d = v2 - v1;
        e = v3 - v1;
        w1 = (e.x * (v1.y - target.y) + e.y * (target.x - v1.x)) / (d.x * e.y - d.y * e.x);
        w2 = (target.y - v1.y - w1 * d.y) / e.y;
        return (w1 >= 0.0) && (w2 >= 0.0) && ((w1 + w2) <= 1.0);
    }

    public Vector3 GetMax()
    {
        return Vector3.Max(a, Vector3.Max(b, c));
    }

    public Vector3 GetMin()
    {
        return Vector3.Min(a, Vector3.Min(b, c));
    }
}
