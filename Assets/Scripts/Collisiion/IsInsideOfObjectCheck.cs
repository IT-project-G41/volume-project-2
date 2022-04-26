using UnityEngine;

public static class IsInsideOfObjectCheck
{
    public static bool checkIfInside(MeshCollider other, Vector3 point)
    {
        Vector3 direction = new Vector3(0, 1, 0);
        Debug.DrawRay(point, direction, Color.green);
        Debug.DrawRay(point, -direction, Color.blue);
        Ray rayf = new Ray(point, direction);
        Ray rayb = new Ray(point, -direction);
        RaycastHit[] r1 = Physics.RaycastAll(rayf);
        RaycastHit[] r2 = Physics.RaycastAll(rayb);
        if (r1.Length > 0 && r2.Length > 0)
        {
            bool output = false;
            int tt, ii;
            for (tt = 0; tt < r1.Length; tt++)
            {
                if (r1[tt].collider == other)
                {
                    output = true;
                }
            }
            if (output)
            {
                output = false;
                for (ii = 0; ii < r2.Length; ii++)
                {
                    if (r2[ii].collider == other)
                    {
                        output = true;
                    }
                }
            }

            return output;
        }

        else return false;
    }

    public static bool IsInsideMeshCollider(MeshCollider col, Vector3 point)
    {
        var temp = Physics.queriesHitBackfaces;
        Ray ray = new Ray(point, Vector3.back);

        bool hitFrontFace = false;
        RaycastHit hit = default;

        Physics.queriesHitBackfaces = true;
        bool hitFrontOrBackFace = col.Raycast(ray, out RaycastHit hit2, 100f);
        if (hitFrontOrBackFace)
        {
            Physics.queriesHitBackfaces = false;
            hitFrontFace = col.Raycast(ray, out hit, 100f);
        }
        Physics.queriesHitBackfaces = temp;

        if (!hitFrontOrBackFace)
        {
            return false;
        }
        else if (!hitFrontFace)
        {
            return true;
        }
        else
        {
            // This can happen when, for instance, the point is inside the torso but there's a part of the mesh (like the tail) that can still be hit on the front
            if (hit.distance > hit2.distance)
            {
                return true;
            }
            else
                return false;
        }

    }

    public static bool IsInCollider(MeshCollider other, Vector3 point)
    {
        Vector3 from = (Vector3.up * 5000f);
        Vector3 dir = (point - from).normalized;
        float dist = Vector3.Distance(from, point);
        //fwd      
        int hit_count = Cast_Till(from, point, other);
        //back
        dir = (from - point).normalized;
        hit_count += Cast_Till(point, point + (dir * dist), other);

        if (hit_count % 2 == 1)
        {
            return true;
        }
        return false;
    }

    static int Cast_Till(Vector3 from, Vector3 to, MeshCollider other)
    {
        int counter = 0;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);
        Debug.DrawRay(from, dir);
        bool Break = false;
        while (!Break)
        {
            Break = true;
            RaycastHit[] hit = Physics.RaycastAll(from, dir, dist);
            for (int tt = 0; tt < hit.Length; tt++)
            {
                if (hit[tt].collider == other)
                {
                    counter++;
                    from = hit[tt].point + dir.normalized * .001f;
                    dist = Vector3.Distance(from, to);
                    Break = false;
                    break;
                }
            }
        }
        return (counter);
    }
}
