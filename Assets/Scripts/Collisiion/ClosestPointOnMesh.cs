using UnityEngine;

public class ClosestPointOnMesh
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mesh">
    /// The mesh that we are looking for the closet point on
    /// </param>
    /// <param name="targetPoint">
    /// The Point in the mesh that we are looking for. 
    /// </param>
    /// <returns>
    /// Will return Vecto3.Positive Infinity if a invalid mesh is sent to it as a parameter
    /// </returns>
    public Vector3 FindClosestPointOnMesh(Mesh mesh, Vector3 targetPoint)
    {
        if (mesh == null || mesh.vertices == null || mesh.vertices.Length < 0)
            return Vector3.positiveInfinity;


        Vector3 ClosestPoint = mesh.vertices[0];
        float shortestDistance = Vector3.Distance(ClosestPoint, targetPoint);

        //Iterate over your vertices and store the distance of that vertex from your point.
        for (int index = 1; index < mesh.vertices.Length; index++)
        {
            float dist = Vector3.Distance(mesh.vertices[index], targetPoint);

            //Store the smallest such distance you find as your best candidate.
            if (shortestDistance > dist)
            {
                ClosestPoint = mesh.vertices[index];
                shortestDistance = dist;
            }
        }

        //Iterate through your triangles and, 
        for (int index = 0; index < mesh.triangles.Length; index += 3)
        {
            //if the triangle has no vertex closer than this best candidate distance plus your bounding radius, skip over it.
            if (triangleIsInRange(targetPoint,
                mesh.vertices[mesh.triangles[index]],
                mesh.vertices[mesh.triangles[index + 1]],
                mesh.vertices[mesh.triangles[index + 2]],
                shortestDistance))
            {
                // if it is in range then see if the closest point is closer than 
                // our best found solution

                Vector3 other = ClosestPointOnTriangle(targetPoint,
                mesh.vertices[mesh.triangles[index]],
                mesh.vertices[mesh.triangles[index + 1]],
                mesh.vertices[mesh.triangles[index + 2]]);

                float dist = Vector3.Distance(other, targetPoint);

                // if the distance is less than our current save it,
                if (shortestDistance > dist)
                {
                    shortestDistance = dist;
                    ClosestPoint = other;
                }
            }
        }

        return ClosestPoint;
    }

    // tells the system if a trinagle is in a certain range
    bool triangleIsInRange(Vector3 target, Vector3 a, Vector3 b, Vector3 c, float minDistance, float tolerance = 0.001f)
    {
        float threashold = minDistance + tolerance;


        return threashold < Vector3.Distance(a, target) ||
            threashold < Vector3.Distance(b, target) ||
            threashold < Vector3.Distance(c, target);
    }

    private Vector3 ClosestPointOnTriangle(Vector3 point, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        // make a plane that represents the triangle
        Plane plane = new Plane(v1, v2, v3);
        Vector3 closestPoint = plane.ClosestPointOnPlane(point);

        // see if the point is inside of the triangle and if it is send the point
        if (PointInTriangle(v1, v2, v3, closestPoint))
        {
            return closestPoint;
        }

        // if it isn't then we need to send the distance between the closest point on a line and the vector
        Vector3 c1 = ClosestPointOnLine(v1, v2, point);
        Vector3 c2 = ClosestPointOnLine(v2, v3, point);
        Vector3 c3 = ClosestPointOnLine(v3, v1, point);

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
}
