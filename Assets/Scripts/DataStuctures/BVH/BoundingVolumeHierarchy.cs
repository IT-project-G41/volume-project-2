using UnityEngine;
// Not tested
public class BoundingVolumeHierarchy3D
{
    BVHNode3D root;

    // creates a new tree. this is a long process
    public BoundingVolumeHierarchy3D(IBVHWraper3D[] objects)
    {
        this.root = new BVHNode3D(objects);
    }

    // finds the closest point on the mesh to another point
    public Vector3 GetClosestPoint(Vector3 point)
    {
        return this.root.GetClosestPoint(point);
    }

    // finds the closest point on the mesh to another point
    public Vector3 GetClosestPoint(Vector3 point, Vector3 closest)
    {
        return this.root.GetClosestPoint(point);
    }
}

public interface IBVHWraper3D
{
    Vector3 GetMin();
    Vector3 GetMax();
    Vector3 Avg();
    Vector3 GetClosest(Vector3 closest);
}

class BVHNode3D
{
    public Vector3 max;
    public Vector3 min;
    public Vector3 avg;
    BVHNode3D[] children;
    IBVHWraper3D dataItem;

    public BVHNode3D(IBVHWraper3D[] objects)
    {
        if (objects == null || objects.Length == 0)
        {
            throw new System.Exception("Null exception thrown because trying to set a null array must have data");
        }
        else if (objects.Length == 1)
        {
            children = null;
            dataItem = objects[0];
            max = dataItem.GetMax();
            min = dataItem.GetMin();
            avg = dataItem.Avg();

            return;
        }

        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        avg = Vector3.zero;

        for (int index = 0; index < objects.Length; index++)
        {
            Vector3 min = objects[index].GetMin();
            Vector3 max = objects[index].GetMax();
            // fix the bounds if need be
            if (maxX < max.x) maxX = max.x;
            if (maxY < max.y) maxY = max.y;
            if (maxZ < max.z) maxZ = max.z;
            if (minX < min.x) minX = min.x;
            if (minY < min.y) minY = min.y;
            if (minZ < min.z) minZ = min.z;

            avg += objects[index].Avg();
        }

        // conform the avgerage to be an average
        avg /= objects.Length;

        // set the bounds of this node
        this.max = new Vector3(maxX, maxY, maxZ);
        this.min = new Vector3(minX, minY, minZ);

        // build the temperary segment data stuctures and initalize them
        System.Collections.Generic.Queue<IBVHWraper3D>[] segments = new System.Collections.Generic.Queue<IBVHWraper3D>[8];
        for (int index = 0; index < segments.Length; index++)
        {
            segments[index] = new System.Collections.Generic.Queue<IBVHWraper3D>();
        }

        // split the node into 8 leaves
        for (int index = 0; index < objects.Length; index++)
        {
            Vector3 avgObj = objects[index].Avg();

            if (this.avg.x < avgObj.x && this.avg.y < avgObj.y && this.avg.z < avgObj.z)
            {
                segments[0].Enqueue(objects[index]);
            }
            else if (this.avg.x >= avgObj.x && this.avg.y < avgObj.y && this.avg.z < avgObj.z)
            {
                segments[1].Enqueue(objects[index]);
            }
            else if (this.avg.x >= avgObj.x && this.avg.y >= avgObj.y && this.avg.z < avgObj.z)
            {
                segments[2].Enqueue(objects[index]);
            }
            else if (this.avg.x < avgObj.x && this.avg.y >= avgObj.y && this.avg.z < avgObj.z)
            {
                segments[3].Enqueue(objects[index]);
            }
            else if (this.avg.x < avgObj.x && this.avg.y < avgObj.y && this.avg.z >= avgObj.z)
            {
                segments[4].Enqueue(objects[index]);
            }
            else if (this.avg.x >= avgObj.x && this.avg.y < avgObj.y && this.avg.z >= avgObj.z)
            {
                segments[5].Enqueue(objects[index]);
            }
            else if (this.avg.x >= avgObj.x && this.avg.y >= avgObj.y && this.avg.z >= avgObj.z)
            {
                segments[6].Enqueue(objects[index]);
            }
            else if (this.avg.x < avgObj.x && this.avg.y >= avgObj.y && this.avg.z >= avgObj.z)
            {
                segments[7].Enqueue(objects[index]);
            }
        }

        // create the children nodes
        this.children = new BVHNode3D[8];
        for (int index = 0; index < children.Length; index++)
        {
            try
            {
                this.children[index] = new BVHNode3D(segments[index].ToArray());
            }
            catch (System.Exception)
            {
                this.children[index] = null;
            }
        }
    }

    public Vector3 GetClosestPoint(Vector3 point)
    {
        if (children == null || children.Length == 0 && this.dataItem == null)
        {
            // should be unreachable
            return GetClosestPointOnAABB(point);
        }
        else if (children == null || children.Length == 0)
        {
            return dataItem.GetClosest(point);
        }

        int index = 0;
        DistanceValues closest = null;

        // find the first value
        for (; index++ < children.Length; index++)
        {
            if (children[index] != null)
            {
                closest = new DistanceValues(
                        index,
                        children[index].GetClosestPointOnAABB(point),
                        point
                    );
            }
        }

        // if nothing was found
        if (closest == null)
        {
            // return this objects data
            return GetClosestPointOnAABB(point);
        }

        // get ready for the next loop
        index++;

        // This is a array that holds the values that are not the most likley but could contain a valid answer
        DistanceValues[] valid = new DistanceValues[this.children.Length];

        // look for the closest valid object and then look look though the children to work out a logical order
        for (; index < this.children.Length; index++)
        {
            if (children[index] != null)
            {

                // get the sorting data out for the objects
                DistanceValues other = new DistanceValues(
                        index,
                        children[index].GetClosestPointOnAABB(point),
                        point
                    );

                // save all the closest and focus on the rest later
                if (closest.dist > other.dist)
                {
                    valid[index] = closest;
                    closest = other;
                }
                else
                {
                    valid[index] = other;
                }
            }
        }

        // search via the closest index
        Vector3 closestPoint = this.children[closest.index].GetClosestPoint(point);
        float dist = Vector3.Distance(point, closestPoint);

        // sort the array so the closer checks get pirority
        System.Array.Sort<DistanceValues>(valid,
                        new System.Comparison<DistanceValues>((i1, i2) => i2.dist.CompareTo(i1.dist)));


        // loop though all the values that could have a closer value
        for (int i = 0; i < valid.Length; i++)
        {
            // if the value has been set to a real value
            if (valid[i] != null)
            {
                // if the if it is possible there is a closer match
                if (valid[i].dist < dist)
                {
                    Vector3 searchResult = children[valid[i].index].GetClosestPoint(point, closestPoint);
                    float oDist = Vector3.Distance(point, searchResult);

                    // if we found a closer point set the new point then continue
                    if (oDist < dist)
                    {
                        closestPoint = searchResult;
                        dist = oDist;
                    }
                }
            }
        }

        return closestPoint;
    }

    public Vector3 GetClosestPoint(Vector3 point, Vector3 nearestPointFound)
    {
        if (children == null || children.Length == 0)
        {
            Vector3 thisPoint = GetClosestPointOnAABB(point);

            if (Vector3.Distance(point, thisPoint) < Vector3.Distance(point, thisPoint))
            {
                return thisPoint;
            }
            else
            {
                return nearestPointFound;
            }
        }

        int index = 0;
        DistanceValues closest = null;

        // find the first value
        for (; index++ < children.Length; index++)
        {
            if (children[index] != null)
            {
                closest = new DistanceValues(
                        index,
                        children[index].GetClosestPointOnAABB(point),
                        point
                    );
            }
        }

        // if nothing was found treat this as a child
        if (closest == null)
        {
            Vector3 thisPoint = GetClosestPointOnAABB(point);

            if (Vector3.Distance(point, thisPoint) < Vector3.Distance(point, thisPoint))
            {
                return thisPoint;
            }
            else
            {
                return nearestPointFound;
            }
        }

        // get ready for the next loop
        index++;

        // This is a array that holds the values that are not the most likley but could contain a valid answer
        DistanceValues[] valid = new DistanceValues[this.children.Length];

        // look for the closest valid object and then look look though the children to work out a logical order
        for (; index < this.children.Length; index++)
        {
            if (children[index] != null)
            {

                // get the sorting data out for the objects
                DistanceValues other = new DistanceValues(
                        index,
                        children[index].GetClosestPointOnAABB(point),
                        point
                    );

                // save all the closest and focus on the rest later
                if (closest.dist > other.dist)
                {
                    valid[index] = closest;
                    closest = other;
                }
                else
                {
                    valid[index] = other;
                }
            }
        }

        // if we haven't found a point that can be closer return with this value and don't go any deeper through the tree
        if (closest.dist > Vector3.Distance(nearestPointFound, point))
        {
            return nearestPointFound;
        }

        // search via the closest index
        Vector3 closestPoint = this.children[closest.index].GetClosestPoint(point, nearestPointFound);
        float dist = Vector3.Distance(point, closestPoint);

        // sort the array so the closer checks get pirority
        System.Array.Sort<DistanceValues>(valid,
                        new System.Comparison<DistanceValues>((i1, i2) => i2.dist.CompareTo(i1.dist)));


        // loop though all the values that could have a closer value
        for (int i = 0; i < valid.Length; i++)
        {
            // if the value has been set to a real value
            if (valid[i] != null)
            {
                // if the if it is possible there is a closer match
                if (valid[i].dist < dist)
                {
                    Vector3 searchResult = children[valid[i].index].GetClosestPoint(point, closestPoint);
                    float oDist = Vector3.Distance(point, searchResult);

                    // if we found a closer point set the new point then continue
                    if (oDist < dist)
                    {
                        closestPoint = searchResult;
                        dist = oDist;
                    }
                }
            }
        }

        return closestPoint;
    }

    public Vector3 GetClosestPointOnAABB(Vector3 other)
    {
        float ax, ay, az;

        if (other.x > this.max.x)
        {
            ax = this.max.x;
        }
        else if (other.x < this.min.x)
        {
            ax = this.min.x;
        }
        else
        {
            ax = other.x;
        }

        if (other.y > this.max.y)
        {
            ay = this.max.y;
        }
        else if (other.y < this.min.y)
        {
            ay = this.min.y;
        }
        else
        {
            ay = other.y;
        }

        if (other.z > this.max.z)
        {
            az = this.max.z;
        }
        else if (other.z < this.min.z)
        {
            az = this.min.z;
        }
        else
        {
            az = other.z;
        }

        return new Vector3(ax, ay, az);
    }

    class DistanceValues
    {
        public int index;
        public float dist;
        public Vector3 position;

        public DistanceValues(int index, Vector3 pos, Vector3 otherPoint)
        {
            this.index = index;
            this.position = pos;
            dist = Vector3.Distance(pos, otherPoint);
        }
    }
}
